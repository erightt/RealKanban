// KanbanApp.API.Controllers/NotesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using KanbanApp.Services.Board;
using KanbanApp.API.Middleware;
using System.Security.Claims;
using KanbanApp.Domain.Board;

namespace KanbanApp.API.Controllers;

[ApiController]
[Route("api/notebooks/{notebookId}/[controller]")]
[Authorize]
public class NotesController : ControllerBase
{
    private readonly INoteService _noteService;
    private readonly ILogger<NotesController> _logger;

    public NotesController(
        INoteService noteService,
        ILogger<NotesController> logger)
    {
        _noteService = noteService;
        _logger = logger;
    }

    private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

    [HttpPost]
    [ProducesResponseType(typeof(Note), 200)]
    [ProducesResponseType(typeof(ErrorDetails), 400)]
    public async Task<IActionResult> CreateNote(string notebookId, [FromBody] string content)
    {
        var note = await _noteService.CreateNoteAsync(notebookId, content, GetUserId());
        return Ok(note);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Note>), 200)]
    [ProducesResponseType(typeof(ErrorDetails), 404)]
    public async Task<IActionResult> GetNotebookNotes(string notebookId)
    {
        var notes = await _noteService.GetNotebookNotesAsync(notebookId, GetUserId());
        return Ok(notes);
    }

    [HttpPut("{noteId}")]
    [ProducesResponseType(typeof(Note), 200)]
    [ProducesResponseType(typeof(ErrorDetails), 400)]
    [ProducesResponseType(typeof(ErrorDetails), 404)]
    public async Task<IActionResult> UpdateNote(string notebookId, string noteId, [FromBody] string content)
    {
        var note = await _noteService.UpdateNoteAsync(noteId, content, GetUserId());
        return Ok(note);
    }

    [HttpDelete("{noteId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ErrorDetails), 404)]
    public async Task<IActionResult> DeleteNote(string notebookId, string noteId)
    {
        await _noteService.DeleteNoteAsync(noteId, GetUserId());
        return NoContent();
    }

    [HttpPut("reorder")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ErrorDetails), 400)]
    [ProducesResponseType(typeof(ErrorDetails), 404)]
    public async Task<IActionResult> ReorderNotes(string notebookId, [FromBody] Dictionary<string, int> newOrder)
    {
        await _noteService.ReorderNotesAsync(notebookId, newOrder, GetUserId());
        return NoContent();
    }

    [HttpPost("convert-to-cards")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ErrorDetails), 404)]
    public async Task<IActionResult> ConvertToCards(string notebookId)
    {
        await _noteService.ConvertToCardsAsync(notebookId, GetUserId());
        return NoContent();
    }
}