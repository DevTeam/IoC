namespace DevTeam.IoC.Tests.Models
{
    using Contracts;

    internal class IdGenerator
    {
        private static long _id;

        public static long GenerateId(ICreationContext ctx)
        {
            return System.Threading.Interlocked.Increment(ref _id);
        }
    }
}
