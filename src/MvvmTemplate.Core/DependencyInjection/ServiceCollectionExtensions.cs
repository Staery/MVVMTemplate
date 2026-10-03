using Microsoft.Extensions.DependencyInjection;
using MvvmTemplate.Core.Models;
using MvvmTemplate.Core.Navigation;
using MvvmTemplate.Core.Services;
using MvvmTemplate.Core.ViewModels;

namespace MvvmTemplate.Core.DependencyInjection;

/// <summary>Registers everything from the UI-independent core. The UI project adds its own services on top.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds services, navigation and view models. The caller must also register an
    /// <see cref="Abstractions.IDialogService"/> implementation.
    /// </summary>
    public static IServiceCollection AddMvvmTemplateCore(this IServiceCollection services)
    {
        // Services
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<AppSettings>();
        services.AddSingleton<ITodoService>(provider =>
            InMemoryTodoService.CreateWithSampleData(provider.GetRequiredService<TimeProvider>()));

        // Navigation and the shell live for the whole application.
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<ShellViewModel>();

        // Pages are transient: every navigation gets a fresh instance that reloads its data.
        services.AddTransient<HomeViewModel>();
        services.AddTransient<TodoListViewModel>();
        services.AddTransient<SettingsViewModel>();

        return services;
    }
}
