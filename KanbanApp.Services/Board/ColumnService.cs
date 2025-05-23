// KanbanApp.Services.Board/ColumnService.cs
using KanbanApp.Domain.Board;
using KanbanApp.API.Exceptions;
using KanbanApp.Infrastructure.Repositories;

namespace KanbanApp.Services.Board;

public class ColumnService : IColumnService
{
    private readonly IBoardRepository _boardRepository;
    private readonly IColumnRepository _columnRepository;
    private readonly ICardRepository _cardRepository;

    public ColumnService(
        IBoardRepository boardRepository,
        IColumnRepository columnRepository,
        ICardRepository cardRepository)
    {
        _boardRepository = boardRepository;
        _columnRepository = columnRepository;
        _cardRepository = cardRepository;
    }

    public async Task<Column> CreateColumnAsync(string boardId, string name, string ownerId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Column name cannot be empty");

        var board = await _boardRepository.GetByIdAsync(boardId);
        if (board == null || board.OwnerId != ownerId)
            throw new NotFoundException("Board not found or access denied");

        var column = new Column
        {
            BoardId = boardId,
            Name = name
        };

        return await _columnRepository.AddAsync(column);
    }

    public async Task<IEnumerable<Column>> GetBoardColumnsAsync(string boardId, string ownerId)
    {
        var board = await _boardRepository.GetByIdAsync(boardId);
        if (board == null || board.OwnerId != ownerId)
            throw new NotFoundException("Board not found or access denied");

        return await _columnRepository.GetByBoardIdAsync(boardId);
    }

    public async Task<Column> UpdateColumnNameAsync(string columnId, string newName, string ownerId)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Column name cannot be empty");

        var column = await _columnRepository.GetByIdAsync(columnId);
        if (column == null)
            throw new NotFoundException("Column not found");

        var board = await _boardRepository.GetByIdAsync(column.BoardId);
        if (board == null || board.OwnerId != ownerId)
            throw new NotFoundException("Access denied");

        column.Name = newName;
        await _columnRepository.UpdateAsync(column);
        
        return column;
    }

    public async Task DeleteColumnAsync(string columnId, string ownerId)
    {
        var column = await _columnRepository.GetByIdAsync(columnId);
        if (column == null)
            throw new NotFoundException("Column not found");

        var board = await _boardRepository.GetByIdAsync(column.BoardId);
        if (board == null || board.OwnerId != ownerId)
            throw new NotFoundException("Access denied");

        // Удаляем все карточки в колонке
        var cards = await _cardRepository.GetByColumnIdAsync(columnId);
        foreach (var card in cards)
        {
            await _cardRepository.DeleteAsync(card.Id);
        }

        await _columnRepository.DeleteAsync(columnId);
    }

    public async Task ReorderColumnsAsync(string boardId, Dictionary<string, int> newOrder, string ownerId)
    {
        var board = await _boardRepository.GetByIdAsync(boardId);
        if (board == null || board.OwnerId != ownerId)
            throw new NotFoundException("Board not found or access denied");

        await _columnRepository.ReorderColumnsAsync(boardId, newOrder);
    }
}