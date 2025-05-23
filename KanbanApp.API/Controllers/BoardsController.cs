// KanbanApp.API.Controllers/BoardsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using KanbanApp.Services.Board;
using KanbanApp.API.Middleware;
using System.Security.Claims;
using KanbanApp.Domain.Board;

namespace KanbanApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BoardsController : ControllerBase
{
    private readonly IBoardService _boardService;
    private readonly ILogger<BoardsController> _logger;

    public BoardsController(
        IBoardService boardService,
        ILogger<BoardsController> logger)
    {
        _boardService = boardService;
        _logger = logger;
    }

    private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

    [HttpPost]
    [ProducesResponseType(typeof(Board), 200)]
    [ProducesResponseType(typeof(ErrorDetails), 400)]
    public async Task<IActionResult> CreateBoard([FromBody] string name)
    {
        var userId = GetUserId();
        var board = await _boardService.CreateBoardAsync(name, userId);
        return Ok(board);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Board>), 200)]
    public async Task<IActionResult> GetUserBoards()
    {
        var userId = GetUserId();
        var boards = await _boardService.GetUserBoardsAsync(userId);
        return Ok(boards);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Board), 200)]
    [ProducesResponseType(typeof(ErrorDetails), 404)]
    public async Task<IActionResult> GetBoardById(string id)
    {
        var userId = GetUserId();
        var board = await _boardService.GetBoardByIdAsync(id, userId);
        
        if (board == null)
            return NotFound(new ErrorDetails(404, "Board not found or access denied"));
            
        return Ok(board);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Board), 200)]
    [ProducesResponseType(typeof(ErrorDetails), 400)]
    [ProducesResponseType(typeof(ErrorDetails), 404)]
    public async Task<IActionResult> UpdateBoardName(string id, [FromBody] string newName)
    {
        var userId = GetUserId();
        var board = await _boardService.UpdateBoardNameAsync(id, newName, userId);
        return Ok(board);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ErrorDetails), 404)]
    public async Task<IActionResult> DeleteBoard(string id)
    {
        var userId = GetUserId();
        await _boardService.DeleteBoardAsync(id, userId);
        return NoContent();
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<Board>), 200)]
    public async Task<IActionResult> SearchBoards([FromQuery] string name)
    {
        var userId = GetUserId();
        var boards = await _boardService.SearchBoardsAsync(userId, name);
        return Ok(boards);
    }
}