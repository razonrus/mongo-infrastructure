using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Castle.DynamicProxy;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Infrastructure
{
    public static class MongoExtensions
    {
        const string IdName = "_id";
        static readonly ProxyGenerator ProxyGenerator = new ProxyGenerator();

        public static WriteConcernResult RemoveById(this MongoCollection col, BsonValue id)
        {
            return col.Remove(Query.EQ(IdName, id));
        }
        static readonly ConcurrentDictionary<string, MongoCollection> cache = new ConcurrentDictionary<string, MongoCollection>();

        public static IEnumerable<WriteConcernResult> InsertEnumerable<T>(this MongoCollection<T> collection, IEnumerable<T> sequence)
        {
            if (sequence != null && sequence.Any())
                return collection.InsertBatch(sequence);
            return new WriteConcernResult[0];
        }

        /// <summary>
        /// Returns unique incremental id. In order to work properly, you should call InitIncrementalIdCounter once before usage.
        /// See MongoDatabaseInitializer constructor.
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static long NewId<TDocument>(this MongoCollection<TDocument> collection)
        {
            var col = collection.Database.GetCollection("IncrementalIds");
            var res = col.FindAndModify(Query.EQ(IdName, typeof(TDocument).Name), SortBy.Null, Update.Inc("value", 1L), true, true);
            return (long)res.ModifiedDocument["value"];
        }

        public static void InitIncrementalIdCounter<TDocument>(this MongoCollection<TDocument> collection, long initialValue = 10000)
        {
            var col = collection.Database.GetCollection("IncrementalIds");
            var existing = col.FindOneById(typeof(TDocument).Name);
            if (existing != null) return;
            col.Insert(new { Id = typeof(TDocument).Name, value = initialValue });
        }

        /// <summary>
        /// Returns a MongoCollection decorated with RetryingInterceptor in case of exception, if will retry the operation specified times with pause of specified ms.
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="db"></param>
        /// <param name="collectionName"></param>
        /// <param name="retryCount"></param>
        /// <param name="pauseBetweenRetries"></param>
        /// <returns></returns>
        public static MongoCollection<TDocument> GetRetryCollection<TDocument>(this MongoDatabase db, string collectionName = null, int retryCount = 5, int pauseBetweenRetries = 2000)
        {
            var t = typeof(TDocument).Name;
            if (collectionName == null) collectionName = t;
            var key = string.Concat(db.Name, collectionName, t);
            return (MongoCollection<TDocument>)cache.GetOrAdd(key, k =>
            {
                var coll = db.GetCollection<TDocument>(collectionName);
                return (MongoCollection<TDocument>)ProxyGenerator.CreateClassProxyWithTarget(typeof(MongoCollection<TDocument>), coll, new object[] { db, collectionName, coll.Settings }, new RetryingInterceptor { RetryCount = retryCount, PauseBetweenCalls = pauseBetweenRetries });
            });
        }

        public static MongoCollection<TDocument> GetRetryCollection<TColectionName, TDocument>(this MongoDatabase db)
        {
            var collectionName = typeof(TColectionName).Name;
            return GetRetryCollection<TDocument>(db, collectionName);
        }

        public static WriteConcernResult UpdateById<T>(this MongoCollection<T> collection, BsonValue id, Func<UpdateBuilder<T>, UpdateBuilder<T>> update, MongoUpdateOptions options = null)
        {
            if (options == null) options = new MongoUpdateOptions();
            return collection.Update(Query.EQ(IdName, id), update(new UpdateBuilder<T>()), options);
        }

        public static void UpdateOrInsert<T>(this MongoCollection<T> collection, T prototype, MongoUpdateOptions options = null)
        {
            if (options == null)
            {
                options = new MongoUpdateOptions { Flags = UpdateFlags.Upsert };
            }
            
            var doc = new BsonDocument();
            var wr = BsonWriter.Create(doc);
            BsonSerializer.Serialize(wr, prototype);
            var update = new UpdateBuilder();
            foreach (var el in doc)
            {
                if (el.Name != IdName)
                    update.Set(el.Name, el.Value);
            }

            collection.Update(Query.EQ(IdName, doc[IdName]), update, options);
        }

        public static void ClearCache()
        {
            cache.Clear();
        }
    }
}