// KanbanApp.Services.Board/NoteService.cs
using KanbanApp.Domain.Board;
using KanbanApp.API.Exceptions;
using KanbanApp.Infrastructure.Repositories;

namespace KanbanApp.Services.Board;

public class NoteService : INoteService
{
    private readonly INotebookRepository _notebookRepository;
    private readonly INoteRepository _noteRepository;
    private readonly IColumnRepository _columnRepository;
    private readonly ICardRepository _cardRepository;

    public NoteService(
        INotebookRepository notebookRepository,
        INoteRepository noteRepository,
        IColumnRepository columnRepository,
        ICardRepository cardRepository)
    {
        _notebookRepository = notebookRepository;
        _noteRepository = noteRepository;
        _columnRepository = columnRepository;
        _cardRepository = cardRepository;
    }

    public async Task<Note> CreateNoteAsync(string notebookId, string content, string ownerId)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Note content cannot be empty");

        var notebook = await _notebookRepository.GetByIdAsync(notebookId);
        if (notebook == null)
            throw new NotFoundException("Notebook not found");

        var note = new Note
        {
            NotebookId = notebookId,
            Content = content
        };

        return await _noteRepository.AddAsync(note);
    }

    public async Task<IEnumerable<Note>> GetNotebookNotesAsync(string notebookId, string ownerId)
    {
        var notebook = await _notebookRepository.GetByIdAsync(notebookId);
        if (notebook == null)
            throw new NotFoundException("Notebook not found");

        return await _noteRepository.GetByNotebookIdAsync(notebookId);
    }

    public async Task<Note> UpdateNoteAsync(string noteId, string content, string ownerId)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Note content cannot be empty");

        var note = await _noteRepository.GetByIdAsync(noteId);
        if (note == null)
            throw new NotFoundException("Note not found");

        note.Content = content;
        await _noteRepository.UpdateAsync(note);
        
        return note;
    }

    public async Task DeleteNoteAsync(string noteId, string ownerId)
    {
        var note = await _noteRepository.GetByIdAsync(noteId);
        if (note == null)
            throw new NotFoundException("Note not found");

        await _noteRepository.DeleteAsync(noteId);
    }

    public async Task ReorderNotesAsync(string notebookId, Dictionary<string, int> newOrder, string ownerId)
    {
        var notebook = await _notebookRepository.GetByIdAsync(notebookId);
        if (notebook == null)
            throw new NotFoundException("Notebook not found");

        await _noteRepository.ReorderNotesAsync(notebookId, newOrder);
    }

    public async Task ConvertToCardsAsync(string notebookId, string ownerId)
    {
        var notebook = await _notebookRepository.GetByIdAsync(notebookId);
        if (notebook == null)
            throw new NotFoundException("Notebook not found");

        // Создаем новую колонку с названием блокнота
        var column = new Column
        {
            BoardId = notebook.BoardId,
            Name = notebook.Name
        };
        column = await _columnRepository.AddAsync(column);

        // Получаем все заметки из блокнота
        var notes = await _noteRepository.GetByNotebookIdAsync(notebookId);

        // Преобразуем заметки в карточки
        foreach (var note in notes.OrderBy(n => n.Order))
        {
            var card = new Card
            {
                ColumnId = column.Id,
                Title = note.Content.Length > 50 ? note.Content.Substring(0, 50) + "..." : note.Content,
                Description = note.Content
            };
            await _cardRepository.AddAsync(card);
        }

        // Удаляем блокнот и все его заметки
        await _noteRepository.DeleteAsync(notebookId);
        await _notebookRepository.DeleteAsync(notebookId);
    }
}