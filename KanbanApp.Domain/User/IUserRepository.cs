namespace KanbanApp.Domain.User;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task<IEnumerable<User>> GetAllAsync();
    Task DeleteAsync(User user);
}