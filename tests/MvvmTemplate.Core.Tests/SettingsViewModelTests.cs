using Microsoft.Extensions.DependencyInjection;
using MvvmTemplate.Core.Models;
using MvvmTemplate.Core.Navigation;
using MvvmTemplate.Core.Tests.Fakes;
using MvvmTemplate.Core.ViewModels;

namespace MvvmTemplate.Core.Tests;

public sealed class SettingsViewModelTests
{
    [Fact]
    public void ConfirmDeletion_writes_through_to_the_settings_and_notifies()
    {
        var settings = new AppSettings();
        var viewModel = new SettingsViewModel(settings);
        var changed = new List<string?>();
        viewModel.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        viewModel.ConfirmDeletion = false;

        Assert.False(settings.ConfirmDeletion);
        Assert.Equal([nameof(SettingsViewModel.ConfirmDeletion)], changed);
    }

    [Fact]
    public void Shows_version_and_runtime()
    {
        var viewModel = new SettingsViewModel(new AppSettings());

        Assert.Equal("1.0.0", viewModel.Version);
        Assert.Contains(".NET", viewModel.Runtime);
    }

    [Fact]
    public async Task Settings_are_shared_between_pages()
    {
        var dialogs = new FakeDialogService();
        using var services = TestServices.Create(dialogs);
        var navigation = services.GetRequiredService<INavigationService>();

        navigation.NavigateTo<SettingsViewModel>();
        ((SettingsViewModel)navigation.CurrentPage!).ConfirmDeletion = false;

        navigation.NavigateTo<TodoListViewModel>();
        var tasks = (TodoListViewModel)navigation.CurrentPage!;
        await tasks.RemoveCommand.ExecuteAsync(tasks.Items[0]);

        Assert.Empty(dialogs.Confirmations);
        Assert.Equal(2, tasks.TotalCount);
    }
}
