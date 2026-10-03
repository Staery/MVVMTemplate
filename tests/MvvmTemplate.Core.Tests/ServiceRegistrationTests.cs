using Microsoft.Extensions.DependencyInjection;
using MvvmTemplate.Core.Navigation;
using MvvmTemplate.Core.Services;
using MvvmTemplate.Core.Tests.Fakes;
using MvvmTemplate.Core.ViewModels;

namespace MvvmTemplate.Core.Tests;

public sealed class ServiceRegistrationTests
{
    [Fact]
    public void Shell_navigation_and_services_are_singletons()
    {
        using var services = TestServices.Create();

        Assert.Same(services.GetRequiredService<ShellViewModel>(), services.GetRequiredService<ShellViewModel>());
        Assert.Same(services.GetRequiredService<INavigationService>(), services.GetRequiredService<INavigationService>());
        Assert.Same(services.GetRequiredService<ITodoService>(), services.GetRequiredService<ITodoService>());
    }

    [Theory]
    [InlineData(typeof(HomeViewModel))]
    [InlineData(typeof(TodoListViewModel))]
    [InlineData(typeof(SettingsViewModel))]
    public void Pages_are_transient(Type pageType)
    {
        using var services = TestServices.Create();

        Assert.NotSame(services.GetRequiredService(pageType), services.GetRequiredService(pageType));
    }

    [Fact]
    public async Task The_app_starts_with_sample_data()
    {
        using var services = TestServices.Create();

        var items = await services.GetRequiredService<ITodoService>().GetAllAsync();

        Assert.NotEmpty(items);
    }
}
