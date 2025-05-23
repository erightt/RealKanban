// KanbanApp.Services.Board/ICardService.cs
using KanbanApp.Domain.Board;

namespace KanbanApp.Services.Board;

public interface ICardService
{
    Task<Card> CreateCardAsync(string columnId, string title, string? description, string ownerId);
    Task<IEnumerable<Card>> GetColumnCardsAsync(string columnId, string ownerId);
    Task<Card> UpdateCardAsync(string cardId, string title, string? description, string ownerId);
    Task DeleteCardAsync(string cardId, string ownerId);
    Task MoveCardAsync(string cardId, string newColumnId, int newOrder, string ownerId);
    Task ReorderCardsAsync(string columnId, Dictionary<string, int> newOrder, string ownerId);
}