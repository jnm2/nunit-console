using System.Collections.Generic;
using NUnit.Common;

namespace NUnit.Engine
{
    internal static class Extensions
    {
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
    }
}
