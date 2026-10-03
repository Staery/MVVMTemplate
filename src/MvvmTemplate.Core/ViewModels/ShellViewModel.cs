using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmTemplate.Core.Navigation;

namespace MvvmTemplate.Core.ViewModels;

/// <summary>
/// View model of the main window: owns the sidebar menu and exposes the page chosen by
/// <see cref="INavigationService"/>. The menu selection and the current page are kept in sync in both directions.
/// </summary>
public sealed partial class ShellViewModel : ViewModelBase
{
    private readonly INavigationService _navigation;

    [ObservableProperty]
    private NavigationItem? _selectedMenuItem;

    public ShellViewModel(INavigationService navigation)
    {
        _navigation = navigation;
        _navigation.CurrentPageChanged += OnCurrentPageChanged;

        // Add an entry here for every page that should appear in the sidebar.
        MenuItems =
        [
            new NavigationItem("Home", "Icon.Home", typeof(HomeViewModel)),
            new NavigationItem("Tasks", "Icon.Tasks", typeof(TodoListViewModel)),
            new NavigationItem("Settings", "Icon.Settings", typeof(SettingsViewModel)),
        ];
    }

    /// <summary>Application name shown in the sidebar and the window title.</summary>
    public string AppName => "MvvmTemplate";

    public IReadOnlyList<NavigationItem> MenuItems { get; }

    /// <summary>The page shown in the content area; WPF picks its view through a <c>DataTemplate</c>.</summary>
    public PageViewModel? CurrentPage => _navigation.CurrentPage;

    /// <summary>Shows the first menu entry. Called once by the composition root after the window is shown.</summary>
    public void Start() => _navigation.NavigateTo(MenuItems[0].PageType);

    [RelayCommand(CanExecute = nameof(CanGoBack))]
    private void GoBack() => _navigation.GoBack();

    private bool CanGoBack() => _navigation.CanGoBack;

    partial void OnSelectedMenuItemChanged(NavigationItem? value)
    {
        if (value is not null)
        {
            _navigation.NavigateTo(value.PageType);
        }
    }

    private void OnCurrentPageChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(CurrentPage));

        // Pages opened from code (e.g. a button on the home page) highlight their menu entry too.
        // NavigationService ignores navigation to the page that is already current, so this does not loop.
        SelectedMenuItem = MenuItems.FirstOrDefault(item => item.PageType == CurrentPage?.GetType());
        GoBackCommand.NotifyCanExecuteChanged();
    }
}
