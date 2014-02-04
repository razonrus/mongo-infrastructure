using Infrastructure.Database;
using Infrastructure.Extensions;
using MongoDB.Driver;
using SampleConsoleApplication.Database.Domain;

namespace SampleConsoleApplication.Database
{
    public class SampleDatabase : MongoDatabaseWrapperBase
    {
        public SampleDatabase(MongoDatabase db) : base(db) { }

        public MongoCollection<User> Users { get { return Database.GetRetryCollection<User>(); } }
        public MongoCollection<Article> Articles { get { return Database.GetRetryCollection<Article>(); } }
    }
}