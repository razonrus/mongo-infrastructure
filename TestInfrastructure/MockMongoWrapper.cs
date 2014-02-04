using System;
using System.Linq.Expressions;
using System.Text;
using Infrastructure.Database;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;

namespace TestInfrastructure
{
    public class MockMongoWrapper
    {
        private readonly Mock<MongoServer> server;
        private readonly Mock<IMongoInitializerBase> mock;

        public MockMongoWrapper()
        {
            string message;
            server = new Mock<MongoServer>(new MongoServerSettings());
            server.Setup(s => s.IsDatabaseNameValid(It.IsAny<string>(), out message)).Returns(true);

            mock = new Mock<IMongoInitializerBase>();
        }

        private static Mock<MongoDatabase> GetDatabase(Mock<MongoServer> server)
        {
            var mongoDatabaseSettings = new MongoDatabaseSettings
            {
                GuidRepresentation = GuidRepresentation.Standard,
                ReadEncoding = new UTF8Encoding(),
                ReadPreference = new ReadPreference(),
                WriteConcern = new WriteConcern(),
                WriteEncoding = new UTF8Encoding()
            };
            var database = new Mock<MongoDatabase>(server.Object, "test", mongoDatabaseSettings);
            string message;
            database.Setup(x => x.IsCollectionNameValid(It.IsAny<string>(), out message)).Returns(true);
            return database;
        }

        public MockMongoWrapper SetupDatabase<T>(Expression<Func<IMongoInitializerBase, T>> databaseGetter, Func<MockMongoDatabaseWrapper, MockMongoDatabaseWrapper> action = null)
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

        public IMongoInitializerBase Object
        {
            get { return mock.Object; }
        }
    }
}