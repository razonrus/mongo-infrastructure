using Infrastructure.Database;
using Infrastructure.Extensions;
using MongoDB.Driver.Builders;
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
            SampleDb.Articles.EnsureIndex(IndexKeys<Article>.Ascending(x => x.AuthorId));
        }

        protected override void InitIncrementalIdCounters()
        {
            SampleDb.Users.InitIncrementalIdCounter(0);
        }

        public SampleDatabase SampleDb
        {
            get
            {
                return new SampleDatabase(GetDatabase(dbPrefix + "-sample"));
            }
        }
        public LogDatabase LogDb
        {
            get
            {
                return new LogDatabase(GetDatabase(dbPrefix + "-sample-log"));
            }
        }
    }
}