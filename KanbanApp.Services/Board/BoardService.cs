// KanbanApp.Services.Board/BoardService.cs
namespace KanbanApp.Services.Board;
using KanbanApp.Domain.Board;
using KanbanApp.Domain.User;
using KanbanApp.API.Exceptions;
using KanbanApp.Infrastructure.Repositories;
public class BoardService : IBoardService
{
    private readonly IBoardRepository _boardRepository;
    private readonly IColumnRepository _columnRepository;
    private readonly ICardRepository _cardRepository;

    public BoardService(
        IBoardRepository boardRepository,
        IColumnRepository columnRepository,
        ICardRepository cardRepository)
    {
        _boardRepository = boardRepository;
        _columnRepository = columnRepository;
        _cardRepository = cardRepository;
    }

    public async Task<Board> CreateBoardAsync(string name, string ownerId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Board name cannot be empty");

        var board = new Board
        {
            Name = name,
            OwnerId = ownerId
        };

        return await _boardRepository.AddAsync(board);
    }

    public async Task<IEnumerable<Board>> GetUserBoardsAsync(string ownerId)
        => await _boardRepository.GetByOwnerIdAsync(ownerId);

    public async Task<Board?> GetBoardByIdAsync(string id, string ownerId)
    {
        var board = await _boardRepository.GetByIdAsync(id);
        
        if (board == null || board.OwnerId != ownerId)
            return null;
            
        return board;
    }

    public async Task<Board> UpdateBoardNameAsync(string id, string newName, string ownerId)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Board name cannot be empty");

        var board = await _boardRepository.GetByIdAsync(id);
        
        if (board == null || board.OwnerId != ownerId)
            throw new NotFoundException("Board not found or access denied");

        board.Name = newName;
        await _boardRepository.UpdateAsync(board);
        
        return board;
    }

    public async Task DeleteBoardAsync(string id, string ownerId)
    {
        var board = await _boardRepository.GetByIdAsync(id);
        
        if (board == null || board.OwnerId != ownerId)
            throw new NotFoundException("Board not found or access denied");

        // Удаляем все колонки и карточки доски
        var columns = await _columnRepository.GetByBoardIdAsync(id);
        foreach (var column in columns)
        {
            await _cardRepository.DeleteAsync(column.Id);
        }
        
        await _columnRepository.DeleteAsync(id);
        await _boardRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<Board>> SearchBoardsAsync(string ownerId, string name)
        => await _boardRepository.SearchByNameAsync(ownerId, name);
}