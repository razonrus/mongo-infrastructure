using MongoDB.Driver;

namespace Infrastructure.Database
{
    public abstract class MongoDatabaseWrapperBase
    {
        protected MongoDatabaseWrapperBase(MongoDatabase db)
        {
            Database = db;
        }

        public MongoDatabase Database { get; private set; }
    }
}