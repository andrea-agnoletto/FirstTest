using System;
using System.ComponentModel.DataAnnotations;

namespace TodoList.Api.Models;

/// <summary>
/// Provides the values required to create a task.
/// </summary>
public class CreateTodoRequest
{
    /// <summary>
    /// Gets or sets the required task title, limited to 120 characters.
    /// </summary>
    [Required, StringLength(120)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional task notes, limited to 1000 characters.
    /// </summary>
    [StringLength(1000)]
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets the optional due date for the task.
    /// </summary>
    public DateTimeOffset? DueDate { get; set; }
}

/// <summary>
/// Provides the values required to update an existing task.
/// </summary>
public sealed class UpdateTodoRequest : CreateTodoRequest
{
    /// <summary>
    /// Gets or sets whether the task is complete.
    /// </summary>
    public bool IsComplete { get; set; }
}
