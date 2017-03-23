namespace DevTeam.IoC.Tests
{
    using System;
    using System.Linq;
    using Contracts;

    internal static class TestsExtensions
    {
        [NotNull]
        public static IDisposable Register<T>([NotNull] this T registry, [NotNull] IRegistryContext context)
            where T : IRegistry
        {
            if (registry == null) throw new ArgumentNullException(nameof(registry));
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (!registry.TryRegister(context, out IDisposable registration))
            {
                throw new InvalidOperationException($"Can not register {string.Join(Environment.NewLine, context.Keys.Select(i => i.ToString()).ToArray())}.{Environment.NewLine}{Environment.NewLine}{registry}");
            }

            return registration;
        }
    }
}
