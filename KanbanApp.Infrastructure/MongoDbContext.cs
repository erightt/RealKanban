using MongoDB.Driver;

namespace KanbanApp.Infrastructure;

public class MongoDbContext
{
    public IMongoDatabase Database { get; }

    public MongoDbContext(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        Database = client.GetDatabase(databaseName);
    }
}