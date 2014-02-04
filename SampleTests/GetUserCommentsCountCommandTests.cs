using System.Collections.Generic;
using Infrastructure.Extensions;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;
using SampleConsoleApplication.Commands;
using SampleConsoleApplication.Database;
using SampleConsoleApplication.Database.Domain;
using TestInfrastructure;

namespace SampleTests
{
    [TestFixture]
    public class GetUserCommentsCountCommandTests
    {
        [TearDown]
        public void ClearCache()
        {
            MongoExtensions.ClearCache();
        }

        [Test]
        public void ByArticleTest()
        {
            var mongoInitializer = new MockMongoWrapper<IMongoInitializer>()
                .SetupDatabase(x => x.SampleDb, x => x
                    .SetupCollection<User>()
                    .SetupCollection<Article>(
                            m => m.Setup(c => c.FindOneById("")).Returns(CreateArticle())))
                .SetupDatabase(x => x.LogDb,
                    x => x.SetupCollection<Log>())
                .Object;

            var count = new GetUserCommentsCountCommand(mongoInitializer).ByArticle("", 1);

            Assert.AreEqual(1, count);
        }

        private static Article CreateArticle()
        {
            return new Article
            {
                Comments = new List<Comment> {new Comment {UserId = 1}}
            };
        }

        [Test]
        public void ByAuthorTest()
        {
            var mongoInitializer = new MockMongoWrapper<IMongoInitializer>()
                .SetupDatabase(x => x.SampleDb, x => x
                    .SetupCollection<Article>(
                        m => m.Setup(c => c.Find(It.IsAny<IMongoQuery>()))
                            .Returns(() => new MongoCursorStub<Article>(m.Object, new List<Article> {CreateArticle()}))))
                .Object;

            var count = new GetUserCommentsCountCommand(mongoInitializer).ByAuthor(2, 1);

            Assert.AreEqual(1, count);
        }
    }
}
