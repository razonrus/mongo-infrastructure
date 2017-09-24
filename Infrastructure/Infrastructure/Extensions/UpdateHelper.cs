using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Driver;

namespace Infrastructure.Extensions
{
    internal static class UpdateHelper
    {
        public static UpdateDefinition<T> GetUpdater<T>(T entity, IEnumerable<Expression<Func<T, object>>> fields)
        {
            UpdateDefinition<T> update = null;

            foreach (var field in fields)
            {
                var value = field.Compile()(entity);

                var propertyInfo = GetPropertyInfo(field);

                var parameter = Expression.Parameter(typeof(T), "x");
                var body = Expression.PropertyOrField(parameter, propertyInfo.Name);
                var delegateType = typeof(Func<,>).MakeGenericType(typeof(T), propertyInfo.PropertyType);
                var func = Expression.Lambda(delegateType, body, parameter);


                if (update == null)
                {
                    update = (UpdateDefinition<T>) typeof(UpdateDefinitionBuilder<T>)
                        .GetMethods()
                        .Last(x => x.Name == "Set")
                        .MakeGenericMethod(propertyInfo.PropertyType)
                        .Invoke(Builders<T>.Update, new[] {func, value});
                }
                else
                {
                    update = (UpdateDefinition<T>) typeof(UpdateDefinitionExtensions)
                        .GetMethods()
                        .Last(x => x.Name == "Set")
                        .MakeGenericMethod(typeof(T), propertyInfo.PropertyType)
                        .Invoke(update, new[] {update, func, value});
                }
            }
            return update;
        }

        private static PropertyInfo GetPropertyInfo<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertyLambda)
        {
            Type type = typeof(TSource);

            MemberExpression member = propertyLambda.Body as MemberExpression 
                                      ?? ((UnaryExpression)propertyLambda.Body).Operand as MemberExpression;
            if (member == null)
                throw new ArgumentException($"Expression '{propertyLambda}' refers to a method, not a property.");

            PropertyInfo propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException($"Expression '{propertyLambda}' refers to a field, not a property.");

            if (type != propInfo.ReflectedType &&
                !type.IsSubclassOf(propInfo.ReflectedType))
                throw new ArgumentException($"Expresion '{propertyLambda}' refers to a property that is not from type {type}.");

            return propInfo;
        }
    }
}