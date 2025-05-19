using KanbanApp.Domain.Login;

namespace KanbanApp.Services.Auth
{

    using KanbanApp.Domain;
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterModel model);
        Task<AuthResponse> LoginAsync(LoginModel model);
    }
}
