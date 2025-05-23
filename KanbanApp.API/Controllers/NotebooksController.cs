// KanbanApp.API.Controllers/NotebooksController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using KanbanApp.Services.Board;
using KanbanApp.API.Middleware;
using System.Security.Claims;
using KanbanApp.Domain.Board;

namespace KanbanApp.API.Controllers;

[ApiController]
[Route("api/boards/{boardId}/[controller]")]
[Authorize]
public class NotebooksController : ControllerBase
{
    private readonly INotebookService _notebookService;
    private readonly ILogger<NotebooksController> _logger;

    public NotebooksController(
        INotebookService notebookService,
        ILogger<NotebooksController> logger)
    {
        _notebookService = notebookService;
        _logger = logger;
    }

    private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

    [HttpPost]
    [ProducesResponseType(typeof(Notebook), 200)]
    [ProducesResponseType(typeof(ErrorDetails), 400)]
    public async Task<IActionResult> CreateNotebook(string boardId, [FromBody] string name)
    {
        var notebook = await _notebookService.CreateNotebookAsync(boardId, name, GetUserId());
        return Ok(notebook);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Notebook>), 200)]
    [ProducesResponseType(typeof(ErrorDetails), 404)]
    public async Task<IActionResult> GetBoardNotebooks(string boardId)
    {
        var notebooks = await _notebookService.GetBoardNotebooksAsync(boardId, GetUserId());
        return Ok(notebooks);
    }

    [HttpPut("{notebookId}")]
    [ProducesResponseType(typeof(Notebook), 200)]
    [ProducesResponseType(typeof(ErrorDetails), 400)]
    [ProducesResponseType(typeof(ErrorDetails), 404)]
    public async Task<IActionResult> UpdateNotebookName(string boardId, string notebookId, [FromBody] string newName)
    {
        var notebook = await _notebookService.UpdateNotebookNameAsync(notebookId, newName, GetUserId());
        return Ok(notebook);
    }

    [HttpDelete("{notebookId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ErrorDetails), 404)]
    public async Task<IActionResult> DeleteNotebook(string boardId, string notebookId)
    {
        await _notebookService.DeleteNotebookAsync(notebookId, GetUserId());
        return NoContent();
    }
}