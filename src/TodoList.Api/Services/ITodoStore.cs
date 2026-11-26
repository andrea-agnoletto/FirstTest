using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TodoList.Api.Models;

namespace TodoList.Api.Services;

/// <summary>
/// Defines asynchronous operations for retrieving and maintaining TODO items.
/// </summary>
public interface ITodoStore
{
    /// <summary>
    /// Retrieves all TODO items in the store.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The TODO items currently in the store.</returns>
    Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a TODO item by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the item to retrieve.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The matching item, or <see langword="null"/> if it does not exist.</returns>
    Task<TodoItem?> GetAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Creates and stores a TODO item.
    /// </summary>
    /// <param name="request">The values used to create the item.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The newly created item.</returns>
    Task<TodoItem> CreateAsync(CreateTodoRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Updates a TODO item identified by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the item to update.</param>
    /// <param name="request">The values to apply to the item.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The updated item, or <see langword="null"/> if it does not exist.</returns>
    Task<TodoItem?> UpdateAsync(Guid id, UpdateTodoRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a TODO item identified by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the item to delete.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns><see langword="true"/> if an item was deleted; otherwise, <see langword="false"/>.</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
