namespace DevTeam.IoC
{
    using Contracts;

#if NET35
    using System.Linq;
#endif

    internal static class Arrays
    {
#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static bool SequenceEqual<T>([NotNull] T[] array1, [NotNull] T[] array2)
        {
#if NET35
            return array1.SequenceEqual(array2);
#else
            return ((System.Collections.IStructuralEquatable)array1).Equals(array2, System.Collections.StructuralComparisons.StructuralEqualityComparer);
#endif
        }
    }
}
