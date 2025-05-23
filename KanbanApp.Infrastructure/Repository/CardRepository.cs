// KanbanApp.Infrastructure.Repositories/CardRepository.cs
using MongoDB.Driver;
using KanbanApp.Domain.Board;

namespace KanbanApp.Infrastructure.Repositories;

public interface ICardRepository
{
    Task<Card?> GetByIdAsync(string id);
    Task<IEnumerable<Card>> GetByColumnIdAsync(string columnId);
    Task<Card> AddAsync(Card card);
    Task UpdateAsync(Card card);
    Task DeleteAsync(string id);
    Task MoveCardAsync(string cardId, string newColumnId, int newOrder);
    Task ReorderCardsAsync(string columnId, Dictionary<string, int> newOrder);
}

public class CardRepository : ICardRepository
{
    private readonly IMongoCollection<Card> _cards;

    public CardRepository(IMongoDatabase database)
    {
        _cards = database.GetCollection<Card>("Cards");
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        // Индекс для поиска по колонке
        var columnIndex = Builders<Card>.IndexKeys.Ascending(c => c.ColumnId);
        _cards.Indexes.CreateOne(new CreateIndexModel<Card>(columnIndex));
    }

    public async Task<Card?> GetByIdAsync(string id)
        => await _cards.Find(c => c.Id == id).FirstOrDefaultAsync();

    public async Task<IEnumerable<Card>> GetByColumnIdAsync(string columnId)
        => await _cards.Find(c => c.ColumnId == columnId).SortBy(c => c.Order).ToListAsync();

    public async Task<Card> AddAsync(Card card)
    {
        // Устанавливаем порядок как последний в колонке
        var lastOrder = await _cards.Find(c => c.ColumnId == card.ColumnId)
            .SortByDescending(c => c.Order)
            .Project(c => c.Order)
            .FirstOrDefaultAsync();
        
        card.Order = lastOrder + 1;
        
        await _cards.InsertOneAsync(card);
        return card;
    }

    public async Task UpdateAsync(Card card)
        => await _cards.ReplaceOneAsync(c => c.Id == card.Id, card);

    public async Task DeleteAsync(string id)
        => await _cards.DeleteOneAsync(c => c.Id == id);

    public async Task MoveCardAsync(string cardId, string newColumnId, int newOrder)
    {
        var card = await GetByIdAsync(cardId);
        if (card == null) return;

        // Обновляем порядок карточек в старой колонке
        await _cards.UpdateManyAsync(
            c => c.ColumnId == card.ColumnId && c.Order > card.Order,
            Builders<Card>.Update.Inc(c => c.Order, -1));

        // Обновляем порядок карточек в новой колонке
        await _cards.UpdateManyAsync(
            c => c.ColumnId == newColumnId && c.Order >= newOrder,
            Builders<Card>.Update.Inc(c => c.Order, 1));

        // Перемещаем карточку
        card.ColumnId = newColumnId;
        card.Order = newOrder;
        await UpdateAsync(card);
    }

    public async Task ReorderCardsAsync(string columnId, Dictionary<string, int> newOrder)
    {
        var updates = new List<WriteModel<Card>>();
        
        foreach (var (cardId, order) in newOrder)
        {
            var filter = Builders<Card>.Filter.Eq(c => c.Id, cardId) & 
                         Builders<Card>.Filter.Eq(c => c.ColumnId, columnId);
            
            var update = Builders<Card>.Update.Set(c => c.Order, order);
            
            updates.Add(new UpdateOneModel<Card>(filter, update));
        }
        
        if (updates.Any())
        {
            await _cards.BulkWriteAsync(updates);
        }
    }
}