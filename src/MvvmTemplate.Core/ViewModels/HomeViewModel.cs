using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmTemplate.Core.Models;
using MvvmTemplate.Core.Navigation;
using MvvmTemplate.Core.Services;

namespace MvvmTemplate.Core.ViewModels;

/// <summary>Start page: a short summary of the sample data and shortcuts to the other pages.</summary>
public sealed partial class HomeViewModel(
    ITodoService todoService,
    INavigationService navigation,
    TimeProvider timeProvider) : PageViewModel("Home")
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CompletionPercent))]
    private int _totalCount;

    [ObservableProperty]
    private int _activeCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CompletionPercent))]
    private int _completedCount;

    /// <summary>Greeting that depends on the local time of day.</summary>
    public string Greeting => timeProvider.GetLocalNow().Hour switch
    {
        < 5 => "Good night",
        < 12 => "Good morning",
        < 18 => "Good afternoon",
        _ => "Good evening",
    };

    /// <summary>Share of completed tasks, 0–100.</summary>
    public int CompletionPercent => TotalCount == 0 ? 0 : CompletedCount * 100 / TotalCount;

    public override void OnNavigatedTo(object? parameter) => LoadCommand.Execute(null);

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var items = await todoService.GetAllAsync();
            TotalCount = items.Count;
            CompletedCount = items.Count(item => item.IsDone);
            ActiveCount = TotalCount - CompletedCount;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void OpenTasks() => navigation.NavigateTo<TodoListViewModel>();

    /// <summary>Shows how to pass a parameter: the task list opens with the "Active" filter applied.</summary>
    [RelayCommand]
    private void OpenActiveTasks() => navigation.NavigateTo<TodoListViewModel>(TodoFilter.Active);

    [RelayCommand]
    private void OpenSettings() => navigation.NavigateTo<SettingsViewModel>();
}
