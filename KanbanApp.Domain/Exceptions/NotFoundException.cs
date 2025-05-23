// KanbanApp.API.Exceptions/NotFoundException.cs
namespace KanbanApp.API.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}