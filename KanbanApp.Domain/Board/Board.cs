// KanbanApp.Domain.Board/Board.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using KanbanApp.Domain.User;

namespace KanbanApp.Domain.Board;

public class Board
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;
    public string OwnerId { get; set; } = null!; 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}