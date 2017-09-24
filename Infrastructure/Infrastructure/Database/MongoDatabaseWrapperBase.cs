using MongoDB.Driver;

namespace Infrastructure.Database
{
    public abstract class MongoDatabaseWrapperBase
    {
        protected MongoDatabaseWrapperBase(IMongoDatabase db)
        {
            Database = db;
        }

        public IMongoDatabase Database { get; }

        protected IMongoCollection<T> GetCollection<T>()
        {
            return Database.GetCollection<T>(typeof(T).Name);
        }
    }
}