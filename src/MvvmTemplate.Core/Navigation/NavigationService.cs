using MvvmTemplate.Core.ViewModels;

namespace MvvmTemplate.Core.Navigation;

/// <summary>
/// Default <see cref="INavigationService"/>. Page view models are resolved from the
/// <see cref="IServiceProvider"/>, so they get their dependencies injected; their lifetime
/// (transient or singleton) is decided at registration time.
/// </summary>
public sealed class NavigationService(IServiceProvider services) : INavigationService
{
    /// <summary>How many previous pages are remembered for <see cref="GoBack"/>.</summary>
    public const int MaxHistory = 32;

    private readonly LinkedList<PageViewModel> _history = new();

    public PageViewModel? CurrentPage { get; private set; }

    public bool CanGoBack => _history.Count > 0;

    public event EventHandler? CurrentPageChanged;

    public void NavigateTo<TPage>(object? parameter = null)
        where TPage : PageViewModel =>
        NavigateTo(typeof(TPage), parameter);

    public void NavigateTo(Type pageType, object? parameter = null)
    {
        ArgumentNullException.ThrowIfNull(pageType);

        if (!typeof(PageViewModel).IsAssignableFrom(pageType))
        {
            throw new ArgumentException($"{pageType.Name} does not derive from {nameof(PageViewModel)}.", nameof(pageType));
        }

        // Re-selecting the current page is a no-op unless the caller passes new data.
        if (CurrentPage?.GetType() == pageType && parameter is null)
        {
            return;
        }

        var page = services.GetService(pageType) as PageViewModel
            ?? throw new InvalidOperationException($"{pageType.Name} is not registered in the service collection.");

        if (CurrentPage is { } previous)
        {
            previous.OnNavigatedFrom();
            _history.AddLast(previous);
            if (_history.Count > MaxHistory)
            {
                _history.RemoveFirst();
            }
        }

        Activate(page, parameter);
    }

    public void GoBack()
    {
        if (_history.Last is not { } node)
        {
            return;
        }

        _history.RemoveLast();
        CurrentPage?.OnNavigatedFrom();
        Activate(node.Value, parameter: null);
    }

    private void Activate(PageViewModel page, object? parameter)
    {
        CurrentPage = page;
        page.OnNavigatedTo(parameter);
        CurrentPageChanged?.Invoke(this, EventArgs.Empty);
    }
}
