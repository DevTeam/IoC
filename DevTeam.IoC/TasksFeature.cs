namespace DevTeam.IoC
{
#if !NET35
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

        public IEnumerable<IConfiguration> GetDependencies(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            yield break;
        }

        public IEnumerable<IDisposable> Apply(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            var reflection = container.Resolve().Instance<IReflection>();
            yield return
                container
                .Register()
                .Contract(typeof(Task<>))
                .KeyComparer(Wellknown.KeyComparer.AnyTagAnyState)
                .FactoryMethod(ctx => ResolveTask(ctx, reflection))
                .Apply();
        }

        public override int GetHashCode()
        {
            return GetType().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj != null && GetType() == obj.GetType();
        }

        private static object ResolveTask(ICreationContext creationContext, IReflection reflection)
        {
            var ctx = creationContext.ResolverContext;
            var genericContractKey = ctx.Key as IContractKey ?? (ctx.Key as ICompositeKey)?.ContractKeys.SingleOrDefault();
            if (genericContractKey == null)
            {
                throw new InvalidOperationException();
            }

            var taskValueType = genericContractKey.GenericTypeArguments.First();
            var taskType = typeof(ResolverTask<>).MakeGenericType(taskValueType);
            var factory = ctx.Container.Resolve().Instance<IInstanceFactoryProvider>(creationContext.StateProvider);
            var ctor = reflection.GetTypeInfo(taskType).DeclaredConstructors.Single();
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
                var compositeKey = ctx.Key as ICompositeKey;
                var resolver = ctx.Container.Resolve();
                if (compositeKey != null)
                {
                    resolver.Key(compositeKey.TagKeys.Cast<IKey>()).Key(compositeKey.StateKeys.Cast<IKey>());
                }

                return () => resolver.Instance<T>();
            }
        }
    }
#endif
}
