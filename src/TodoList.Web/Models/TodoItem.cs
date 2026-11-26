using System;

namespace TodoList.Web.Models;

/// <summary>
/// Represents a task returned by the TODO API.
/// </summary>
public sealed class TodoItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public bool IsComplete { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? DueDate { get; set; }
}

/// <summary>
/// Provides the values accepted when creating or editing a task.
/// </summary>
public sealed class TodoInput
{
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTimeOffset? DueDate { get; set; }
}
