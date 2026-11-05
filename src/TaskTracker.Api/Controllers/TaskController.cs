using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Data;
using TaskTracker.Api.Models;

namespace TaskTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _db;

    public TasksController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetAll()
    {
        var tasks = await _db.Tasks.OrderBy(t => t.Id).ToListAsync();
        return Ok(tasks);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskItem>> GetById(int id)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task is null)
        {
            return NotFound();
        }
        return task;
    }

    [HttpPost]
    public async Task<ActionResult<TaskItem>> Create(TaskItem input)
    {
        var task = new TaskItem
        {
            Title = input.Title,
            IsDone = false,
            CreatedAt = DateTime.UtcNow
        };
        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

        [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, TaskItem input)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task is null)
        {
            return NotFound();
        }

        task.Title = input.Title;
        task.IsDone = input.IsDone;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task is null)
        {
            return NotFound();
        }

        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}