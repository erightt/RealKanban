using KanbanApp.Domain.User;

public class AuthResponse
{
    public string Token { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string Username { get; set; } = null!;
    public UserRole Role { get; set; }
}