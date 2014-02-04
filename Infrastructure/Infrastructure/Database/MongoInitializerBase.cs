using System;
using System.Configuration;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Infrastructure.Database
{
    public abstract class MongoInitializerBase : IMongoInitializerBase
    {
        private static MongoClient mongoClient;
        private static MongoServer mongoServer;
        protected static string dbPrefix;

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
            dbPrefix = urlBuilder.DatabaseName;
            if (string.IsNullOrWhiteSpace(dbPrefix))
                throw new ArgumentException("Connection string must include database name prefix. E.g. mongodb://mongodb01/staging");
            mongoClient = new MongoClient(connectionString);
            mongoServer = mongoClient.GetServer();
        }

        public MongoDatabase GetDatabase(string dbName)
        {
            return mongoServer.GetDatabase(dbName);
        }
    }


}