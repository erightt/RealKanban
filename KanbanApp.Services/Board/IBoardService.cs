// KanbanApp.Services.Board/IBoardService.cs
namespace KanbanApp.Services.Board;
using KanbanApp.Domain.Board;
using KanbanApp.Domain.User;
public interface IBoardService
{
    Task<Board> CreateBoardAsync(string name, string ownerId);
    Task<IEnumerable<Board>> GetUserBoardsAsync(string ownerId);
    Task<Board?> GetBoardByIdAsync(string id, string ownerId);
    Task<Board> UpdateBoardNameAsync(string id, string newName, string ownerId);
    Task DeleteBoardAsync(string id, string ownerId);
    Task<IEnumerable<Board>> SearchBoardsAsync(string ownerId, string name);
}