using Infrastructure.Database;
using MongoDB.Driver;
using SampleConsoleApplication.Database.Domain;

namespace SampleConsoleApplication.Database
{
    public class SampleDatabase : MongoDatabaseWrapperBase
    {
        public SampleDatabase(IMongoDatabase db) : base(db) { }

        public IMongoCollection<User> Users => Database.GetCollection<User>(typeof(User).Name);
        public IMongoCollection<Article> Articles => Database.GetCollection<Article>(typeof(Article).Name);
    }
}