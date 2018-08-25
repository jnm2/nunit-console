using System;
using System.Collections.Generic;
using NUnit.Common;

namespace NUnit.Engine
{
    internal static class Extensions
    {
        public static bool Any<TSource>(this IEnumerable<TSource> source)
        {
            Guard.ArgumentNotNull(source, nameof(source));

            using (var en = source.GetEnumerator())
            {
                return en.MoveNext();
            }
        }

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            Guard.ArgumentNotNull(source, nameof(source));

            using (var en = source.GetEnumerator())
            {
                if (en.MoveNext()) return en.Current;
            }

            return default(TSource);
        }

        public static bool TryFirst<TSource>(this IEnumerable<TSource> source, out TSource value)
        {
            Guard.ArgumentNotNull(source, nameof(source));

            using (var en = source.GetEnumerator())
            {
                if (en.MoveNext())
                {
                    value = en.Current;
                    return true;
                }
            }

            value = default(TSource);
            return false;
        }

        public static bool TryFirst<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, out TSource value)
        {
            Guard.ArgumentNotNull(source, nameof(source));
            Guard.ArgumentNotNull(predicate, nameof(predicate));

            foreach (var item in source)
            {
                if (!predicate.Invoke(item)) continue;
                value = item;
                return true;
            }

            value = default(TSource);
            return false;
        }

        public static TOutput[] ConvertAll<TInput, TOutput>(this IList<TInput> list, Converter<TInput, TOutput> converter)
        {
            Guard.ArgumentNotNull(list, nameof(list));
            Guard.ArgumentNotNull(converter, nameof(converter));

            var array = new TOutput[list.Count];

            for (var i = 0; i < array.Length; i++)
            {
                array[i] = converter.Invoke(list[i]);
            }

            return array;
        }

        public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            Guard.ArgumentNotNull(source, nameof(source));
            Guard.ArgumentNotNull(selector, nameof(selector));

            foreach (var value in source)
                yield return selector.Invoke(value);
        }
    }
}
