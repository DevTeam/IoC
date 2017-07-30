namespace DevTeam.IoC
{
#if !NET35
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Contracts;

    internal sealed class TasksFeature: IConfiguration
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
                .FactoryMethod(ctx => ResolveTask(ctx, reflection));
        }

        public override int GetHashCode()
        {
            return GetType().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj != null && GetType() == obj.GetType();
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
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
            var factory = ctx.Container.Resolve().Instance<IMethodFactory>(creationContext.StateProvider);
            var ctor = reflection.GetType(taskType).Constructors.Single();
            return factory.CreateConstructor(ctor)(ctx);
        }

        private sealed class ResolverTask<T> : Task<T>
        {
            public ResolverTask(IResolverContext ctx) 
                : base(CreateFunction(ctx))
            {
            }

#if !NET35 && !NET40
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            private static Func<T> CreateFunction(IResolverContext ctx)
            {
                var compositeKey = ctx.Key as ICompositeKey;
                var resolver = ctx.Container.Resolve();
                if (compositeKey != null)
                {
                    resolver.Key(compositeKey.TagKeys).Key(compositeKey.StateKeys);
                }

                return () => resolver.Instance<T>();
            }
        }
    }
#endif
}
