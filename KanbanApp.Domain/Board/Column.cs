// KanbanApp.Domain.Board/Column.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KanbanApp.Domain.Board;

public class Column
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    public string BoardId { get; set; } = null!; // ID доски, к которой принадлежит колонка
    public string Name { get; set; } = null!;
    public int Order { get; set; } // Порядок колонки на доске
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}