// KanbanApp.Services.Board/IColumnService.cs
using KanbanApp.Domain.Board;

namespace KanbanApp.Services.Board;

public interface IColumnService
{
    Task<Column> CreateColumnAsync(string boardId, string name, string ownerId);
    Task<IEnumerable<Column>> GetBoardColumnsAsync(string boardId, string ownerId);
    Task<Column> UpdateColumnNameAsync(string columnId, string newName, string ownerId);
    Task DeleteColumnAsync(string columnId, string ownerId);
    Task ReorderColumnsAsync(string boardId, Dictionary<string, int> newOrder, string ownerId);
}