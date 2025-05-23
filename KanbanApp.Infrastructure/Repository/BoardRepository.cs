// KanbanApp.Infrastructure.Repositories/BoardRepository.cs
using MongoDB.Driver;
using KanbanApp.Domain.Board;

namespace KanbanApp.Infrastructure.Repositories;

public interface IBoardRepository
{
    Task<Board?> GetByIdAsync(string id);
    Task<IEnumerable<Board>> GetByOwnerIdAsync(string ownerId);
    Task<Board> AddAsync(Board board);
    Task UpdateAsync(Board board);
    Task DeleteAsync(string id);
    Task<IEnumerable<Board>> SearchByNameAsync(string ownerId, string name);
}

public class BoardRepository : IBoardRepository
{
    private readonly IMongoCollection<Board> _boards;

    public BoardRepository(IMongoDatabase database)
    {
        _boards = database.GetCollection<Board>("Boards");
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        // Индекс для поиска по владельцу
        var ownerIndex = Builders<Board>.IndexKeys.Ascending(b => b.OwnerId);
        _boards.Indexes.CreateOne(new CreateIndexModel<Board>(ownerIndex));
        
        // Текстовый индекс для поиска по названию
        var textIndex = Builders<Board>.IndexKeys.Text(b => b.Name);
        _boards.Indexes.CreateOne(new CreateIndexModel<Board>(textIndex));
    }

    public async Task<Board?> GetByIdAsync(string id)
        => await _boards.Find(b => b.Id == id).FirstOrDefaultAsync();

    public async Task<IEnumerable<Board>> GetByOwnerIdAsync(string ownerId)
        => await _boards.Find(b => b.OwnerId == ownerId).ToListAsync();

    public async Task<Board> AddAsync(Board board)
    {
        await _boards.InsertOneAsync(board);
        return board;
    }

    public async Task UpdateAsync(Board board)
    {
        board.UpdatedAt = DateTime.UtcNow;
        await _boards.ReplaceOneAsync(b => b.Id == board.Id, board);
    }

    public async Task DeleteAsync(string id)
        => await _boards.DeleteOneAsync(b => b.Id == id);

    public async Task<IEnumerable<Board>> SearchByNameAsync(string ownerId, string name)
        => await _boards.Find(b => b.OwnerId == ownerId && b.Name.Contains(name)).ToListAsync();
}