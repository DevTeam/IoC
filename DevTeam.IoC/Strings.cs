namespace DevTeam.IoC
{
    using Contracts;

    internal static class Strings
    {
#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsNullOrWhiteSpace([CanBeNull] this string str)
        {
            if (str == null)
            {
                return true;
            }

            return str.Trim() == string.Empty;
        }
    }
}
