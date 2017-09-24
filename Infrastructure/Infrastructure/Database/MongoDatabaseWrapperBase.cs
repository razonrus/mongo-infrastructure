using MongoDB.Driver;

namespace Infrastructure.Database
{
    public abstract class MongoDatabaseWrapperBase
    {
        protected MongoDatabaseWrapperBase(IMongoDatabase db)
        {
            Database = db;
        }

        public IMongoDatabase Database { get; private set; }
    }
}