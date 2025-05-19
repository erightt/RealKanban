using MongoDB.Driver;
using KanbanApp.Domain.User;

namespace KanbanApp.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _users;

    public UserRepository(IMongoDatabase database)
    {
        _users = database.GetCollection<User>("Users");
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        var indexKeys = Builders<User>.IndexKeys
            .Ascending(u => u.Email)
            .Ascending(u => u.Username);
        
        _users.Indexes.CreateOne(new CreateIndexModel<User>(
            indexKeys, 
            new CreateIndexOptions { Unique = true }));
    }

    public async Task<User?> GetByIdAsync(string id) 
        => await _users.Find(u => u.Id == id).FirstOrDefaultAsync();

    public async Task<User?> GetByUsernameAsync(string username) 
        => await _users.Find(u => u.Username == username).FirstOrDefaultAsync();

    public async Task<User?> GetByEmailAsync(string email) 
        => await _users.Find(u => u.Email == email).FirstOrDefaultAsync();

    public async Task AddAsync(User user) 
        => await _users.InsertOneAsync(user);

    public async Task UpdateAsync(User user) 
        => await _users.ReplaceOneAsync(u => u.Id == user.Id, user);

    public async Task<IEnumerable<User>> GetAllAsync() 
        => await _users.Find(_ => true).ToListAsync();
}