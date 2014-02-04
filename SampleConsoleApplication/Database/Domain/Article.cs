using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SampleConsoleApplication.Database.Domain
{
    public class Article
    {
        public Article()
        {
        }

        [BsonRepresentation((BsonType.ObjectId))]
        public string Id { get; set; }

        public long AuthorId { get; set; }

        public List<Comment> Comments { get; set; }
    }
}