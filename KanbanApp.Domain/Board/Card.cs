// KanbanApp.Domain.Board/Card.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KanbanApp.Domain.Board;

public class Card
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    public string ColumnId { get; set; } = null!; // ID колонки, к которой принадлежит карточка
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int Order { get; set; } // Порядок карточки в колонке
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
}