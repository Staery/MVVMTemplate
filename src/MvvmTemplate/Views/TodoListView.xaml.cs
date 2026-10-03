using System.Windows.Controls;

namespace MvvmTemplate.Views;

/// <summary>View for the page view model of the same name; the DataContext is set by its DataTemplate.</summary>
public partial class TodoListView : UserControl
{
    public TodoListView()
    {
        InitializeComponent();
    }
}
