using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using Infrastructure.Extensions;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Operations;
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
            var cursor = new Mock<IAsyncCursor<Article>>();
            cursor.Setup(x => x.MoveNext(It.IsAny<CancellationToken>())).Returns(true);
            cursor.SetupGet(x => x.Current).Returns(new[] { CreateArticle() });

            var mongoInitializer = new MockMongoWrapper<IMongoInitializer>()
                .SetupDatabase(x => x.SampleDb, x => x
                    .SetupCollection<User>()
                    .SetupCollection<Article>(
                            m => m.Setup(c => c.FindSync(It.IsAny<FilterDefinition<Article>>(), It.IsAny<FindOptions<Article, Article>>(), It.IsAny<CancellationToken>())).Returns(cursor.Object)
                            ))
                .SetupDatabase(x => x.LogDb,
                    x => x.SetupCollection<Log>())
                .Object;

            

            var count = new GetUserCommentsCountCommand(mongoInitializer).ByArticle(ObjectId.GenerateNewId().ToString(), 1);

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
            var result = new Mock<IFindFluent<Article, Article>>();

            var mongoInitializer = new MockMongoWrapper<IMongoInitializer>()
                .SetupDatabase(x => x.SampleDb, x => x
                    .SetupCollection<Article>(
                        m => m.Setup(c => c.Find(It.IsAny<FilterDefinition<Article>>(), It.IsAny<FindOptions>()))
                            .Returns(result.Object)
                    ))
                            //.Returns(() => new Mock<IFindFluent<Article>>())
                           // new <Article>(m.Object, new List<Article> {CreateArticle()}))))
               .Object;

            var count = new GetUserCommentsCountCommand(mongoInitializer).ByAuthor(2, 1);

            Assert.AreEqual(1, count);
        }
    }
}
