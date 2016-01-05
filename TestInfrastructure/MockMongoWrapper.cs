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
        private readonly Mock<MongoServer> server;
        private readonly Mock<TInitializer> mock;

        public MockMongoWrapper()
        {
            string message;
            server = new Mock<MongoServer>(new MongoServerSettings());
            server.Setup(s => s.IsDatabaseNameValid(It.IsAny<string>(), out message)).Returns(true);

            mock = new Mock<TInitializer>();
        }

        private static Mock<MongoDatabase> GetDatabase(Mock<MongoServer> server)
        {
            var mongoDatabaseSettings = new MongoDatabaseSettings
            {
                GuidRepresentation = GuidRepresentation.Standard,
                ReadEncoding = new UTF8Encoding(),
                ReadPreference = new ReadPreference(ReadPreferenceMode.Nearest),
                WriteConcern = new WriteConcern(),
                WriteEncoding = new UTF8Encoding()
            };
            var database = new Mock<MongoDatabase>(server.Object, "test", mongoDatabaseSettings);
            string message;
            database.Setup(x => x.IsCollectionNameValid(It.IsAny<string>(), out message)).Returns(true);
            return database;
        }

        public MockMongoWrapper<TInitializer> SetupDatabase<T>(Expression<Func<TInitializer, T>> databaseGetter, Func<MockMongoDatabaseWrapper, MockMongoDatabaseWrapper> action = null)
            where T : MongoDatabaseWrapperBase
        {
            Mock<MongoDatabase> database = GetDatabase(server);

            mock.SetupGet(databaseGetter).Returns((T)Activator.CreateInstance(typeof(T), database.Object));

            if (action != null)
            {
                action(new MockMongoDatabaseWrapper(database));
            }

            return this;
        }

        public TInitializer Object
        {
            get { return mock.Object; }
        }
    }
}