namespace DevTeam.IoC.Tests.Models
{
    using System;
    using Contracts;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class IdGenerator
    {
        private static long _id;

        public static long GenerateId([NotNull] ICreationContext ctx)
        {
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));
            return System.Threading.Interlocked.Increment(ref _id);
        }
    }
}
