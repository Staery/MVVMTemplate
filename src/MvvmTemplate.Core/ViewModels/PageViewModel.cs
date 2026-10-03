namespace MvvmTemplate.Core.ViewModels;

/// <summary>
/// A view model that can be shown in the shell's content area through <see cref="Navigation.INavigationService"/>.
/// The WPF project maps every page view model to its view with a <c>DataTemplate</c>.
/// </summary>
public abstract class PageViewModel(string title) : ViewModelBase
{
    /// <summary>Title shown in the shell header.</summary>
    public string Title { get; } = title;

    /// <summary>Called after the page became the current page.</summary>
    /// <param name="parameter">Optional value passed to <see cref="Navigation.INavigationService.NavigateTo(Type, object?)"/>.</param>
    public virtual void OnNavigatedTo(object? parameter)
    {
    }

    /// <summary>Called before another page replaces this one.</summary>
    public virtual void OnNavigatedFrom()
    {
    }
}
