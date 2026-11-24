using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using TodoList.Api.Models;
using TodoList.Api.Services;

namespace TodoList.Tests;

[TestFixture]
public class HttpEndPointTest
{
    private string _directory = null!;
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void SetUp()
    {
        _directory = Path.Combine(Path.GetTempPath(), "TodoList.HttpTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_directory);
        var environment = new TestEnvironment { ContentRootPath = _directory };
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ITodoStore>();
                services.AddSingleton<ITodoStore>(new JsonTodoStore(environment));
            }));
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
        Directory.Delete(_directory, recursive: true);
    }

    [Test]
    public async Task CrudEndpointsCreateReadUpdateAndDeleteItem()
    {
        using var initial = await _client.GetAsync("/api/todos");
        Assert.That(initial.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(await initial.Content.ReadFromJsonAsync<List<TodoItem>>(), Is.Empty);

        using var createdResponse = await _client.PostAsJsonAsync("/api/todos", new CreateTodoRequest { Title = "First task", Notes = "Details" });
        Assert.That(createdResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var created = (await createdResponse.Content.ReadFromJsonAsync<TodoItem>())!;
        Assert.That(createdResponse.Headers.Location?.ToString(), Is.EqualTo($"/api/todos/{created.Id}"));

        using var getResponse = await _client.GetAsync($"/api/todos/{created.Id}");
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That((await getResponse.Content.ReadFromJsonAsync<TodoItem>())!.Title, Is.EqualTo("First task"));
        var all = await _client.GetFromJsonAsync<List<TodoItem>>("/api/todos");
        Assert.That(all!.Select(item => item.Id), Is.EqualTo(new[] { created.Id }));

        using var updateResponse = await _client.PutAsJsonAsync($"/api/todos/{created.Id}", new UpdateTodoRequest { Title = "Updated", IsComplete = true });
        Assert.That(updateResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var updated = (await updateResponse.Content.ReadFromJsonAsync<TodoItem>())!;
        Assert.Multiple(() =>
        {
            Assert.That(updated.Id, Is.EqualTo(created.Id));
            Assert.That(updated.Title, Is.EqualTo("Updated"));
            Assert.That(updated.IsComplete, Is.True);
        });
        var persisted = await _client.GetFromJsonAsync<TodoItem>($"/api/todos/{created.Id}");
        Assert.That(persisted!.Title, Is.EqualTo("Updated"));

        using var deleteResponse = await _client.DeleteAsync($"/api/todos/{created.Id}");
        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        using var afterDelete = await _client.GetAsync($"/api/todos/{created.Id}");
        Assert.That(afterDelete.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task MissingItemReturnsNotFoundForGetPutAndDelete()
    {
        var url = $"/api/todos/{Guid.NewGuid()}";
        using var get = await _client.GetAsync(url);
        using var put = await _client.PutAsJsonAsync(url, new UpdateTodoRequest { Title = "Missing" });
        using var delete = await _client.DeleteAsync(url);
        Assert.Multiple(() =>
        {
            Assert.That(get.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(put.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(delete.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        });
    }

    [Test]
    public async Task InvalidIdentifiersAndMalformedJsonAreRejected()
    {
        using var invalidId = await _client.GetAsync("/api/todos/not-a-guid");
        using var invalidJson = await _client.PostAsync("/api/todos", new StringContent("{", System.Text.Encoding.UTF8, "application/json"));
        Assert.Multiple(() =>
        {
            Assert.That(invalidId.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(invalidJson.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        });
    }

    private sealed class TestEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "TodoList.Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = "Development";
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
