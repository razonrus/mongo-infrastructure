using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Infrastructure.StringBuilders
{
    public static class NameForQueryBuilder
    {
        public static string GetName<T, TItem>(Expression<Func<T, IEnumerable<TItem>>> list, Expression<Func<TItem, object>> item)
        {
            return string.Format("{0}.$.{1}", PropertyHelper.GetName(list), PropertyHelper.GetName(item));
        }
    }
}