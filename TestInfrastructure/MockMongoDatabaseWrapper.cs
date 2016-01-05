using System;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;

namespace TestInfrastructure
{
    public class MockMongoDatabaseWrapper
    {
        private readonly Mock<MongoDatabase> database;

        public MockMongoDatabaseWrapper(Mock<MongoDatabase> database)
        {
            this.database = database;
        }

        public MockMongoDatabaseWrapper SetupCollection<T>(Action<Mock<MongoCollection<T>>> setupAction = null)
        {
            var mongoCollectionSettings = new MongoCollectionSettings
            {
                GuidRepresentation = GuidRepresentation.Standard,
                ReadEncoding = new UTF8Encoding(),
                ReadPreference = new ReadPreference(ReadPreferenceMode.Nearest),
                WriteConcern = new WriteConcern(),
                WriteEncoding = new UTF8Encoding(),
                ReadConcern = new ReadConcern()
            };

            var collection = new Mock<MongoCollection<T>>(database.Object, "test", mongoCollectionSettings);
            collection.Setup(x => x.Update(It.IsAny<IMongoQuery>(), It.IsAny<IMongoUpdate>(), It.IsAny<MongoUpdateOptions>())).Returns((WriteConcernResult)null);

            if (setupAction != null)
                setupAction(collection);

            collection.SetupGet(x => x.Settings).Returns(mongoCollectionSettings);
            collection.SetupGet(x => x.Database).Returns(database.Object);
            database.Setup(x => x.GetCollection<T>(typeof(T).Name)).Returns(collection.Object);

            return this;
        }
    }
}
