using System.ComponentModel.DataAnnotations;

namespace TodoList.Api.Models;

public class CreateTodoRequest
{
    [Required, StringLength(120)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Notes { get; set; }

    public DateTimeOffset? DueDate { get; set; }
}

public sealed class UpdateTodoRequest : CreateTodoRequest
{
    public bool IsComplete { get; set; }
}
