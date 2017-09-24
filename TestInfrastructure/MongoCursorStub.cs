using System.Collections;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Core.Operations;
using Moq;

namespace TestInfrastructure
{
    public class MongoCursorStub<T> : AsyncCursor<T>
    {
        private readonly IEnumerable<T> items;

        public MongoCursorStub(IMongoCollection<T> collection, IEnumerable<T> items)
            : base(null, collection.CollectionNamespace, new BsonDocument(), new List<T>(), 0, null, null, new Mock<IBsonSerializer<T>>().Object, null, null)
        {
            this.items = items;
        }

        //protected override IEnumerator IEnumerableGetEnumerator()
        //{
        //    return items.GetEnumerator();
        //}

        //public override IEnumerator<T> GetEnumerator()
        //{
        //    return items.GetEnumerator();
        //}
    }
}