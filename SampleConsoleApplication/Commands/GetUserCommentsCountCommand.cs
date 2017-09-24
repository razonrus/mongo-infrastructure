using System.Linq;
using Infrastructure.Extensions;
using MongoDB.Driver;
using SampleConsoleApplication.Database;
using SampleConsoleApplication.Database.Domain;

namespace SampleConsoleApplication.Commands
{
    public class GetUserCommentsCountCommand
    {
        private readonly IMongoInitializer mongoInitializer;

        public GetUserCommentsCountCommand(IMongoInitializer initializer)
        {
            mongoInitializer = initializer;
        }

        //it is advisable to make this method like "ByArticle(Article article, long userId)" in production code.
        //current code is bad, it шы only for demonstration how to mock a mongo in tests
        public int ByArticle(string articleId, long userId)
        {
            var article = mongoInitializer.SampleDb.Articles.FindOneById(articleId);

            var count = article.Comments?.Count(x => x.UserId == userId) ?? 0;


            //need for demonstrate configuring multiple collections in test
            mongoInitializer.SampleDb.Users.UpdateById(userId, x => x.Inc(u => u.CommentsCount, count));

            //need for demonstrate configuring multiple databases in test
            mongoInitializer.LogDb.Logs.InsertOne(new Log());

            return count;
        }

        public int ByAuthor(long authorId, long userId)
        {
            var articles = mongoInitializer.SampleDb.Articles.Find(Builders<Article>.Filter.Eq("AuthorId", authorId)).ToList();

            return articles.SelectMany(x => x.Comments).Where(x => x != null).Count(x => x.UserId == userId);
        }
    }
}