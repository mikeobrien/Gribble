using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Gribble.Model;

namespace Gribble.Extensions
{
    public enum SyncFields { Include, Exclude }

    public static class Queryable
    {
        // -------------------- Queryable -----------------------

        public static IQueryable<TSource> Empty<TSource>()
        {
            return (IQueryable<TSource>)Enumerable.Empty<TSource>();
        }

        public static IQueryable<TSource> Randomize<TSource>(this IQueryable<TSource> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return source.Provider.CreateQuery<TSource>(Expression.Call(null, 
                ((MethodInfo) MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), source.Expression));
        }

        public static IQueryable<TSource> TakePercent<TSource>(this IQueryable<TSource> source, int percent)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (percent < 0 || percent > 100) throw new ArgumentOutOfRangeException(nameof(percent), 
                "Percent must be between 0 and 100.");
            return source.Provider.CreateQuery<TSource>(Expression.Call(null, 
                ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), 
                new [] { source.Expression, Expression.Constant(percent) }));
        }

        public static IQueryable<TSource> CopyTo<TSource>(this IQueryable<TSource> source, IQueryable<TSource> target)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (target == null) throw new ArgumentNullException(nameof(target));
            return source.Provider.Execute<IQueryable<TSource>>(Expression.Call(null, 
                ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource)), 
                new [] { source.Expression, target.Expression }));
        }

        public static IQueryable<TTarget> SyncWith<TTarget, TKey>(this IQueryable<TTarget> target, 
            IQueryable<TTarget> source, Expression<Func<TTarget, TKey>> keySelector,
            SyncFields syncFields, params Expression<Func<TTarget, object>>[] syncSelectors)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            return target.Provider.Execute<IQueryable<TTarget>>(Expression.Call(null,
                ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TTarget), typeof(TKey)), 
                target.Expression, source.Expression, Expression.Quote(keySelector), Expression.Constant(syncFields), 
                Expression.NewArrayInit(typeof(Expression<Func<TTarget, object>>), syncSelectors)));
        }

        public static IQueryable<TSource> Distinct<TSource, TKey>(this IQueryable<TSource> source, 
            Expression<Func<TSource, TKey>> selector)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            return source.Provider.CreateQuery<TSource>(Expression.Call(null, 
                ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), 
                typeof(TKey)), new[] { source.Expression, Expression.Quote(selector) }));
        }

        public static IQueryable<TSource> Distinct<TSource, TKey, TOrder>(this IQueryable<TSource> source, 
            Expression<Func<TSource, TKey>> selector, Expression<Func<TSource, TOrder>> orderSelector, Order order)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            if (selector == null) throw new ArgumentNullException(nameof(orderSelector));
            return source.Provider.CreateQuery<TSource>(Expression.Call(null,
                ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey), typeof(TOrder)), 
                source.Expression, Expression.Quote(selector), Expression.Quote(orderSelector), Expression.Constant(order)));
        }

        public static IQueryable<TSource> Duplicates<TSource, TKey>(this IQueryable<TSource> source, 
            Expression<Func<TSource, TKey>> selector)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            return source.Provider.CreateQuery<TSource>(Expression.Call(null, 
                ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey)), 
                new[] { source.Expression, Expression.Quote(selector) }));
        }

        public static IQueryable<TSource> Duplicates<TSource, TKey, TOrder>(this IQueryable<TSource> source, 
            Expression<Func<TSource, TKey>> selector, Expression<Func<TSource, TOrder>> orderSelector, Order order)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            if (selector == null) throw new ArgumentNullException(nameof(orderSelector));
            return source.Provider.CreateQuery<TSource>(Expression.Call(null, 
                ((MethodInfo)MethodBase.GetCurrentMethod()).MakeGenericMethod(typeof(TSource), typeof(TKey), typeof(TOrder)), 
                source.Expression, Expression.Quote(selector), Expression.Quote(orderSelector), Expression.Constant(order)));
        }

        public static IQueryable<TSource> Duplicates<TSource, TKey, TOrder1, TOrder2>(
            this IQueryable<TSource> source, Expression<Func<TSource, TKey>> selector, 
            Expression<Func<TSource, TOrder1>> orderSelector1, Order order1, 
            Expression<Func<TSource, TOrder2>> orderSelector2, Order order2)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            if (selector == null) throw new ArgumentNullException(nameof(orderSelector1));
            if (selector == null) throw new ArgumentNullException(nameof(orderSelector2));
            return source.Provider.CreateQuery<TSource>(Expression.Call(null, 
                ((MethodInfo)MethodBase.GetCurrentMethod())
                .MakeGenericMethod(typeof(TSource), typeof(TKey), typeof(TOrder1), typeof(TOrder2)), 
                source.Expression, Expression.Quote(selector), Expression.Quote(orderSelector1), 
                Expression.Constant(order1), Expression.Quote(orderSelector2), Expression.Constant(order2)));
        }

        public static IQueryable<TSource> Intersect<TSource>(this IQueryable<TSource> source, 
            IQueryable<TSource> compare, params Expression<Func<TSource, object>>[] selectors)
        {
            return SetOperation(MethodBase.GetCurrentMethod(), source, compare, selectors);
        }

        public static IQueryable<TSource> Except<TSource>(this IQueryable<TSource> source, 
            IQueryable<TSource> compare, params Expression<Func<TSource, object>>[] selectors)
        {
            return SetOperation(MethodBase.GetCurrentMethod(), source, compare, selectors);
        }

        private static IQueryable<TSource> SetOperation<TSource>(MethodBase methodBase, 
            IQueryable<TSource> source, IQueryable<TSource> compare, 
            params Expression<Func<TSource, object>>[] selectors)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (compare == null) throw new ArgumentNullException(nameof(compare));
            if (selectors == null) throw new ArgumentNullException(nameof(selectors));
            if (selectors.Length == 0) throw new ArgumentException("No selectors specified.", nameof(selectors));
            return source.Provider.CreateQuery<TSource>(
                Expression.Call(null, ((MethodInfo)methodBase).MakeGenericMethod(typeof(TSource)),
                    new[] { source.Expression, compare.Expression, 
                            Expression.NewArrayInit(typeof(Expression<Func<TSource, object>>), selectors) }));
        }

        // -------------------- Enumerable -----------------------

        public static IEnumerable<TSource> Randomize<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return source.OrderBy(x => Guid.NewGuid());
        }

        public static IEnumerable<TSource> TakePercent<TSource>(this IEnumerable<TSource> source, int percent)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (percent < 0 || percent > 100) throw new ArgumentOutOfRangeException(nameof(percent), "Percent must be between 0 and 100.");
            return source.Take(Convert.ToInt32((percent / 100.0) * source.Count()));
        }

        public static IQueryable<TSource> CopyTo<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> target)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (target == null) throw new ArgumentNullException(nameof(target));
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
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            return source.GroupBy(selector).SelectMany(x => x.Take(1));
        }

        public static IEnumerable<TSource> Distinct<TSource, TKey, TOrder>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, Func<TSource, TOrder> orderSelector, Order order)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            if (selector == null) throw new ArgumentNullException(nameof(orderSelector));
            return source.GroupBy(selector).SelectMany(x => (order == Order.Ascending ? 
                x.OrderBy(orderSelector) : x.OrderByDescending(orderSelector)).Take(1));
        }

        public static IEnumerable<TSource> Duplicates<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            return source.GroupBy(selector).Where(x => x.Count() > 1).SelectMany(x => x.Skip(1));
        }

        public static IEnumerable<TSource> Duplicates<TSource, TKey, TOrder>(
            this IEnumerable<TSource> source, Func<TSource, TKey> selector, 
            Func<TSource, TOrder> orderSelector, Order order)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            if (selector == null) throw new ArgumentNullException(nameof(orderSelector));
            return source.GroupBy(selector).Where(x => x.Count() > 1)
                .SelectMany(x => (order == Order.Ascending ? x.OrderBy(orderSelector) : 
                x.OrderByDescending(orderSelector)).Skip(1));
        }

        public static IEnumerable<TSource> Duplicates<TSource, TKey, TOrder1, TOrder2>(
            this IEnumerable<TSource> source, Func<TSource, TKey> selector, 
            Func<TSource, TOrder1> orderSelector1, Order order1, 
            Func<TSource, TOrder2> orderSelector2, Order order2)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            if (selector == null) throw new ArgumentNullException(nameof(orderSelector1));
            if (selector == null) throw new ArgumentNullException(nameof(orderSelector2));
            return source.GroupBy(selector).Where(x => x.Count() > 1).SelectMany(x => 
                x.When(order1 == Order.Ascending, y => y.OrderBy(orderSelector1), y => y.OrderByDescending(orderSelector1))
                .When(order2 == Order.Ascending, y => ((IOrderedEnumerable<TSource>)y).ThenBy(orderSelector2), 
                                                y => ((IOrderedEnumerable<TSource>)y).ThenByDescending(orderSelector2)).Skip(1));
        }

        public static IQueryable<TSource> SyncWith<TSource, TKey>(this IEnumerable<TSource> target, 
            IEnumerable<TSource> source, Func<TSource, TKey> keySelector,
            SyncFields fieldSelectionType, params Expression<Func<TSource, object>>[] syncSelectors)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            var results = target.Join(source, keySelector, keySelector, (t, s) => new { Target = t, Source = s }).ToList();
            var syncProperties = syncSelectors.Select(x => x.GetPropertyInfo()).ToList();
            var properties = fieldSelectionType == SyncFields.Include ? syncProperties : 
                typeof (TSource).GetCachedProperties().Where(x => !syncProperties.Contains(x)).ToList();
            results.ForEach(x => properties.ForEach(y => y.SetValue(x.Target, y.GetValue(x.Source, null), null)));
            return target.AsQueryable();
        }

        internal static IEnumerable<TTarget> When<TSource, TTarget>(this IEnumerable<TSource> source, bool predicate,
            Func<IEnumerable<TSource>, IEnumerable<TTarget>> trueCondition,
            Func<IEnumerable<TSource>, IEnumerable<TTarget>> falseCondition)
        {
            return predicate ? trueCondition(source) : falseCondition(source);
        }

        public static IEnumerable<TSource> Intersect<TSource>(this IEnumerable<TSource> source, 
            IEnumerable<TSource> compare, params Func<TSource, object>[] selectors)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (compare == null) throw new ArgumentNullException(nameof(compare));
            if (selectors == null) throw new ArgumentNullException(nameof(selectors));
            if (selectors.Length == 0) return Enumerable.Intersect(source, compare);
            return selectors.Aggregate(source, (current, selector) => current
                .Where(x => compare.Any(y => selector(x).Equals(selector(y)))));
        }

        public static IEnumerable<TSource> Except<TSource>(this IEnumerable<TSource> source, 
            IEnumerable<TSource> compare, params Func<TSource, object>[] selectors)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (compare == null) throw new ArgumentNullException(nameof(compare));
            if (selectors == null) throw new ArgumentNullException(nameof(selectors));
            if (selectors.Length == 0) return Enumerable.Except(source, compare);
            return Enumerable.Except(source, source.Intersect(compare, selectors));
        }
    }
}
