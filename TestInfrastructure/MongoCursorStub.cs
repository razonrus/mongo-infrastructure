using System.Collections;
using System.Collections.Generic;
using MongoDB.Driver;

namespace TestInfrastructure
{
    public class MongoCursorStub<T> : MongoCursor<T>
    {
        private readonly IEnumerable<T> items;

        public MongoCursorStub(MongoCollection<T> collection, IEnumerable<T> items)
            : base(collection, null, null, null, null)
        {
            this.items = items;
        }

        protected override IEnumerator IEnumerableGetEnumerator()
        {
            return items.GetEnumerator();
        }

        public override IEnumerator<T> GetEnumerator()
        {
            return items.GetEnumerator();
        }
    }
}