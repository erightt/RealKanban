// KanbanApp.Infrastructure.Repositories/INoteRepository.cs
using KanbanApp.Domain.Board;
using MongoDB.Driver;

namespace KanbanApp.Infrastructure.Repositories;

public interface INoteRepository
{
    Task<Note?> GetByIdAsync(string id);
    Task<IEnumerable<Note>> GetByNotebookIdAsync(string notebookId);
    Task<Note> AddAsync(Note note);
    Task UpdateAsync(Note note);
    Task DeleteAsync(string id);
    Task ReorderNotesAsync(string notebookId, Dictionary<string, int> newOrder);
}

public class NoteRepository : INoteRepository
{
    private readonly IMongoCollection<Note> _notes;

    public NoteRepository(IMongoDatabase database)
    {
        _notes = database.GetCollection<Note>("Notes");
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        var notebookIndex = Builders<Note>.IndexKeys.Ascending(n => n.NotebookId);
        _notes.Indexes.CreateOne(new CreateIndexModel<Note>(notebookIndex));
    }

    public async Task<Note?> GetByIdAsync(string id)
        => await _notes.Find(n => n.Id == id).FirstOrDefaultAsync();

    public async Task<IEnumerable<Note>> GetByNotebookIdAsync(string notebookId)
        => await _notes.Find(n => n.NotebookId == notebookId).SortBy(n => n.Order).ToListAsync();

    public async Task<Note> AddAsync(Note note)
    {
        var lastOrder = await _notes.Find(n => n.NotebookId == note.NotebookId)
            .SortByDescending(n => n.Order)
            .Project(n => n.Order)
            .FirstOrDefaultAsync();
        
        note.Order = lastOrder + 1;
        
        await _notes.InsertOneAsync(note);
        return note;
    }

    public async Task UpdateAsync(Note note)
    {
        note.UpdatedAt = DateTime.UtcNow;
        await _notes.ReplaceOneAsync(n => n.Id == note.Id, note);
    }

    public async Task DeleteAsync(string id)
        => await _notes.DeleteOneAsync(n => n.Id == id);

    public async Task ReorderNotesAsync(string notebookId, Dictionary<string, int> newOrder)
    {
        var updates = new List<WriteModel<Note>>();
        
        foreach (var (noteId, order) in newOrder)
        {
            var filter = Builders<Note>.Filter.Eq(n => n.Id, noteId) & 
                         Builders<Note>.Filter.Eq(n => n.NotebookId, notebookId);
            
            var update = Builders<Note>.Update.Set(n => n.Order, order);
            
            updates.Add(new UpdateOneModel<Note>(filter, update));
        }
        
        if (updates.Any())
        {
            await _notes.BulkWriteAsync(updates);
        }
    }
}