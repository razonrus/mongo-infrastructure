using System;
using System.Configuration;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Infrastructure.Database
{
    public abstract class MongoInitializerBase : IMongoInitializerBase
    {
        public MongoClient MongoClient;
        protected static string DbPrefix;

        protected MongoInitializerBase()
        {
            InitializeDatabase(ConfigurationManager.ConnectionStrings["MongoDbConnectionString"].ConnectionString);
            CreateIndexes();
            InitIncrementalIdCounters();
        }

        protected virtual void InitIncrementalIdCounters()
        {
        }

        protected virtual void CreateIndexes()
        {
        }


        private void InitializeDatabase(string connectionString)
        {
            // Specify, that null values should be ignored by BsonSerializer
            var p = new ConventionPack
            {
                new IgnoreIfNullConvention(true), 
                new IgnoreExtraElementsConvention(true)
            };
            ConventionRegistry.Register("all", p, t => true);

            var urlBuilder = new MongoUrlBuilder(connectionString);
            DbPrefix = urlBuilder.DatabaseName;
            if (string.IsNullOrWhiteSpace(DbPrefix))
                throw new ArgumentException("Connection string must include database name prefix. E.g. mongodb://mongodb01/staging");
            MongoClient = new MongoClient(connectionString);
        }

        public IMongoDatabase GetDatabase(string dbName)
        {
            return MongoClient.GetDatabase(dbName);
        }
    }


}