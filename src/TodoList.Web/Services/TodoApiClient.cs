using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TodoList.Web.Models;

namespace TodoList.Web.Services;

/// <summary>
/// Sends HTTP requests to the TODO API.
/// </summary>
/// <param name="http">The HTTP client configured with the API base address.</param>
public sealed class TodoApiClient(HttpClient http)
{
    /// <summary>
    /// Retrieves all TODO items from the API.
    /// </summary>
    /// <returns>The TODO items returned by the API.</returns>
    public async Task<List<TodoItem>> GetTodosAsync() => await http.GetFromJsonAsync<List<TodoItem>>("api/todos") ?? [];

    /// <summary>
    /// Creates a TODO item through the API.
    /// </summary>
    /// <param name="input">The values used to create the item.</param>
    /// <returns>The item created by the API.</returns>
    public async Task<TodoItem> CreateAsync(TodoInput input)
    {
        var response = await http.PostAsJsonAsync("api/todos", input);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TodoItem>())!;
    }

    /// <summary>
    /// Updates a TODO item through the API.
    /// </summary>
    /// <param name="item">The item containing the values to update.</param>
    public async Task UpdateAsync(TodoItem item)
    {
        var response = await http.PutAsJsonAsync($"api/todos/{item.Id}", new { item.Title, item.Notes, item.DueDate, item.IsComplete });
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Deletes a TODO item through the API.
    /// </summary>
    /// <param name="id">The identifier of the item to delete.</param>
    public async Task DeleteAsync(Guid id)
    {
        var response = await http.DeleteAsync($"api/todos/{id}");
        response.EnsureSuccessStatusCode();
    }
}
