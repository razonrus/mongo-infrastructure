using MongoDB.Driver;

namespace Infrastructure.Database
{
    public interface IMongoInitializerBase
    {
        IMongoDatabase GetDatabase(string dbName);
    }
}