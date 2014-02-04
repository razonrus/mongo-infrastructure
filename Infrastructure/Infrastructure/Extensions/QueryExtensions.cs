using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Infrastructure.StringBuilders;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Infrastructure.Extensions
{
    public static class QueryExtensions
    {
        public static IMongoQuery ElemMatch<T, TItem>(Expression<Func<T, IEnumerable<TItem>>> expression, IMongoQuery query)
        {
            return Query.ElemMatch(PropertyHelper.GetName(expression), query);
        }

        public static IMongoQuery ElemMatch<T>(Expression<Func<T, object>> expression, IMongoQuery query)
        {
            return Query.ElemMatch(PropertyHelper.GetName(expression), query);
        }

        public static IMongoQuery EQ<T>(Expression<Func<T, object>> expression, BsonValue value)
        {
            return Query.EQ(PropertyHelper.GetName(expression), value);
        }
    }
}