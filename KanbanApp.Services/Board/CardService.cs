// KanbanApp.Services.Board/CardService.cs
using KanbanApp.Domain.Board;
using KanbanApp.API.Exceptions;
using KanbanApp.Infrastructure.Repositories;

namespace KanbanApp.Services.Board;

public class CardService : ICardService
{
    private readonly IColumnRepository _columnRepository;
    private readonly ICardRepository _cardRepository;

    public CardService(
        IColumnRepository columnRepository,
        ICardRepository cardRepository)
    {
        _columnRepository = columnRepository;
        _cardRepository = cardRepository;
    }

    public async Task<Card> CreateCardAsync(string columnId, string title, string? description, string ownerId)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Card title cannot be empty");

        var column = await _columnRepository.GetByIdAsync(columnId);
        if (column == null)
            throw new NotFoundException("Column not found");

        var card = new Card
        {
            ColumnId = columnId,
            Title = title,
            Description = description
        };

        return await _cardRepository.AddAsync(card);
    }

    public async Task<IEnumerable<Card>> GetColumnCardsAsync(string columnId, string ownerId)
    {
        var column = await _columnRepository.GetByIdAsync(columnId);
        if (column == null)
            throw new NotFoundException("Column not found");

        return await _cardRepository.GetByColumnIdAsync(columnId);
    }

    public async Task<Card> UpdateCardAsync(string cardId, string title, string? description, string ownerId)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Card title cannot be empty");

        var card = await _cardRepository.GetByIdAsync(cardId);
        if (card == null)
            throw new NotFoundException("Card not found");

        var column = await _columnRepository.GetByIdAsync(card.ColumnId);
        if (column == null)
            throw new NotFoundException("Column not found");

        card.Title = title;
        card.Description = description;
        await _cardRepository.UpdateAsync(card);
        
        return card;
    }

    public async Task DeleteCardAsync(string cardId, string ownerId)
    {
        var card = await _cardRepository.GetByIdAsync(cardId);
        if (card == null)
            throw new NotFoundException("Card not found");

        var column = await _columnRepository.GetByIdAsync(card.ColumnId);
        if (column == null)
            throw new NotFoundException("Column not found");

        await _cardRepository.DeleteAsync(cardId);
    }

    public async Task MoveCardAsync(string cardId, string newColumnId, int newOrder, string ownerId)
    {
        var card = await _cardRepository.GetByIdAsync(cardId);
        if (card == null)
            throw new NotFoundException("Card not found");

        var oldColumn = await _columnRepository.GetByIdAsync(card.ColumnId);
        if (oldColumn == null)
            throw new NotFoundException("Old column not found");

        var newColumn = await _columnRepository.GetByIdAsync(newColumnId);
        if (newColumn == null)
            throw new NotFoundException("New column not found");

        await _cardRepository.MoveCardAsync(cardId, newColumnId, newOrder);
    }

    public async Task ReorderCardsAsync(string columnId, Dictionary<string, int> newOrder, string ownerId)
    {
        var column = await _columnRepository.GetByIdAsync(columnId);
        if (column == null)
            throw new NotFoundException("Column not found");

        await _cardRepository.ReorderCardsAsync(columnId, newOrder);
    }
}