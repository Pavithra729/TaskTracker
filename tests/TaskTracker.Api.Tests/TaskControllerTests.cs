using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Controllers;
using TaskTracker.Api.Data;
using TaskTracker.Api.Models;

namespace TaskTracker.Api.Tests;

public class TasksControllerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _db;
    private readonly TasksController _controller;

    public TasksControllerTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();

        _controller = new TasksController(_db);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task Create_SavesTask_AndReturns201()
    {
        var result = await _controller.Create(new TaskItem { Title = "Write tests" });

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var task = Assert.IsType<TaskItem>(created.Value);
        Assert.True(task.Id > 0);
        Assert.Equal("Write tests", task.Title);
        Assert.False(task.IsDone);
        Assert.Equal(1, await _db.Tasks.CountAsync());
    }

    [Fact]
    public async Task GetAll_ReturnsAllTasks()
    {
        _db.Tasks.AddRange(new TaskItem { Title = "A" }, new TaskItem { Title = "B" });
        await _db.SaveChangesAsync();

        var result = await _controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var tasks = Assert.IsAssignableFrom<IEnumerable<TaskItem>>(ok.Value);
        Assert.Equal(2, tasks.Count());
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsTask()
    {
        var seeded = new TaskItem { Title = "Find me" };
        _db.Tasks.Add(seeded);
        await _db.SaveChangesAsync();

        var result = await _controller.GetById(seeded.Id);

        Assert.Equal("Find me", result.Value?.Title);
    }

    [Fact]
    public async Task GetById_MissingId_Returns404()
    {
        var result = await _controller.GetById(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Update_ExistingTask_ChangesFields_AndReturns204()
    {
        var seeded = new TaskItem { Title = "Old title" };
        _db.Tasks.Add(seeded);
        await _db.SaveChangesAsync();

        var result = await _controller.Update(seeded.Id, new TaskItem { Title = "New title", IsDone = true });

        Assert.IsType<NoContentResult>(result);
        var saved = await _db.Tasks.AsNoTracking().SingleAsync(t => t.Id == seeded.Id);
        Assert.Equal("New title", saved.Title);
        Assert.True(saved.IsDone);
    }

    [Fact]
    public async Task Update_MissingTask_Returns404()
    {
        var result = await _controller.Update(999, new TaskItem { Title = "Nope" });

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_ExistingTask_RemovesIt_AndReturns204()
    {
        var seeded = new TaskItem { Title = "Delete me" };
        _db.Tasks.Add(seeded);
        await _db.SaveChangesAsync();

        var result = await _controller.Delete(seeded.Id);

        Assert.IsType<NoContentResult>(result);
        Assert.False(await _db.Tasks.AnyAsync(t => t.Id == seeded.Id));
    }

    [Fact]
    public async Task Delete_MissingTask_Returns404()
    {
        var result = await _controller.Delete(999);

        Assert.IsType<NotFoundResult>(result);
    }
}