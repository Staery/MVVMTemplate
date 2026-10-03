namespace MvvmTemplate.Core.Navigation;

/// <summary>An entry of the shell's sidebar menu.</summary>
/// <param name="Title">Text shown in the menu.</param>
/// <param name="IconKey">Key of a <c>Geometry</c> resource in the WPF project's <c>Themes/Icons.xaml</c>.</param>
/// <param name="PageType">The page view model to navigate to.</param>
public sealed record NavigationItem(string Title, string IconKey, Type PageType);
