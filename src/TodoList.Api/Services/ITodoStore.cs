using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TodoList.Api.Models;

namespace TodoList.Api.Services;

public interface ITodoStore
{
    Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken cancellationToken);
    Task<TodoItem?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<TodoItem> CreateAsync(CreateTodoRequest request, CancellationToken cancellationToken);
    Task<TodoItem?> UpdateAsync(Guid id, UpdateTodoRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
