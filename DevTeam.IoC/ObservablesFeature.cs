namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using Contracts;

    [SuppressMessage("ReSharper", "IdentifierTypo")]
    internal class ObservablesFeature : IConfiguration
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
            yield return
                container
                    .Register()
                    .Contract(typeof(IObservable<>))
                    .FactoryMethod(ResolveObservable)
                    .Apply();
        }

        private object ResolveObservable(ICreationContext creationContext)
        {
            var ctx = creationContext.ResolverContext;
            var genericContractKey = ctx.Key as IContractKey ?? (ctx.Key as ICompositeKey)?.ContractKeys.SingleOrDefault();
            if (genericContractKey == null)
            {
                throw new InvalidOperationException();
            }

            var itemType = genericContractKey.GenericTypeArguments.First();
            var enumType = typeof(IEnumerable<>).MakeGenericType(itemType);
            var enumereble = ctx.Container.Resolve().Contract(enumType).Instance();
            var observableType = typeof(Observable<>).MakeGenericType(itemType);

            var factory = ctx.Container.Resolve().Instance<IInstanceFactoryProvider>(ctx);
            var ctor = observableType.GetTypeInfo().DeclaredConstructors.Single(i => i.GetParameters().Length == 1);
            return factory.GetFactory(ctor).Create(enumereble);
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
                if (enumerable == null) throw new ArgumentNullException(nameof(enumerable));
                _enumerable = enumerable;
            }

            public IDisposable Subscribe(IObserver<T> observer)
            {
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
