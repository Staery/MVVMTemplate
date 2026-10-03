using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using MvvmTemplate.Core.Navigation;
using MvvmTemplate.Core.Tests.Fakes;
using MvvmTemplate.Core.ViewModels;

namespace MvvmTemplate.Core.Tests;

public sealed class ShellViewModelTests : IDisposable
{
    private readonly ServiceProvider _services = TestServices.Create();
    private readonly ShellViewModel _shell;

    public ShellViewModelTests() => _shell = _services.GetRequiredService<ShellViewModel>();

    public void Dispose() => _services.Dispose();

    [Fact]
    public void Start_shows_the_first_menu_entry_and_selects_it()
    {
        _shell.Start();

        Assert.IsType<HomeViewModel>(_shell.CurrentPage);
        Assert.Same(_shell.MenuItems[0], _shell.SelectedMenuItem);
    }

    [Fact]
    public void Selecting_a_menu_entry_navigates_to_its_page()
    {
        _shell.Start();
        var changed = new List<string?>();
        _shell.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        _shell.SelectedMenuItem = _shell.MenuItems.Single(item => item.PageType == typeof(SettingsViewModel));

        Assert.IsType<SettingsViewModel>(_shell.CurrentPage);
        Assert.Contains(nameof(ShellViewModel.CurrentPage), changed);
    }

    [Fact]
    public void Navigating_from_code_updates_the_menu_selection()
    {
        _shell.Start();
        var home = Assert.IsType<HomeViewModel>(_shell.CurrentPage);

        home.OpenTasksCommand.Execute(null);

        Assert.IsType<TodoListViewModel>(_shell.CurrentPage);
        Assert.Equal(typeof(TodoListViewModel), _shell.SelectedMenuItem?.PageType);
    }

    [Fact]
    public void GoBack_is_available_only_when_there_is_history()
    {
        _shell.Start();
        Assert.False(_shell.GoBackCommand.CanExecute(null));

        _shell.SelectedMenuItem = _shell.MenuItems[1];
        Assert.True(_shell.GoBackCommand.CanExecute(null));

        _shell.GoBackCommand.Execute(null);

        Assert.IsType<HomeViewModel>(_shell.CurrentPage);
        Assert.Same(_shell.MenuItems[0], _shell.SelectedMenuItem);
        Assert.False(_shell.GoBackCommand.CanExecute(null));
    }

    [Fact]
    public void Every_menu_entry_points_to_a_registered_page()
    {
        var navigation = _services.GetRequiredService<INavigationService>();

        foreach (var item in _shell.MenuItems)
        {
            navigation.NavigateTo(item.PageType);
            Assert.IsType(item.PageType, navigation.CurrentPage);
            Assert.Equal(item.Title, navigation.CurrentPage!.Title);
        }
    }

    [Fact]
    public void Clearing_the_selection_does_not_navigate()
    {
        _shell.Start();
        var page = _shell.CurrentPage;

        _shell.SelectedMenuItem = null;

        Assert.Same(page, _shell.CurrentPage);
    }
}
