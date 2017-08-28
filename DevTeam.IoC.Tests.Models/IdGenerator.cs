namespace DevTeam.IoC.Tests.Models
{
    using Contracts;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal sealed class IdGenerator
    {
        private static long _id;

        public static long GenerateId(CreationContext ctx)
        {
            return System.Threading.Interlocked.Increment(ref _id);
        }
    }
}
