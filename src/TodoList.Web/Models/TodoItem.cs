using System;

namespace TodoList.Web.Models;

public sealed class TodoItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public bool IsComplete { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? DueDate { get; set; }
}

public sealed class TodoInput
{
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTimeOffset? DueDate { get; set; }
}
