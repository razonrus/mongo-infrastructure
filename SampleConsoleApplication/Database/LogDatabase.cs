using Infrastructure.Database;
using Infrastructure.Extensions;
using MongoDB.Driver;
using SampleConsoleApplication.Database.Domain;

namespace SampleConsoleApplication.Database
{
    public class LogDatabase : MongoDatabaseWrapperBase
    {
        public LogDatabase(IMongoDatabase db) : base(db) { }

        public IMongoCollection<Log> Logs => Database.GetCollection<Log>(typeof(Log).Name);
    }
}