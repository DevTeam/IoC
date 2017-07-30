namespace DevTeam.IoC
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Contracts;

    [SuppressMessage("ReSharper", "IdentifierTypo")]
    internal sealed class EnumerablesFeature: IConfiguration
    {
        public static readonly IConfiguration Shared = new EnumerablesFeature();

        private EnumerablesFeature()
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
                .Contract(typeof(IEnumerable<>))
                .FactoryMethod(ctx => ResolveEnumerable(ctx, reflection));
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
        private static object ResolveEnumerable(ICreationContext creationContext, IReflection reflection)
        {
            var ctx = creationContext.ResolverContext;
            var genericContractKey = ctx.Key as IContractKey ?? (ctx.Key as ICompositeKey)?.ContractKeys.SingleOrDefault();
            if (genericContractKey == null)
            {
                throw new InvalidOperationException();
            }

            var enumItemType = genericContractKey.GenericTypeArguments.First();
            var enumType = typeof(Enumerable<>).MakeGenericType(enumItemType);
            var contractKeys = new List<IContractKey> { new ContractKey(reflection, enumItemType, true) };
            var container = ctx.Container;

            var keys = (
                from contractKey in contractKeys
                from key in FilterByContract(GetAllRegistrations(container), contractKey)
                select key).Distinct();

            var source =
                from key in keys
                select container.Resolve().Key(new [] { key }).Instance();

            var factory = container.Resolve().Instance<IMethodFactory>(creationContext.StateProvider);
            var ctor = reflection.GetType(enumType).Constructors.Single(i => i.GetParameters().Length == 1);
            return factory.CreateConstructor(ctor)(source);
        }

        private static IEnumerable<IKey> GetAllRegistrations(IContainer container)
        {
            foreach (var registration in container.Registrations)
            {
                yield return registration;
            }

            if (container.Parent == null)
            {
                yield break;
            }

            foreach (var registration in GetAllRegistrations(container.Parent))
            {
                yield return registration;
            }
        }

        private static IEnumerable<IKey> FilterByContract(IEnumerable<IKey> keys, IContractKey contractKey)
        {
            foreach (var key in keys)
            {
                switch (key)
                {
                    case IContractKey curContractKey:
                        if (curContractKey.ContractType == contractKey.ContractType)
                        {
                            yield return contractKey;
                        }
                        break;

                    case ICompositeKey compositeKey:
                        if (compositeKey.ContractKeys.Contains(contractKey))
                        {
                            yield return RootContainerConfiguration.KeyFactory.CreateCompositeKey(compositeKey.ContractKeys.Where(i => i.ContractType != contractKey.ContractType).Concat(Enumerable.Repeat(contractKey, 1)), compositeKey.TagKeys, compositeKey.StateKeys);
                        }
                        break;
                }
            }
        }

        private class Enumerable<T> : IEnumerable<T>
        {
            private readonly IEnumerable<object> _source;

            public Enumerable(IEnumerable<object> source)
            {
                _source = source ?? throw new ArgumentNullException(nameof(source));
            }

            public IEnumerator<T> GetEnumerator()
            {
                return new Enumerator<T>(_source.GetEnumerator());
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private class Enumerator<T> : IEnumerator<T>
        {
            private readonly IEnumerator<object> _source;

            public Enumerator(IEnumerator<object> source)
            {
                _source = source ?? throw new ArgumentNullException(nameof(source));
            }

            public T Current => (T)_source.Current;

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                return _source.MoveNext();
            }

            public void Reset()
            {
                _source.Reset();
            }

            public void Dispose()
            {
                _source.Dispose();
            }
        }
    }
}
