using Infrastructure.Database;
using Infrastructure.Extensions;
using MongoDB.Driver;
using SampleConsoleApplication.Database.Domain;

namespace SampleConsoleApplication.Database
{
    public interface IMongoInitializer
    {
        SampleDatabase SampleDb { get; }
        LogDatabase LogDb { get; }
    }

    public class MongoInitializer : MongoInitializerBase, IMongoInitializer
    {
        protected override void CreateIndexes()
        {
            SampleDb.Articles.Indexes.CreateOne(Builders<Article>.IndexKeys.Ascending(x => x.AuthorId));
        }

        protected override void InitIncrementalIdCounters()
        {
            SampleDb.Users.InitIncrementalIdCounter(0);
        }

        public SampleDatabase SampleDb => new SampleDatabase(GetDatabase(DbPrefix + "-sample"));

        public LogDatabase LogDb => new LogDatabase(GetDatabase(DbPrefix + "-sample-log"));
    }
}