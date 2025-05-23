// KanbanApp.Infrastructure.Repositories/INotebookRepository.cs
using KanbanApp.Domain.Board;
using MongoDB.Driver;

namespace KanbanApp.Infrastructure.Repositories;

public interface INotebookRepository
{
    Task<Notebook?> GetByIdAsync(string id);
    Task<IEnumerable<Notebook>> GetByBoardIdAsync(string boardId);
    Task<Notebook> AddAsync(Notebook notebook);
    Task UpdateAsync(Notebook notebook);
    Task DeleteAsync(string id);
}

// KanbanApp.Infrastructure.Repositories/NotebookRepository.cs
public class NotebookRepository : INotebookRepository
{
    private readonly IMongoCollection<Notebook> _notebooks;

    public NotebookRepository(IMongoDatabase database)
    {
        _notebooks = database.GetCollection<Notebook>("Notebooks");
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        var boardIndex = Builders<Notebook>.IndexKeys.Ascending(n => n.BoardId);
        _notebooks.Indexes.CreateOne(new CreateIndexModel<Notebook>(boardIndex));
    }

    public async Task<Notebook?> GetByIdAsync(string id)
        => await _notebooks.Find(n => n.Id == id).FirstOrDefaultAsync();

    public async Task<IEnumerable<Notebook>> GetByBoardIdAsync(string boardId)
        => await _notebooks.Find(n => n.BoardId == boardId).ToListAsync();

    public async Task<Notebook> AddAsync(Notebook notebook)
    {
        await _notebooks.InsertOneAsync(notebook);
        return notebook;
    }

    public async Task UpdateAsync(Notebook notebook)
        => await _notebooks.ReplaceOneAsync(n => n.Id == notebook.Id, notebook);

    public async Task DeleteAsync(string id)
        => await _notebooks.DeleteOneAsync(n => n.Id == id);
}