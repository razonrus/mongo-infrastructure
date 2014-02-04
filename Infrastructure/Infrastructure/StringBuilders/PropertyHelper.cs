using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Infrastructure.StringBuilders
{
    public static class PropertyHelper
    {
        public static void SetProperty<T, TValue>(T obj, Expression<Func<T, TValue>> get, TValue propValue)
        {
            // re-write in .NET 4.0 as a "set"
            var member = (MemberExpression)get.Body;
            var param = Expression.Parameter(typeof(TValue), "value");
            var set = Expression.Lambda<Action<T, TValue>>(
                Expression.Assign(member, param), get.Parameters[0], param);

            // compile it
            var action = set.Compile();
            action(obj, propValue);
        }

        public static string GetName<T>(Expression<Func<T>> expression)
        {
            return GetName(expression.Body);
        }

        public static string GetName<T>(Expression<Func<T, object>> expression)
        {
            return expression != null ? GetName(expression.Body) : null;
        }

        public static string GetName<T, TProperty>(Expression<Func<T, TProperty>> expression)
        {
            return GetName(expression.Body);
        }

        public static IEnumerable<string> ListNames<T>(params Expression<Func<T, object>>[] expressions)
        {
            return expressions.Select(GetName).ToArray();
        }

        public static string GetName(LambdaExpression expression)
        {
            return GetName(expression.Body);
        }

        private static string GetName(Expression expression)
        {
            var extractor = new NamesExtractor();
            extractor.Visit(expression);
            return extractor.GetName();
        }

        private static void ExtractNames(Expression exp, Stack<string> names)
        {
            if (exp == null)
                return;

            var memberExpression = exp as MemberExpression;
            if (memberExpression != null)
            {
                names.Push(memberExpression.Member.Name);
                ExtractNames(memberExpression.Expression, names);
            }

            switch (exp.NodeType)
            {
                case ExpressionType.ConvertChecked:
                case ExpressionType.Convert:
                {
                    var unaryExpression = exp as UnaryExpression;
                    ExtractNames(names, unaryExpression);
                }
                    break;
            }

            var methodCallExpression = exp as MethodCallExpression;
            if (methodCallExpression != null)
            {
                Expression expression = methodCallExpression.Object ?? methodCallExpression.Arguments.First();
                names.Push(methodCallExpression.Method.Name);
                ExtractNames(expression, names);
            }
        }

        private static void ExtractNames(Stack<string> names, UnaryExpression unaryExpression)
        {
            if (unaryExpression != null)
            {
                ExtractNames(unaryExpression.Operand, names);
            }
        }


        private class NamesExtractor : ExpressionVisitor
        {
            private readonly Stack<string> names = new Stack<string>();

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                names.Push(node.Method.Name);
                return base.VisitMethodCall(node);
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                names.Push(node.Member.Name);
                return base.VisitMember(node);
            }

            public string GetName()
            {
                return string.Join(".", names);
            }
        }

    }
}