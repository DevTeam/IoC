namespace DevTeam.IoC.Tests
{
    using System;

    using Contracts;

    internal static class TestExtensions
    {
        public static IKeyFactory GetKeyFactory(this IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            return container.Resolve().Instance<IKeyFactory>();
        }

        public static IResolverContext CreateContext(this IResolver resolver, IKey key)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            if (key == null) throw new ArgumentNullException(nameof(key));

            IResolverContext context;
            if (resolver.TryCreateResolverContext(key, out context))
            {
                return context;
            }

            throw new InvalidOperationException();
        }
    }
}
