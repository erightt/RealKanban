// KanbanApp.Services.Board/INoteService.cs
using KanbanApp.Domain.Board;

namespace KanbanApp.Services.Board;

public interface INoteService
{
    Task<Note> CreateNoteAsync(string notebookId, string content, string ownerId);
    Task<IEnumerable<Note>> GetNotebookNotesAsync(string notebookId, string ownerId);
    Task<Note> UpdateNoteAsync(string noteId, string content, string ownerId);
    Task DeleteNoteAsync(string noteId, string ownerId);
    Task ReorderNotesAsync(string notebookId, Dictionary<string, int> newOrder, string ownerId);
    Task ConvertToCardsAsync(string notebookId, string ownerId);
}