using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskAPI.DTOs;
using TaskAPI.Services;

namespace TaskAPI.Controllers;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class TasksController : ControllerBase
{
    private readonly TaskService _service;

    public TasksController(TaskService service)
    {
        _service = service;
    }

    private string OwnerId => User.FindFirstValue(ClaimTypes.Name)!;

    [HttpGet]
    [ResponseCache(Duration = 30, VaryByQueryKeys = ["*"])]
    public async Task<IActionResult> GetAll([FromQuery] TaskQueryParams query) =>
        Ok(await _service.GetAllAsync(query, OwnerId));

    [HttpGet("{id}")]
    [ResponseCache(Duration = 30)]
    public async Task<IActionResult> GetById(int id)
    {
        var task = await _service.GetByIdAsync(id, OwnerId);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTaskRequest request)
    {
        var created = await _service.CreateAsync(request, OwnerId);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateTaskRequest request)
    {
        var updated = await _service.UpdateAsync(id, request, OwnerId);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id, OwnerId);
        return deleted ? NoContent() : NotFound();
    }
}
