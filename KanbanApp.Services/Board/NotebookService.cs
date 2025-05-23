// KanbanApp.Services.Board/NotebookService.cs
using KanbanApp.Domain.Board;
using KanbanApp.API.Exceptions;
using KanbanApp.Infrastructure.Repositories;

namespace KanbanApp.Services.Board;

public class NotebookService : INotebookService
{
    private readonly IBoardRepository _boardRepository;
    private readonly INotebookRepository _notebookRepository;
    private readonly INoteRepository _noteRepository;

    public NotebookService(
        IBoardRepository boardRepository,
        INotebookRepository notebookRepository,
        INoteRepository noteRepository)
    {
        _boardRepository = boardRepository;
        _notebookRepository = notebookRepository;
        _noteRepository = noteRepository;
    }

    public async Task<Notebook> CreateNotebookAsync(string boardId, string name, string ownerId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Notebook name cannot be empty");

        var board = await _boardRepository.GetByIdAsync(boardId);
        if (board == null || board.OwnerId != ownerId)
            throw new NotFoundException("Board not found or access denied");

        var notebook = new Notebook
        {
            BoardId = boardId,
            Name = name
        };

        return await _notebookRepository.AddAsync(notebook);
    }

    public async Task<IEnumerable<Notebook>> GetBoardNotebooksAsync(string boardId, string ownerId)
    {
        var board = await _boardRepository.GetByIdAsync(boardId);
        if (board == null || board.OwnerId != ownerId)
            throw new NotFoundException("Board not found or access denied");

        return await _notebookRepository.GetByBoardIdAsync(boardId);
    }

    public async Task<Notebook> UpdateNotebookNameAsync(string notebookId, string newName, string ownerId)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Notebook name cannot be empty");

        var notebook = await _notebookRepository.GetByIdAsync(notebookId);
        if (notebook == null)
            throw new NotFoundException("Notebook not found");

        var board = await _boardRepository.GetByIdAsync(notebook.BoardId);
        if (board == null || board.OwnerId != ownerId)
            throw new NotFoundException("Access denied");

        notebook.Name = newName;
        await _notebookRepository.UpdateAsync(notebook);
        
        return notebook;
    }

    public async Task DeleteNotebookAsync(string notebookId, string ownerId)
    {
        var notebook = await _notebookRepository.GetByIdAsync(notebookId);
        if (notebook == null)
            throw new NotFoundException("Notebook not found");

        var board = await _boardRepository.GetByIdAsync(notebook.BoardId);
        if (board == null || board.OwnerId != ownerId)
            throw new NotFoundException("Access denied");

        // Удаляем все заметки в блокноте
        var notes = await _noteRepository.GetByNotebookIdAsync(notebookId);
        foreach (var note in notes)
        {
            await _noteRepository.DeleteAsync(note.Id);
        }

        await _notebookRepository.DeleteAsync(notebookId);
    }
}