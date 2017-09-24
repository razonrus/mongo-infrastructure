using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Infrastructure.StringBuilders;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Extensions
{
    public static class QueryExtensions
    {
        public static FilterDefinition<T> ElemMatch<T, TItem>(Expression<Func<T, IEnumerable<TItem>>> expression, FilterDefinition<T> query)
        {
            return Builders<T>.Filter.ElemMatch(PropertyHelper.GetName(expression), query);
        }

        public static FilterDefinition<T> ElemMatch<T>(Expression<Func<T, object>> expression, FilterDefinition<T> query)
        {
            return Builders<T>.Filter.ElemMatch(PropertyHelper.GetName(expression), query);
        }

        public static FilterDefinition<T> EQ<T>(Expression<Func<T, object>> expression, BsonValue value)
        {
            return Builders<T>.Filter.Eq(PropertyHelper.GetName(expression), value);
        }
    }
}