using Microsoft.AspNetCore.Mvc;
using TaskTracker.Api.Models;

namespace TaskTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private static readonly List<TaskItem> _tasks = new();
    private static int _nextId = 1;

    [HttpGet]
    public ActionResult<IEnumerable<TaskItem>> GetAll()
    {
        return Ok(_tasks);
    }

    [HttpGet("{id:int}")]
    public ActionResult<TaskItem> GetById(int id)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == id);
        if (task is null)
        {
            return NotFound();
        }
        return task;
    }

    [HttpPost]
    public ActionResult<TaskItem> Create(TaskItem input)
    {
        var task = new TaskItem
        {
            Id = _nextId++,
            Title = input.Title,
            IsDone = false,
            CreatedAt = DateTime.UtcNow
        };
        _tasks.Add(task);
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }
}