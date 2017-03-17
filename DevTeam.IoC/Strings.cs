namespace DevTeam.IoC
{
    using Contracts;

    internal static class Strings
    {
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
