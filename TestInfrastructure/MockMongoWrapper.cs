using System;
using System.Linq.Expressions;
using System.Text;
using Infrastructure.Database;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;

namespace TestInfrastructure
{
    public class MockMongoWrapper<TInitializer> where TInitializer : class
    {
        private readonly Mock<MongoClient> client;
        private readonly Mock<TInitializer> mock;

        public MockMongoWrapper()
        {
            client = new Mock<MongoClient>();
            client.SetupGet(x => x.Settings).Returns(new MongoClientSettings());

            mock = new Mock<TInitializer>();
        }

        private static Mock<IMongoDatabase> GetDatabase<T>(Mock<MongoClient> server)
        {
            var mongoDatabaseSettings = new MongoDatabaseSettings
            {
                GuidRepresentation = GuidRepresentation.Standard,
                ReadEncoding = new UTF8Encoding(),
                ReadPreference = new ReadPreference(ReadPreferenceMode.Nearest),
                WriteConcern = new WriteConcern(),
                WriteEncoding = new UTF8Encoding()
            };
            var database = new Mock<IMongoDatabase>();
            database.SetupGet(x => x.Settings).Returns(mongoDatabaseSettings);
            database.SetupGet(x => x.DatabaseNamespace).Returns(new DatabaseNamespace(typeof(T).Name));
            return database;
        }

        public MockMongoWrapper<TInitializer> SetupDatabase<T>(Expression<Func<TInitializer, T>> databaseGetter, Func<MockMongoDatabaseWrapper, MockMongoDatabaseWrapper> action = null)
            where T : MongoDatabaseWrapperBase
        {
            Mock<IMongoDatabase> database = GetDatabase<T>(client);

            mock.SetupGet(databaseGetter).Returns((T)Activator.CreateInstance(typeof(T), database.Object));

            action?.Invoke(new MockMongoDatabaseWrapper(database));

            return this;
        }

        public TInitializer Object => mock.Object;
    }
}