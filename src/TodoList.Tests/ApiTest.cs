using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using TodoList.Api.Models;
using TodoList.Api.Services;

namespace TodoList.Tests;

[TestFixture]
public class ApiTest
{
    private string _directory = null!;
    private JsonTodoStore _store = null!;

    [SetUp]
    public void SetUp()
    {
        _directory = Path.Combine(Path.GetTempPath(), "TodoList.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_directory);
        _store = new JsonTodoStore(new TestEnvironment { ContentRootPath = _directory });
    }

    [TearDown]
    public void TearDown() => Directory.Delete(_directory, recursive: true);

    [Test]
    public async Task EmptyStoreReturnsNoItems()
    {
        Assert.That(await _store.GetAllAsync(CancellationToken.None), Is.Empty);
        Assert.That(await _store.GetAsync(Guid.NewGuid(), CancellationToken.None), Is.Null);
    }

    [Test]
    public async Task CreatePersistsNormalizedItemAcrossStoreInstances()
    {
        var due = new DateTimeOffset(2027, 1, 2, 0, 0, 0, TimeSpan.Zero);
        var created = await _store.CreateAsync(new CreateTodoRequest { Title = "  Buy milk  ", Notes = "  Two litres  ", DueDate = due }, CancellationToken.None);
        var reloaded = new JsonTodoStore(new TestEnvironment { ContentRootPath = _directory });
        var item = await reloaded.GetAsync(created.Id, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(created.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(item, Is.Not.Null);
            Assert.That(item!.Title, Is.EqualTo("Buy milk"));
            Assert.That(item.Notes, Is.EqualTo("Two litres"));
            Assert.That(item.DueDate, Is.EqualTo(due));
            Assert.That(item.IsComplete, Is.False);
        });
    }

    [Test]
    public async Task UpdatePersistsChangesAndPreservesIdentity()
    {
        var created = await _store.CreateAsync(new CreateTodoRequest { Title = "Original" }, CancellationToken.None);
        var updated = await _store.UpdateAsync(created.Id, new UpdateTodoRequest { Title = " Updated ", Notes = " ", IsComplete = true }, CancellationToken.None);
        var loaded = await _store.GetAsync(created.Id, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(updated, Is.Not.Null);
            Assert.That(loaded!.Id, Is.EqualTo(created.Id));
            Assert.That(loaded.CreatedAt, Is.EqualTo(created.CreatedAt));
            Assert.That(loaded.Title, Is.EqualTo("Updated"));
            Assert.That(loaded.Notes, Is.Null);
            Assert.That(loaded.IsComplete, Is.True);
        });
    }

    [Test]
    public async Task DeleteRemovesOnlyTheRequestedItemAndMissingOperationsReturnNotFound()
    {
        var first = await _store.CreateAsync(new CreateTodoRequest { Title = "First" }, CancellationToken.None);
        var second = await _store.CreateAsync(new CreateTodoRequest { Title = "Second" }, CancellationToken.None);
        Assert.That(await _store.DeleteAsync(first.Id, CancellationToken.None), Is.True);
        Assert.That(await _store.DeleteAsync(first.Id, CancellationToken.None), Is.False);
        Assert.That(await _store.UpdateAsync(first.Id, new UpdateTodoRequest { Title = "Missing" }, CancellationToken.None), Is.Null);
        Assert.That((await _store.GetAllAsync(CancellationToken.None)).Select(x => x.Id), Is.EqualTo(new[] { second.Id }));
    }

    [Test]
    public async Task GetAllOrdersOpenTasksByDueDateBeforeCompletedTasks()
    {
        var later = await _store.CreateAsync(new CreateTodoRequest { Title = "Later", DueDate = DateTimeOffset.Parse("2027-02-01T00:00:00Z") }, CancellationToken.None);
        var earlier = await _store.CreateAsync(new CreateTodoRequest { Title = "Earlier", DueDate = DateTimeOffset.Parse("2027-01-01T00:00:00Z") }, CancellationToken.None);
        var completed = await _store.CreateAsync(new CreateTodoRequest { Title = "Completed" }, CancellationToken.None);
        await _store.UpdateAsync(completed.Id, new UpdateTodoRequest { Title = completed.Title, IsComplete = true }, CancellationToken.None);
        Assert.That((await _store.GetAllAsync(CancellationToken.None)).Select(x => x.Id), Is.EqualTo(new[] { earlier.Id, later.Id, completed.Id }));
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
