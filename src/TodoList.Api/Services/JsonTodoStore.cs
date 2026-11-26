using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using TodoList.Api.Models;

namespace TodoList.Api.Services;

/// <summary>
/// Stores TODO items in a JSON file.
/// </summary>
public sealed class JsonTodoStore : ITodoStore
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    /// <summary>
    /// Initializes a JSON-backed store in the application's data directory.
    /// </summary>
    /// <param name="environment">The hosting environment used to locate the content root.</param>
    public JsonTodoStore(IWebHostEnvironment environment)
    {
        var dataDirectory = Path.Combine(environment.ContentRootPath, "Data");
        Directory.CreateDirectory(dataDirectory);
        _filePath = Path.Combine(dataDirectory, "todos.json");
    }

    /// <summary>
    /// Retrieves all items, ordering open tasks before completed tasks and then by due date.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The ordered TODO items currently in the JSON file.</returns>
    public async Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try { return (await ReadAsync(cancellationToken)).OrderBy(x => x.IsComplete).ThenBy(x => x.DueDate ?? DateTimeOffset.MaxValue).ThenByDescending(x => x.CreatedAt).ToList(); }
        finally { _gate.Release(); }
    }

    /// <summary>
    /// Retrieves an item by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the item to retrieve.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The matching item, or <see langword="null"/> if it does not exist.</returns>
    public async Task<TodoItem?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try { return (await ReadAsync(cancellationToken)).SingleOrDefault(x => x.Id == id); }
        finally { _gate.Release(); }
    }

    /// <summary>
    /// Creates an item, normalizing its title and notes before persisting it.
    /// </summary>
    /// <param name="request">The values used to create the item.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The newly created item.</returns>
    public async Task<TodoItem> CreateAsync(CreateTodoRequest request, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var todos = await ReadAsync(cancellationToken);
            var item = new TodoItem { Title = request.Title.Trim(), Notes = Clean(request.Notes), DueDate = request.DueDate };
            todos.Add(item);
            await WriteAsync(todos, cancellationToken);
            return item;
        }
        finally { _gate.Release(); }
    }

    /// <summary>
    /// Updates an existing item and persists the changes.
    /// </summary>
    /// <param name="id">The identifier of the item to update.</param>
    /// <param name="request">The values to apply to the item.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The updated item, or <see langword="null"/> if it does not exist.</returns>
    public async Task<TodoItem?> UpdateAsync(Guid id, UpdateTodoRequest request, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var todos = await ReadAsync(cancellationToken);
            var item = todos.SingleOrDefault(x => x.Id == id);
            if (item is null) return null;
            item.Title = request.Title.Trim(); item.Notes = Clean(request.Notes); item.DueDate = request.DueDate; item.IsComplete = request.IsComplete;
            await WriteAsync(todos, cancellationToken);
            return item;
        }
        finally { _gate.Release(); }
    }

    /// <summary>
    /// Removes an item from the JSON file.
    /// </summary>
    /// <param name="id">The identifier of the item to delete.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns><see langword="true"/> if an item was deleted; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var todos = await ReadAsync(cancellationToken);
            var item = todos.SingleOrDefault(x => x.Id == id);
            if (item is null) return false;
            todos.Remove(item);
            await WriteAsync(todos, cancellationToken);
            return true;
        }
        finally { _gate.Release(); }
    }

    /// <summary>
    /// Reads all items from the JSON file.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the read operation.</param>
    /// <returns>The deserialized items, or an empty list if no file exists or it contains no items.</returns>
    private async Task<List<TodoItem>> ReadAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_filePath)) return [];
        await using var stream = File.OpenRead(_filePath);
        return await JsonSerializer.DeserializeAsync<List<TodoItem>>(stream, _jsonOptions, cancellationToken) ?? [];
    }

    /// <summary>
    /// Serializes and writes all items to the JSON file.
    /// </summary>
    /// <param name="todos">The items to persist.</param>
    /// <param name="cancellationToken">A token used to cancel the write operation.</param>
    private async Task WriteAsync(List<TodoItem> todos, CancellationToken cancellationToken)
    {
        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, todos, _jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Trims a string and converts null, empty, or whitespace-only values to <see langword="null"/>.
    /// </summary>
    /// <param name="value">The value to normalize.</param>
    /// <returns>The trimmed value, or <see langword="null"/> when it contains no non-whitespace characters.</returns>
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
