using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Castle.DynamicProxy;
using Infrastructure.Interceptors;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Infrastructure.Extensions
{
    public static class MongoExtensions
    {
        const string IdName = "_id";
        static readonly ProxyGenerator ProxyGenerator = new ProxyGenerator();

        public static T FindOneById<T>(this IMongoCollection<T> col, BsonValue id)
        {
            return col.Find(Builders<T>.Filter.Eq(IdName, id)).SingleOrDefault();
        }
        public static DeleteResult RemoveById<T>(this IMongoCollection<T> col, BsonValue id)
        {
            return col.DeleteOne(Builders<T>.Filter.Eq(IdName, id));
        }
        static readonly ConcurrentDictionary<string, object> cache = new ConcurrentDictionary<string, object>();

        public static void InsertEnumerable<T>(this IMongoCollection<T> collection, IEnumerable<T> sequence)
        {
            if (sequence != null && sequence.Any())
                collection.InsertMany(sequence);
        }

        /// <summary>
        /// Returns unique incremental id. In order to work properly, you should call InitIncrementalIdCounter once before usage.
        /// See MongoDatabaseInitializer constructor.
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static long NewId<TDocument>(this IMongoCollection<TDocument> collection)
        {
            var col = collection.Database.GetCollection<BsonDocument>("IncrementalIds");
            var filter = Builders<BsonDocument>.Filter.Eq(IdName, typeof(TDocument).Name);
            var res = col.FindOneAndUpdate(filter, Builders<BsonDocument>.Update.Inc("value", 1L), new FindOneAndUpdateOptions<BsonDocument, BsonDocument>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            });
            
            return (long)res["value"];
        }

        public static void InitIncrementalIdCounter<TDocument>(this IMongoCollection<TDocument> collection, long initialValue = 10000)
        {
            var col = collection.Database.GetCollection<BsonDocument>("IncrementalIds");
            var existing = col.Find(typeof(TDocument).Name);
            if (existing != null) return;
            col.InsertOne(new { Id = typeof(TDocument).Name, value = initialValue }.ToBsonDocument());
        }

        /// <summary>
        /// Returns a IMongoCollection decorated with RetryingInterceptor in case of exception, if will retry the operation specified times with pause of specified ms.
        /// </summary>
        /// <typeparam name="TDocument"></typeparam>
        /// <param name="db"></param>
        /// <param name="collectionName"></param>
        /// <param name="retryCount"></param>
        /// <param name="pauseBetweenRetries"></param>
        /// <returns></returns>
        public static IMongoCollection<TDocument> GetRetryCollection<TDocument>(this IMongoDatabase db, string collectionName = null, int retryCount = 5, int pauseBetweenRetries = 2000)
        {
            var t = typeof(TDocument).Name;
            if (collectionName == null) collectionName = t;
            var key = string.Concat(db.DatabaseNamespace.DatabaseName, collectionName, t);
            return (IMongoCollection<TDocument>)cache.GetOrAdd(key, k =>
            {
                var coll = db.GetCollection<TDocument>(collectionName);
                return (IMongoCollection<TDocument>)ProxyGenerator.CreateClassProxyWithTarget(typeof(IMongoCollection<TDocument>), coll, new object[] { db, collectionName, coll.Settings }, 
                    new RetryingInterceptor { RetryCount = retryCount, PauseBetweenCalls = pauseBetweenRetries });
            });
        }

        public static IMongoCollection<TDocument> GetRetryCollection<TColectionName, TDocument>(this IMongoDatabase db)
        {
            var collectionName = typeof(TColectionName).Name;
            return GetRetryCollection<TDocument>(db, collectionName);
        }

        public static UpdateResult UpdateById<T>(this IMongoCollection<T> collection, BsonValue id, Func<UpdateDefinitionBuilder<T>, UpdateDefinition<T>> update, UpdateOptions options = null)
        {
            id = GetId<T>(id);
            
            return collection.UpdateOne(Builders<T>.Filter.Eq(IdName, id), update(new UpdateDefinitionBuilder<T>()), options ?? new UpdateOptions());
        }

          public static UpdateResult UpdateByIdOrInsert<T>(this IMongoCollection<T> collection, BsonValue id,
             Func<UpdateDefinitionBuilder<T>, UpdateDefinition<T>> update) where T : new()
         {
            return collection.UpdateById(id, update, new UpdateOptions
            {
                IsUpsert = true
            });
         }

        public static UpdateResult UpdateArrayById<T, TItem>(this IMongoCollection<T> collection, BsonValue id,
            Expression<Func<T, IEnumerable<TItem>>> array, FilterDefinition<TItem> elemSelector,
            Func<UpdateDefinitionBuilder<T>, Expression<Func<T, IEnumerable<TItem>>>, UpdateDefinition<T>> update)
        {
            id = GetId<T>(id);

            return collection.UpdateMany(
                Builders<T>.Filter.And(
                    Builders<T>.Filter.Eq("_id", id),
                    QueryExtensions.ElemMatch(array,
                        elemSelector
                        )
                    ),
                update(new UpdateDefinitionBuilder<T>(), array)
                );
        }

        private static BsonValue GetId<T>(BsonValue id)
        {
            if (typeof(T).GetProperties()
                .Any(x => (x.Name == "Id" || x.GetCustomAttributes(typeof(BsonIdAttribute), true).Any())
                          && x.GetCustomAttributes(typeof(BsonRepresentationAttribute), true).Any()))
                id = new ObjectId(id.ToString());
            return id;
        }

        public static void UpdateOrInsert<T>(this IMongoCollection<T> collection, T prototype, UpdateOptions options = null)
        {
            if (options == null)
            {
                options = new UpdateOptions { IsUpsert = true};
            }
            
            var doc = new BsonDocument();
            var wr = new BsonDocumentWriter(doc);
            BsonSerializer.Serialize(wr, prototype);
            var update = Builders<T>.Update;
            UpdateDefinition<T> definition = null;
            foreach (var el in doc)
            {
                if (el.Name != IdName)
                    definition = definition == null ? update.Set(el.Name, el.Value) : definition.Set(el.Name, el.Value);
            }

            collection.UpdateOne(Builders<T>.Filter.Eq(IdName, doc[IdName]), definition, options);
        }

        public static void ClearCache()
        {
            cache.Clear();
        }
    }
}