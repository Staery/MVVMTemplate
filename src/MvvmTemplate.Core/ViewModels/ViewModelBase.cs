using CommunityToolkit.Mvvm.ComponentModel;

namespace MvvmTemplate.Core.ViewModels;

/// <summary>
/// Base class for all view models. Derives from <see cref="ObservableValidator"/>, so any view model can
/// use data annotations (<c>[Required]</c>, <c>[MaxLength]</c>, …) and report errors through
/// <see cref="System.ComponentModel.INotifyDataErrorInfo"/>, which WPF bindings display automatically.
/// </summary>
public abstract partial class ViewModelBase : ObservableValidator
{
    /// <summary>Set while a long-running operation is in progress; views can show a progress indicator.</summary>
    [ObservableProperty]
    private bool _isBusy;
}
