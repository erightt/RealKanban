// KanbanApp.Domain.Board/Note.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KanbanApp.Domain.Board;

public class Note
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    public string NotebookId { get; set; } = null!; // ID блокнота, к которому принадлежит заметка
    public string Content { get; set; } = null!; // Содержимое заметки
    public int Order { get; set; } // Порядок заметки в блокноте
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}