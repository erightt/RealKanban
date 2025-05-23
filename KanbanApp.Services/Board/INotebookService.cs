// KanbanApp.Services.Board/INotebookService.cs
using KanbanApp.Domain.Board;

namespace KanbanApp.Services.Board;

public interface INotebookService
{
    Task<Notebook> CreateNotebookAsync(string boardId, string name, string ownerId);
    Task<IEnumerable<Notebook>> GetBoardNotebooksAsync(string boardId, string ownerId);
    Task<Notebook> UpdateNotebookNameAsync(string notebookId, string newName, string ownerId);
    Task DeleteNotebookAsync(string notebookId, string ownerId);
}