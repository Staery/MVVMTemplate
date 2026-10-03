using Microsoft.Extensions.DependencyInjection;
using MvvmTemplate.Core.Navigation;
using MvvmTemplate.Core.Tests.Fakes;

namespace MvvmTemplate.Core.Tests;

public sealed class NavigationServiceTests
{
    private readonly NavigationService _navigation = new(
        new ServiceCollection()
            .AddTransient<FirstPage>()
            .AddTransient<SecondPage>()
            .BuildServiceProvider());

    [Fact]
    public void Starts_without_a_page_and_without_history()
    {
        Assert.Null(_navigation.CurrentPage);
        Assert.False(_navigation.CanGoBack);
    }

    [Fact]
    public void NavigateTo_resolves_the_page_and_raises_CurrentPageChanged()
    {
        var raised = 0;
        _navigation.CurrentPageChanged += (_, _) => raised++;

        _navigation.NavigateTo<FirstPage>();

        var page = Assert.IsType<FirstPage>(_navigation.CurrentPage);
        Assert.Equal(["to"], page.Calls);
        Assert.Equal(1, raised);
    }

    [Fact]
    public void NavigateTo_passes_the_parameter_to_the_page()
    {
        _navigation.NavigateTo<FirstPage>(42);

        var page = Assert.IsType<FirstPage>(_navigation.CurrentPage);
        Assert.Equal(42, page.LastParameter);
    }

    [Fact]
    public void NavigateTo_notifies_the_previous_page_and_records_history()
    {
        _navigation.NavigateTo<FirstPage>();
        var first = (FirstPage)_navigation.CurrentPage!;

        _navigation.NavigateTo<SecondPage>();

        Assert.Equal(["to", "from"], first.Calls);
        Assert.IsType<SecondPage>(_navigation.CurrentPage);
        Assert.True(_navigation.CanGoBack);
    }

    [Fact]
    public void NavigateTo_the_current_page_without_a_parameter_is_ignored()
    {
        _navigation.NavigateTo<FirstPage>();
        var first = _navigation.CurrentPage;
        var raised = 0;
        _navigation.CurrentPageChanged += (_, _) => raised++;

        _navigation.NavigateTo<FirstPage>();

        Assert.Same(first, _navigation.CurrentPage);
        Assert.Equal(0, raised);
        Assert.False(_navigation.CanGoBack);
    }

    [Fact]
    public void NavigateTo_the_current_page_with_a_parameter_opens_a_new_instance()
    {
        _navigation.NavigateTo<FirstPage>();
        var first = _navigation.CurrentPage;

        _navigation.NavigateTo<FirstPage>("refresh");

        Assert.NotSame(first, _navigation.CurrentPage);
        Assert.Equal("refresh", ((FirstPage)_navigation.CurrentPage!).LastParameter);
    }

    [Fact]
    public void GoBack_returns_to_the_previous_page_instance()
    {
        _navigation.NavigateTo<FirstPage>();
        var first = (FirstPage)_navigation.CurrentPage!;
        _navigation.NavigateTo<SecondPage>();
        var second = (SecondPage)_navigation.CurrentPage!;

        _navigation.GoBack();

        Assert.Same(first, _navigation.CurrentPage);
        Assert.Equal(["to", "from", "to"], first.Calls);
        Assert.Equal(["to", "from"], second.Calls);
        Assert.False(_navigation.CanGoBack);
    }

    [Fact]
    public void GoBack_without_history_does_nothing()
    {
        _navigation.NavigateTo<FirstPage>();
        var page = _navigation.CurrentPage;

        _navigation.GoBack();

        Assert.Same(page, _navigation.CurrentPage);
    }

    [Fact]
    public void History_is_limited()
    {
        for (var i = 0; i < NavigationService.MaxHistory + 10; i++)
        {
            _navigation.NavigateTo<FirstPage>(i);
        }

        var steps = 0;
        while (_navigation.CanGoBack)
        {
            _navigation.GoBack();
            steps++;
        }

        Assert.Equal(NavigationService.MaxHistory, steps);
    }

    [Fact]
    public void NavigateTo_a_type_that_is_not_a_page_throws()
    {
        Assert.Throws<ArgumentException>(() => _navigation.NavigateTo(typeof(string)));
    }

    [Fact]
    public void NavigateTo_an_unregistered_page_throws()
    {
        var error = Assert.Throws<InvalidOperationException>(() => _navigation.NavigateTo<UnregisteredPage>());
        Assert.Contains(nameof(UnregisteredPage), error.Message);
    }
}
