// KanbanApp.API.Controllers/CardsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using KanbanApp.Services.Board;
using KanbanApp.API.Middleware;
using System.Security.Claims;
using KanbanApp.Domain.Board;

namespace KanbanApp.API.Controllers;

[ApiController]
[Route("api/columns/{columnId}/[controller]")]
[Authorize]
public class CardsController : ControllerBase
{
    private readonly ICardService _cardService;
    private readonly ILogger<CardsController> _logger;

    public CardsController(
        ICardService cardService,
        ILogger<CardsController> logger)
    {
        _cardService = cardService;
        _logger = logger;
    }

    private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

    [HttpPost]
    [ProducesResponseType(typeof(Card), 200)]
    [ProducesResponseType(typeof(ErrorDetails), 400)]
    public async Task<IActionResult> CreateCard(string columnId, [FromBody] CreateCardRequest request)
    {
        var userId = GetUserId();
        var card = await _cardService.CreateCardAsync(columnId, request.Title, request.Description, request.Color, userId);
        return Ok(card);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Card>), 200)]
    [ProducesResponseType(typeof(ErrorDetails), 404)]
    public async Task<IActionResult> GetColumnCards(string columnId)
    {
        var userId = GetUserId();
        var cards = await _cardService.GetColumnCardsAsync(columnId, userId);
        return Ok(cards);
    }

    [HttpPut("{cardId}")]
    [ProducesResponseType(typeof(Card), 200)]
    [ProducesResponseType(typeof(ErrorDetails), 400)]
    [ProducesResponseType(typeof(ErrorDetails), 404)]
    public async Task<IActionResult> UpdateCard(string columnId, string cardId, [FromBody] UpdateCardRequest request)
    {
        var userId = GetUserId();
        var card = await _cardService.UpdateCardAsync(cardId, request.Title, request.Description, request.Color, userId);
        return Ok(card);
    }

    [HttpDelete("{cardId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ErrorDetails), 404)]
    public async Task<IActionResult> DeleteCard(string columnId, string cardId)
    {
        var userId = GetUserId();
        await _cardService.DeleteCardAsync(cardId, userId);
        return NoContent();
    }

    [HttpPut("{cardId}/move")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ErrorDetails), 400)]
    [ProducesResponseType(typeof(ErrorDetails), 404)]
    public async Task<IActionResult> MoveCard(string columnId, string cardId, [FromBody] MoveCardRequest request)
    {
        var userId = GetUserId();
        await _cardService.MoveCardAsync(cardId, request.NewColumnId, request.NewOrder, userId);
        return NoContent();
    }

    [HttpPut("reorder")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ErrorDetails), 400)]
    [ProducesResponseType(typeof(ErrorDetails), 404)]
    public async Task<IActionResult> ReorderCards(string columnId, [FromBody] Dictionary<string, int> newOrder)
    {
        var userId = GetUserId();
        await _cardService.ReorderCardsAsync(columnId, newOrder, userId);
        return NoContent();
    }
}

public record CreateCardRequest(string Title, string? Description, string? Color);
public record UpdateCardRequest(string Title, string? Description, string? Color);
public record MoveCardRequest(string NewColumnId, int NewOrder);