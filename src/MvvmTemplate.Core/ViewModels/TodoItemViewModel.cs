using CommunityToolkit.Mvvm.ComponentModel;
using MvvmTemplate.Core.Models;

namespace MvvmTemplate.Core.ViewModels;

/// <summary>Wraps an immutable <see cref="TodoItem"/> so the view can bind to and toggle it.</summary>
public sealed partial class TodoItemViewModel(TodoItem model) : ObservableObject
{
    [ObservableProperty]
    private bool _isDone = model.IsDone;

    /// <summary>The model, kept up to date with the view model's state.</summary>
    public TodoItem Model { get; private set; } = model;

    public Guid Id => Model.Id;

    public string Title => Model.Title;

    public DateTimeOffset CreatedAt => Model.CreatedAt;

    partial void OnIsDoneChanged(bool value) => Model = Model with { IsDone = value };
}
