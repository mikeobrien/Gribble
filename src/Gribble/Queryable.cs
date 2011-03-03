using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Gribble
{
    public static class Queryable
    {
        // -------------------- Queryable -----------------------

        public static IQueryable<TSource> Empty<TSource>()
        {
            return (IQueryable<TSource>)Enumerable.Empty<TSource>();
        }

        public static IQueryable<TSource> Randomize<TSource>(this IQueryable<TSource> source)
        {
            if (source == null) throw new ArgumentNullException("source");
            return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo) MethodBase.GetCurrentMethod()).MakeGenericMethod(new [] { typeof(TSource) }), new [] { source.Expression }));
        }

        public static IQueryable<TSource> TakePercent<TSource>(this IQueryable<TSource> source, int percent)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (percent < 0 || percent > 100) throw new ArgumentOutOfRangeException("percent", "Percent must be between 0 and 100.");
            return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(new [] { typeof(TSource) }), new [] { source.Expression, Expression.Constant(percent) }));
        }

        public static IQueryable<TSource> CopyTo<TSource>(this IQueryable<TSource> source, string target)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (target == null) throw new ArgumentNullException("target");
            return source.Provider.Execute<IQueryable<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(new[] { typeof(TSource) }), new[] { source.Expression, Expression.Constant(target) }));
        }

        public static IQueryable<TSource> CopyTo<TSource>(this IQueryable<TSource> source, IQueryable<TSource> target)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (target == null) throw new ArgumentNullException("target");
            return source.Provider.Execute<IQueryable<TSource>>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(new [] { typeof(TSource) }), new [] { source.Expression, GetSourceExpression(target) }));
        }

        public static IQueryable<TSource> Distinct<TSource, TKey>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> selector)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (selector == null) throw new ArgumentNullException("selector");
            return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(new[] { typeof(TSource), typeof(TKey) }), new[] { source.Expression, Expression.Quote(selector) }));
        }

        public static IQueryable<TSource> Intersect<TSource>(this IQueryable<TSource> source, IQueryable<TSource> compare, params Expression<Func<TSource, object>>[] selectors)
        {
            return SetOperation(MethodBase.GetCurrentMethod(), source, compare, selectors);
        }

        public static IQueryable<TSource> Except<TSource>(this IQueryable<TSource> source, IQueryable<TSource> compare, params Expression<Func<TSource, object>>[] selectors)
        {
            return SetOperation(MethodBase.GetCurrentMethod(), source, compare, selectors);
        }

        private static IQueryable<TSource> SetOperation<TSource>(MethodBase methodBase, IQueryable<TSource> source, IQueryable<TSource> compare, params Expression<Func<TSource, object>>[] selectors)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (compare == null) throw new ArgumentNullException("compare");
            if (selectors == null) throw new ArgumentNullException("selectors");
            if (selectors.Length == 0) throw new ArgumentException("No selectors specified.", "selectors");
            return source.Provider.CreateQuery<TSource>(Expression.Call(null, ((MethodInfo)methodBase).MakeGenericMethod(new[] { typeof(TSource) }),
                                                                        new[] { source.Expression, 
                                                                                GetSourceExpression(compare), 
                                                                                Expression.NewArrayInit(typeof(Expression<Func<TSource, object>>), selectors) }));
        }

        private static Expression GetSourceExpression<TSource>(IEnumerable<TSource> source)
        {
            var queryable = source as IQueryable<TSource>;
            return queryable != null ? queryable.Expression : Expression.Constant(source, typeof(IEnumerable<TSource>));
        }

        // -------------------- Enumerable -----------------------

        public static IEnumerable<TSource> Randomize<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null) throw new ArgumentNullException("source");
            return source.OrderBy(x => Guid.NewGuid());
        }

        public static IEnumerable<TSource> TakePercent<TSource>(this IEnumerable<TSource> source, int percent)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (percent < 0 || percent > 100) throw new ArgumentOutOfRangeException("percent", "Percent must be between 0 and 100.");
            return source.Take(Convert.ToInt32((percent / 100.0) * source.Count()));
        }

        public static IQueryable<TSource> CopyTo<TSource>(this IEnumerable<TSource> source, string target)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (target == null) throw new ArgumentNullException("target");
            return new List<TSource>(source).AsQueryable();
        }

        public static IQueryable<TSource> CopyTo<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> target)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (target == null) throw new ArgumentNullException("target");
            if (target is ICollection<TSource>)
            {
                var collection = (ICollection<TSource>)target;
                foreach (var item in source) collection.Add(item);
                return collection.AsQueryable();
            }
            return target.Concat(source).AsQueryable();
        }

        public static IEnumerable<TSource> Distinct<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (selector == null) throw new ArgumentNullException("selector");
            return source.Distinct(LambdaComparer<TSource, TKey>.Create(selector));
        }

        public static IEnumerable<TSource> Intersect<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> compare, params Func<TSource, object>[] selectors)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (compare == null) throw new ArgumentNullException("compare");
            if (selectors == null) throw new ArgumentNullException("selectors");
            if (selectors.Length == 0) return Enumerable.Intersect(source, compare);
            return selectors.Aggregate(source, (current, selector) => current.Where(x => compare.Any(y => selector(x).Equals(selector(y)))));
        }

        public static IEnumerable<TSource> Except<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> compare, params Func<TSource, object>[] selectors)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (compare == null) throw new ArgumentNullException("compare");
            if (selectors == null) throw new ArgumentNullException("selectors");
            if (selectors.Length == 0) Enumerable.Except(source, compare);
            return Enumerable.Except(source, source.Intersect(compare, selectors));
        }

        public class LambdaComparer<T, TValue> : IEqualityComparer<T>
        {
            private readonly IList<Func<T, TValue>> _values;

            public LambdaComparer(IList<Func<T, TValue>> values)
            {
                _values = values;
            }

            public static LambdaComparer<T, TValue> Create(params Func<T, TValue>[] values)
            {
                return new LambdaComparer<T, TValue>(new List<Func<T, TValue>>(values));
            }

            public int GetHashCode(T target)
            {
                // Bernstein hash
                return _values.Aggregate(17, (current, value) => unchecked(current * 31 + value(target).GetHashCode()));
            }

            public bool Equals(T target1, T target2)
            {
                return _values.All(x => x(target1).Equals(x(target2)));
            }
        }
    }
}
