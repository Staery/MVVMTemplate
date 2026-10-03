using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MvvmTemplate.Core.Abstractions;
using MvvmTemplate.Core.DependencyInjection;
using MvvmTemplate.Core.ViewModels;
using MvvmTemplate.Services;

namespace MvvmTemplate;

/// <summary>
/// Composition root. Builds a generic host (dependency injection, configuration, logging),
/// registers the core and the WPF-specific services, then shows the shell window.
/// </summary>
public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        var builder = Host.CreateApplicationBuilder();
        ConfigureServices(builder.Services);
        _host = builder.Build();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Services, navigation and view models from MvvmTemplate.Core.
        services.AddMvvmTemplateCore();

        // WPF implementations of the core abstractions.
        services.AddSingleton<IDialogService, DialogService>();

        // Windows. Page views are not registered: they are created by the DataTemplates in Views/ViewTemplates.xaml.
        services.AddSingleton<MainWindow>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += OnDispatcherUnhandledException;

        await _host.StartAsync();

        MainWindow = _host.Services.GetRequiredService<MainWindow>();
        MainWindow.Show();

        _host.Services.GetRequiredService<ShellViewModel>().Start();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        using (_host)
        {
            await _host.StopAsync(TimeSpan.FromSeconds(5));
        }

        base.OnExit(e);
    }

    private async void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        // Keep the application alive, log the error and tell the user.
        e.Handled = true;

        _host.Services.GetRequiredService<ILogger<App>>().LogError(e.Exception, "Unhandled exception");
        await _host.Services.GetRequiredService<IDialogService>()
            .ShowErrorAsync("Unexpected error", $"Something went wrong:\n\n{e.Exception.Message}");
    }
}
