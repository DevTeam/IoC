#if NET35
namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using Contracts;

    internal static class Enumarables
    {
        [NotNull]
        public static IEnumerable<T> Zip<T, T1, T2>([NotNull] this IEnumerable<T1> src1, [NotNull] IEnumerable<T2> src2, [NotNull] Func<T1, T2, T> selector)
        {
            if (src1 == null) throw new ArgumentNullException(nameof(src1));
            if (src2 == null) throw new ArgumentNullException(nameof(src2));
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            using (var enum1 = src1.GetEnumerator())
            using (var enum2 = src2.GetEnumerator())
            {
                while (enum1.MoveNext() && enum2.MoveNext())
                {
                    yield return selector(enum1.Current, enum2.Current);
                }
            }
        }
    }
}
#endif
