﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Infrastructure.StringBuilders;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Extensions
{
    public static class UpdateExtensions
    {
        public static UpdateDefinition<T> Set<T, TItem>(this UpdateDefinitionBuilder<T> builder, Expression<Func<T, IEnumerable<TItem>>> list, Expression<Func<TItem, object>> item, BsonValue value)
        {
            return builder.Set(NameForQueryBuilder.GetName(list, item), value);
        }
        public static UpdateDefinition<T> Inc<T, TItem>(this UpdateDefinitionBuilder<T> builder, Expression<Func<T, IEnumerable<TItem>>> list, Expression<Func<TItem, object>> item, double value)
        {
            return builder.Inc(NameForQueryBuilder.GetName(list, item), value);
        }

        public static UpdateDefinition<T> Unset<T, TItem>(this UpdateDefinitionBuilder<T> builder, Expression<Func<T, IEnumerable<TItem>>> list, Expression<Func<TItem, object>> item)
        {
            return builder.Unset(NameForQueryBuilder.GetName(list, item));
        }
    }
}