using System.Windows;
using MvvmTemplate.Core.ViewModels;

namespace MvvmTemplate;

/// <summary>The shell: sidebar menu on the left, the current page on the right.</summary>
public partial class MainWindow : Window
{
    public MainWindow(ShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
