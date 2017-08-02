namespace DevTeam.IoC.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Contracts;

    internal static class TestsExtensions
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

        [NotNull]
        public static IDisposable Register<T>([NotNull] this T registry, [NotNull] IRegistryContext context)
            where T : IRegistry
        {
            if (registry == null) throw new ArgumentNullException(nameof(registry));
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (!registry.TryRegister(context, out IDisposable registration))
            {
                throw new ContainerException($"Can not register {string.Join(Environment.NewLine, context.Keys.Select(i => i.ToString()).ToArray())}.\nDetails:\n{registry}");
            }

            return registration;
        }

        public static Assembly GetAssembly(Type type)
        {
#if !NETCOREAPP1_0
            return type.Assembly;
#else
            return type.GetTypeInfo().Assembly;
#endif
        }

        public static string GetBinDirectory()
        {
#if !NETCOREAPP1_0
            return AppDomain.CurrentDomain.BaseDirectory;
#else
            return Path.GetDirectoryName(GetAssembly(typeof(TestsExtensions)).Location);
#endif
        }
    }
}
