using MvvmTemplate.Core.ViewModels;

namespace MvvmTemplate.Core.Navigation;

/// <summary>
/// View-model-first navigation: callers ask for a page view model type, the service creates it and
/// makes it the <see cref="CurrentPage"/>. The view layer decides how a page is displayed.
/// </summary>
public interface INavigationService
{
    /// <summary>The page that is currently displayed, or <see langword="null"/> before the first navigation.</summary>
    PageViewModel? CurrentPage { get; }

    /// <summary>Whether <see cref="GoBack"/> has a page to return to.</summary>
    bool CanGoBack { get; }

    /// <summary>Raised after <see cref="CurrentPage"/> has changed.</summary>
    event EventHandler? CurrentPageChanged;

    /// <summary>Navigates to a page of type <typeparamref name="TPage"/>.</summary>
    void NavigateTo<TPage>(object? parameter = null)
        where TPage : PageViewModel;

    /// <summary>Navigates to a page of the given type.</summary>
    /// <exception cref="ArgumentException"><paramref name="pageType"/> does not derive from <see cref="PageViewModel"/>.</exception>
    /// <exception cref="InvalidOperationException">The page type is not registered in the service provider.</exception>
    void NavigateTo(Type pageType, object? parameter = null);

    /// <summary>Returns to the previous page, if there is one.</summary>
    void GoBack();
}
