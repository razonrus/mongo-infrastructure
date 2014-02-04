using Infrastructure.Database;
using Infrastructure.Extensions;
using MongoDB.Driver;
using SampleConsoleApplication.Database.Domain;

namespace SampleConsoleApplication.Database
{
    public class LogDatabase : MongoDatabaseWrapperBase
    {
        public LogDatabase(MongoDatabase db) : base(db) { }

        public MongoCollection<Log> Logs { get { return Database.GetRetryCollection<Log>(); } }
    }
}