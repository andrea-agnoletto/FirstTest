using TodoList.Api.Models;
using TodoList.Api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ITodoStore, JsonTodoStore>();
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins(builder.Configuration.GetValue<string>("ClientOrigin") ?? "https://localhost:5002", "http://localhost:5001")
    .AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.UseHttpsRedirection();
app.UseCors();

var todos = app.MapGroup("/api/todos");
todos.MapGet("/", async (ITodoStore store, CancellationToken ct) => Results.Ok(await store.GetAllAsync(ct)));
todos.MapGet("/{id:guid}", async (Guid id, ITodoStore store, CancellationToken ct) =>
    await store.GetAsync(id, ct) is { } item ? Results.Ok(item) : Results.NotFound());
todos.MapPost("/", async (CreateTodoRequest request, ITodoStore store, CancellationToken ct) =>
{
    var item = await store.CreateAsync(request, ct);
    return Results.Created($"/api/todos/{item.Id}", item);
});
todos.MapPut("/{id:guid}", async (Guid id, UpdateTodoRequest request, ITodoStore store, CancellationToken ct) =>
    await store.UpdateAsync(id, request, ct) is { } item ? Results.Ok(item) : Results.NotFound());
todos.MapDelete("/{id:guid}", async (Guid id, ITodoStore store, CancellationToken ct) =>
    await store.DeleteAsync(id, ct) ? Results.NoContent() : Results.NotFound());

app.Run();
