﻿namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Contracts;

    [SuppressMessage("ReSharper", "IdentifierTypo")]
    internal sealed class ObservablesFeature : IConfiguration
    {
        public static readonly IConfiguration Shared = new ObservablesFeature();

        private ObservablesFeature()
        {
        }

        public IEnumerable<IConfiguration> GetDependencies(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            yield return EnumerablesFeature.Shared;
        }

        public IEnumerable<IDisposable> Apply(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            var reflection = container.Resolve().Instance<IReflection>();
            yield return
                container
                    .Register()
                    .Contract(typeof(IObservable<>))
                    .FactoryMethod(ctx => ResolveObservable(ctx, reflection));
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private object ResolveObservable(CreationContext creationContext, IReflection reflection)
        {
            var ctx = creationContext.ResolverContext;
            var genericContractKey = ctx.Key as IContractKey ?? (ctx.Key as ICompositeKey)?.ContractKeys.SingleOrDefault();
            if (genericContractKey == null)
            {
                throw new ContainerException($"Can not define contract type.\nDetails:\n{creationContext}");
            }

            var itemType = genericContractKey.GenericTypeArguments.First();
            var enumType = typeof(IEnumerable<>).MakeGenericType(itemType);
            var container = ctx.Container;
            var enumereble = container.Resolve().Contract(enumType).Instance();
            var observableType = typeof(Observable<>).MakeGenericType(itemType);

            var factory = container.Resolve().Instance<IMethodFactory>();
            var ctor = reflection.GetType(observableType).Constructors.Single(i => i.GetParameters().Length == 1);
            return factory.CreateConstructor(ctor)(enumereble);
        }

        public override int GetHashCode()
        {
            return GetType().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj != null && GetType() == obj.GetType();
        }

        private class Observable<T> : IObservable<T>, IDisposable
        {
            private readonly IEnumerable<T> _enumerable;

            public Observable([NotNull] IEnumerable<T> enumerable)
            {
                _enumerable = enumerable ?? throw new ArgumentNullException(nameof(enumerable));
            }

            public IDisposable Subscribe(IObserver<T> observer)
            {
                if (observer == null) throw new ArgumentNullException(nameof(observer));
                foreach (var item in _enumerable)
                {
                    observer.OnNext(item);
                }

                observer.OnCompleted();
                return this;
            }

            public void Dispose()
            {
            }
        }
    }
}
