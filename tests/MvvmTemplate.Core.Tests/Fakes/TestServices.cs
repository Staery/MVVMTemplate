using Microsoft.Extensions.DependencyInjection;
using MvvmTemplate.Core.Abstractions;
using MvvmTemplate.Core.DependencyInjection;

namespace MvvmTemplate.Core.Tests.Fakes;

/// <summary>Builds the same container the application uses, with fakes for UI services.</summary>
internal static class TestServices
{
    public static ServiceProvider Create(FakeDialogService? dialogs = null) =>
        new ServiceCollection()
            .AddMvvmTemplateCore()
            .AddSingleton<IDialogService>(dialogs ?? new FakeDialogService())
            .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
}
