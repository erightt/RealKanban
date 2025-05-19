namespace KanbanApp.API.Exceptions;
public class RegistrationException : Exception
{
    public RegistrationException(string message) : base(message) { }
}