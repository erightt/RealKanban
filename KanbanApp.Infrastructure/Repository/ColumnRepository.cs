// KanbanApp.Infrastructure.Repositories/ColumnRepository.cs
using MongoDB.Driver;
using KanbanApp.Domain.Board;

namespace KanbanApp.Infrastructure.Repositories;

public interface IColumnRepository
{
    Task<Column?> GetByIdAsync(string id);
    Task<IEnumerable<Column>> GetByBoardIdAsync(string boardId);
    Task<Column> AddAsync(Column column);
    Task UpdateAsync(Column column);
    Task DeleteAsync(string id);
    Task ReorderColumnsAsync(string boardId, Dictionary<string, int> newOrder);
}

public class ColumnRepository : IColumnRepository
{
    private readonly IMongoCollection<Column> _columns;

    public ColumnRepository(IMongoDatabase database)
    {
        _columns = database.GetCollection<Column>("Columns");
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        // Индекс для поиска по доске
        var boardIndex = Builders<Column>.IndexKeys.Ascending(c => c.BoardId);
        _columns.Indexes.CreateOne(new CreateIndexModel<Column>(boardIndex));
    }

    public async Task<Column?> GetByIdAsync(string id)
        => await _columns.Find(c => c.Id == id).FirstOrDefaultAsync();

    public async Task<IEnumerable<Column>> GetByBoardIdAsync(string boardId)
        => await _columns.Find(c => c.BoardId == boardId).SortBy(c => c.Order).ToListAsync();

    public async Task<Column> AddAsync(Column column)
    {
        // Устанавливаем порядок как последний в доске
        var lastOrder = await _columns.Find(c => c.BoardId == column.BoardId)
            .SortByDescending(c => c.Order)
            .Project(c => c.Order)
            .FirstOrDefaultAsync();
        
        column.Order = lastOrder + 1;
        
        await _columns.InsertOneAsync(column);
        return column;
    }

    public async Task UpdateAsync(Column column)
        => await _columns.ReplaceOneAsync(c => c.Id == column.Id, column);

    public async Task DeleteAsync(string id)
        => await _columns.DeleteOneAsync(c => c.Id == id);

    public async Task ReorderColumnsAsync(string boardId, Dictionary<string, int> newOrder)
    {
        var updates = new List<WriteModel<Column>>();
        
        foreach (var (columnId, order) in newOrder)
        {
            var filter = Builders<Column>.Filter.Eq(c => c.Id, columnId) & 
                         Builders<Column>.Filter.Eq(c => c.BoardId, boardId);
            
            var update = Builders<Column>.Update.Set(c => c.Order, order);
            
            updates.Add(new UpdateOneModel<Column>(filter, update));
        }
        
        if (updates.Any())
        {
            await _columns.BulkWriteAsync(updates);
        }
    }
}