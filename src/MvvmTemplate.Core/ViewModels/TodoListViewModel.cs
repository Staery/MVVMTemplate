using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmTemplate.Core.Abstractions;
using MvvmTemplate.Core.Models;
using MvvmTemplate.Core.Services;

namespace MvvmTemplate.Core.ViewModels;

/// <summary>
/// Sample feature: a task list with add, remove, toggle, filter and search.
/// Shows validation with <see cref="ObservableValidator"/> attributes, async commands,
/// confirmation through <see cref="IDialogService"/> and receiving a navigation parameter.
/// </summary>
public sealed partial class TodoListViewModel : PageViewModel
{
    /// <summary>Longest title the form accepts.</summary>
    public const int MaxTitleLength = 80;

    private readonly ITodoService _todoService;
    private readonly IDialogService _dialogs;
    private readonly AppSettings _settings;
    private readonly List<TodoItemViewModel> _allItems = [];

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    [Required(ErrorMessage = "Enter a title for the task.")]
    [MaxLength(MaxTitleLength, ErrorMessage = "The title can be at most 80 characters long.")]
    [CustomValidation(typeof(TodoListViewModel), nameof(ValidateUniqueTitle))]
    private string _newTitle = string.Empty;

    [ObservableProperty]
    private TodoFilter _filter;

    [ObservableProperty]
    private string _searchText = string.Empty;

    public TodoListViewModel(ITodoService todoService, IDialogService dialogs, AppSettings settings)
        : base("Tasks")
    {
        _todoService = todoService;
        _dialogs = dialogs;
        _settings = settings;
    }

    /// <summary>Tasks that match <see cref="Filter"/> and <see cref="SearchText"/>, in creation order.</summary>
    public ObservableCollection<TodoItemViewModel> Items { get; } = [];

    /// <summary>Values for the filter selector.</summary>
    public IReadOnlyList<TodoFilter> Filters { get; } = Enum.GetValues<TodoFilter>();

    public int TotalCount => _allItems.Count;

    public int ActiveCount => _allItems.Count(item => !item.IsDone);

    public int CompletedCount => TotalCount - ActiveCount;

    public bool IsEmpty => Items.Count == 0;

    public string EmptyMessage => TotalCount == 0
        ? "No tasks yet. Add the first one above."
        : "No tasks match the current filter.";

    /// <summary>Accepts an optional <see cref="TodoFilter"/> to open the list pre-filtered.</summary>
    public override void OnNavigatedTo(object? parameter)
    {
        if (parameter is TodoFilter filter)
        {
            Filter = filter;
        }

        LoadCommand.Execute(null);
    }

    /// <summary>Validation rule used by <see cref="NewTitle"/>: titles must be unique (case-insensitive).</summary>
    public static ValidationResult? ValidateUniqueTitle(string? title, ValidationContext context)
    {
        var viewModel = (TodoListViewModel)context.ObjectInstance;
        var trimmed = title?.Trim();

        return viewModel._allItems.Any(item => string.Equals(item.Title, trimmed, StringComparison.OrdinalIgnoreCase))
            ? new ValidationResult("A task with this title already exists.")
            : ValidationResult.Success;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var items = await _todoService.GetAllAsync();

            foreach (var item in _allItems)
            {
                item.PropertyChanged -= OnItemPropertyChanged;
            }

            _allItems.Clear();
            foreach (var item in items)
            {
                _allItems.Add(Track(new TodoItemViewModel(item)));
            }

            Items.Clear();
            Refresh();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanAdd))]
    private async Task AddAsync()
    {
        ValidateProperty(NewTitle, nameof(NewTitle));
        if (!CanAdd())
        {
            return;
        }

        var item = await _todoService.AddAsync(NewTitle);
        _allItems.Add(Track(new TodoItemViewModel(item)));

        // Reset the form without showing a "required" error for the now empty field.
        NewTitle = string.Empty;
        ClearErrors(nameof(NewTitle));
        Refresh();
    }

    private bool CanAdd() => !string.IsNullOrWhiteSpace(NewTitle) && !GetErrors(nameof(NewTitle)).Any();

    [RelayCommand]
    private async Task RemoveAsync(TodoItemViewModel? item)
    {
        if (item is null)
        {
            return;
        }

        if (_settings.ConfirmDeletion &&
            !await _dialogs.ConfirmAsync("Delete task", $"Delete \"{item.Title}\"?"))
        {
            return;
        }

        await _todoService.RemoveAsync(item.Id);
        Forget(item);
        Refresh();
    }

    [RelayCommand(CanExecute = nameof(HasCompleted))]
    private async Task ClearCompletedAsync()
    {
        var completed = _allItems.Where(item => item.IsDone).ToList();

        if (_settings.ConfirmDeletion &&
            !await _dialogs.ConfirmAsync("Clear completed", $"Delete {completed.Count} completed task(s)?"))
        {
            return;
        }

        foreach (var item in completed)
        {
            await _todoService.RemoveAsync(item.Id);
            Forget(item);
        }

        Refresh();
    }

    private bool HasCompleted() => CompletedCount > 0;

    partial void OnFilterChanged(TodoFilter value) => Refresh();

    partial void OnSearchTextChanged(string value) => Refresh();

    private TodoItemViewModel Track(TodoItemViewModel item)
    {
        item.PropertyChanged += OnItemPropertyChanged;
        return item;
    }

    private void Forget(TodoItemViewModel item)
    {
        item.PropertyChanged -= OnItemPropertyChanged;
        _allItems.Remove(item);
    }

    // Event handlers are the one place where async void is appropriate; failures are reported, not thrown.
    private async void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not TodoItemViewModel item || e.PropertyName != nameof(TodoItemViewModel.IsDone))
        {
            return;
        }

        try
        {
            await _todoService.UpdateAsync(item.Model);
        }
        catch (Exception ex)
        {
            await _dialogs.ShowErrorAsync("Could not update the task", ex.Message);
        }

        Refresh();
    }

    private bool Matches(TodoItemViewModel item) =>
        Filter switch
        {
            TodoFilter.Active => !item.IsDone,
            TodoFilter.Completed => item.IsDone,
            _ => true,
        }
        && (string.IsNullOrWhiteSpace(SearchText) || item.Title.Contains(SearchText.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Brings <see cref="Items"/> in line with the filter without rebuilding it, so unchanged rows keep
    /// their containers (and focus) in the view.
    /// </summary>
    private void Refresh()
    {
        var visible = _allItems.Where(Matches).ToList();

        for (var i = Items.Count - 1; i >= 0; i--)
        {
            if (!visible.Contains(Items[i]))
            {
                Items.RemoveAt(i);
            }
        }

        // Items is now an ordered subsequence of visible: insert whatever is missing.
        for (var i = 0; i < visible.Count; i++)
        {
            if (i >= Items.Count || !ReferenceEquals(Items[i], visible[i]))
            {
                Items.Insert(i, visible[i]);
            }
        }

        OnPropertyChanged(nameof(TotalCount));
        OnPropertyChanged(nameof(ActiveCount));
        OnPropertyChanged(nameof(CompletedCount));
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(EmptyMessage));
        ClearCompletedCommand.NotifyCanExecuteChanged();
    }
}
