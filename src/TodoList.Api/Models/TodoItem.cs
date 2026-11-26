using System;

namespace TodoList.Api.Models;

/// <summary>
/// Represents a task stored by the TODO application.
/// </summary>
public sealed class TodoItem
{
    /// <summary>
    /// Gets or sets the unique identifier of the task.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the task title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional notes describing the task.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets whether the task has been completed.
    /// </summary>
    public bool IsComplete { get; set; }

    /// <summary>
    /// Gets or sets when the task was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the optional due date of the task.
    /// </summary>
    public DateTimeOffset? DueDate { get; set; }
}
