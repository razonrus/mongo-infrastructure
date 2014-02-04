using MongoDB.Driver;

namespace Infrastructure.Database
{
    public interface IMongoInitializerBase
    {
        MongoDatabase GetDatabase(string dbName);
    }
}