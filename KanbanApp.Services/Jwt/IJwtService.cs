using KanbanApp.Domain.User;

public interface IJwtService
{
    string GenerateToken(User user);
}