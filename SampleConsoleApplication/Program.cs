using System.Collections.Generic;
using System.Linq;
using Infrastructure.Extensions;
using MongoDB.Driver.Linq;
using SampleConsoleApplication.Database;
using SampleConsoleApplication.Database.Domain;

namespace SampleConsoleApplication
{
    class Program
    {
        public static MongoInitializer MongoInitializer;

        private static void Main()
        {
            MongoInitializer = new MongoInitializer();

            var user = new User
            {
                Id = MongoInitializer.SampleDb.Users.NewId()
            };

            MongoInitializer.SampleDb.Users.Insert(user);
            MongoInitializer.SampleDb.Articles.Insert(new Article
            {
                AuthorId = user.Id
            });
            
            var article = MongoInitializer.SampleDb.Articles.AsQueryable().First(x => x.AuthorId == user.Id);

            MongoInitializer.SampleDb.Articles.UpdateById(article.Id, x => x.PushAll(a => a.Comments, new List<Comment>
            {
                new Comment(),
                new Comment
                {
                    UserId = user.Id
                }
            }));

            MongoInitializer.SampleDb.Articles.UpdateArrayById(article.Id,
                x => x.Comments,
                QueryExtensions.EQ<Comment>(a => a.UserId, user.Id),
                (builder, array) => builder.Set(array, c => c.Text, "new text"));
        }
    }
}
