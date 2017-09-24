using System;
using System.Text;
using System.Threading;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;

namespace TestInfrastructure
{
    public class MockMongoDatabaseWrapper
    {
        private readonly Mock<IMongoDatabase> database;

        public MockMongoDatabaseWrapper(Mock<IMongoDatabase> database)
        {
            this.database = database;
        }

        public MockMongoDatabaseWrapper SetupCollection<T>(Action<Mock<IMongoCollection<T>>> setupAction = null)
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
            
            var collection = new Mock<IMongoCollection<T>>();
            collection.Setup(x => x.UpdateOne(It.IsAny<FilterDefinition<T>>(), It.IsAny<UpdateDefinition<T>>(), It.IsAny<UpdateOptions>(), It.IsAny<CancellationToken>())).Returns((UpdateResult)null);

            setupAction?.Invoke(collection);

            collection.SetupGet(x => x.CollectionNamespace).Returns(new CollectionNamespace(database.Object.DatabaseNamespace, typeof(T).Name));
            collection.SetupGet(x => x.Settings).Returns(mongoCollectionSettings);
            collection.SetupGet(x => x.Database).Returns(database.Object);
            database.Setup(x => x.GetCollection<T>(typeof(T).Name, It.IsAny<MongoCollectionSettings>())).Returns(collection.Object);

            return this;
        }
    }
}
