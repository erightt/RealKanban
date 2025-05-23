// KanbanApp.API.Controllers/ColumnsController.cs
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
public class ColumnsController : ControllerBase
{
    private readonly IColumnService _columnService;
    private readonly ILogger<ColumnsController> _logger;

    public ColumnsController(
        IColumnService columnService,
        ILogger<ColumnsController> logger)
    {
        _columnService = columnService;
        _logger = logger;
    }

    private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

    [HttpPost]
    [ProducesResponseType(typeof(Column), 200)]
    [ProducesResponseType(typeof(ErrorDetails), 400)]
    public async Task<IActionResult> CreateColumn(string boardId, [FromBody] string name)
    {
        var userId = GetUserId();
        var column = await _columnService.CreateColumnAsync(boardId, name, userId);
        return Ok(column);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Column>), 200)]
    [ProducesResponseType(typeof(ErrorDetails), 404)]
    public async Task<IActionResult> GetBoardColumns(string boardId)
    {
        var userId = GetUserId();
        var columns = await _columnService.GetBoardColumnsAsync(boardId, userId);
        return Ok(columns);
    }

    [HttpPut("{columnId}")]
    [ProducesResponseType(typeof(Column), 200)]
    [ProducesResponseType(typeof(ErrorDetails), 400)]
    [ProducesResponseType(typeof(ErrorDetails), 404)]
    public async Task<IActionResult> UpdateColumnName(string boardId, string columnId, [FromBody] string newName)
    {
        var userId = GetUserId();
        var column = await _columnService.UpdateColumnNameAsync(columnId, newName, userId);
        return Ok(column);
    }

    [HttpDelete("{columnId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ErrorDetails), 404)]
    public async Task<IActionResult> DeleteColumn(string boardId, string columnId)
    {
        var userId = GetUserId();
        await _columnService.DeleteColumnAsync(columnId, userId);
        return NoContent();
    }

    [HttpPut("reorder")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ErrorDetails), 400)]
    [ProducesResponseType(typeof(ErrorDetails), 404)]
    public async Task<IActionResult> ReorderColumns(string boardId, [FromBody] Dictionary<string, int> newOrder)
    {
        var userId = GetUserId();
        await _columnService.ReorderColumnsAsync(boardId, newOrder, userId);
        return NoContent();
    }
}