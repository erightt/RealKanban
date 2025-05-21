using KanbanApp.Domain.Login;

namespace KanbanApp.Services.Auth
{

    using KanbanApp.Domain;
    using KanbanApp.Domain.User;

    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterModel model);
        Task<AuthResponse> LoginAsync(LoginModel model);
        Task<IEnumerable<User>> GetAllUsersAsync(); // Добавляем
        Task DeleteUserAsync(string userIdToDelete, string currentUserId);
    }
}
