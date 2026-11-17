using System.Text.Json;
using TodoList.Api.Models;

namespace TodoList.Api.Services;

public sealed class JsonTodoStore : ITodoStore
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public JsonTodoStore(IWebHostEnvironment environment)
    {
        var dataDirectory = Path.Combine(environment.ContentRootPath, "Data");
        Directory.CreateDirectory(dataDirectory);
        _filePath = Path.Combine(dataDirectory, "todos.json");
    }

    public async Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try { return (await ReadAsync(cancellationToken)).OrderBy(x => x.IsComplete).ThenBy(x => x.DueDate ?? DateTimeOffset.MaxValue).ThenByDescending(x => x.CreatedAt).ToList(); }
        finally { _gate.Release(); }
    }

    public async Task<TodoItem?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try { return (await ReadAsync(cancellationToken)).SingleOrDefault(x => x.Id == id); }
        finally { _gate.Release(); }
    }

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

    private async Task<List<TodoItem>> ReadAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_filePath)) return [];
        await using var stream = File.OpenRead(_filePath);
        return await JsonSerializer.DeserializeAsync<List<TodoItem>>(stream, _jsonOptions, cancellationToken) ?? [];
    }

    private async Task WriteAsync(List<TodoItem> todos, CancellationToken cancellationToken)
    {
        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, todos, _jsonOptions, cancellationToken);
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
