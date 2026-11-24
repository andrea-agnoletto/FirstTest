using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TodoList.Web.Models;

namespace TodoList.Web.Services;

public sealed class TodoApiClient(HttpClient http)
{
    public async Task<List<TodoItem>> GetTodosAsync() => await http.GetFromJsonAsync<List<TodoItem>>("api/todos") ?? [];
    public async Task<TodoItem> CreateAsync(TodoInput input)
    {
        var response = await http.PostAsJsonAsync("api/todos", input);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TodoItem>())!;
    }
    public async Task UpdateAsync(TodoItem item)
    {
        var response = await http.PutAsJsonAsync($"api/todos/{item.Id}", new { item.Title, item.Notes, item.DueDate, item.IsComplete });
        response.EnsureSuccessStatusCode();
    }
    public async Task DeleteAsync(Guid id)
    {
        var response = await http.DeleteAsync($"api/todos/{id}");
        response.EnsureSuccessStatusCode();
    }
}
