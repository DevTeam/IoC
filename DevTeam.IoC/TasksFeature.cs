namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Contracts;

    internal class TasksFeature: IConfiguration
    {
        public static readonly IConfiguration Shared = new TasksFeature();

        private TasksFeature()
        {
        }

        public IEnumerable<IConfiguration> GetDependencies<T>(T container) where T : IContainer
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            yield break;
        }

        public IEnumerable<IDisposable> Apply<T>(T container) where T : IContainer
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            yield return
                container
                .Register()
                .Contract(typeof(Task<>))
                .KeyComparer(Wellknown.KeyComparer.AnyTagAnyState)
                .FactoryMethod(ResolveTask);
        }

        public override int GetHashCode()
        {
            return GetType().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj != null && GetType() == obj.GetType();
        }

        private static object ResolveTask(IResolverContext ctx)
        {
            var genericContractKey = ctx.Key?.ContractKeys.SingleOrDefault();
            if (genericContractKey == null)
            {
                throw new InvalidOperationException();
            }

            var taskValueType = genericContractKey.GenericTypeArguments.First();
            var taskType = typeof(ResolverTask<>).MakeGenericType(taskValueType);
            var factory = ctx.Container.Resolve().Instance<IInstanceFactoryProvider>(ctx.StateProvider);
            var ctor = taskType.GetTypeInfo().DeclaredConstructors.Single();
            return factory.GetFactory(ctor).Create(ctx);
        }

        private class ResolverTask<T> : Task<T>
        {
            public ResolverTask(IResolverContext ctx) 
                : base(CreateFunction(ctx))
            {
            }

            private static Func<T> CreateFunction(IResolverContext ctx)
            {
                return () => ctx.Container
                    .Resolve()
                    .Key(ctx.Key.TagKeys)
                    .Key(ctx.Key.StateKeys)
                    .Instance<T>();
            }
        }
    }
}
