﻿namespace DevTeam.IoC
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    using Contracts;

    [SuppressMessage("ReSharper", "IdentifierTypo")]
    internal class EnumerablesFeature: IConfiguration
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
            yield return
                container
                .Register()
                .Contract(typeof(IEnumerable<>))
                .FactoryMethod(ResolveEnumerable)
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

        private static object ResolveEnumerable(ICreationContext creationContext)
        {
            var ctx = creationContext.ResolverContext;
            var genericContractKey = ctx.Key as IContractKey ?? (ctx.Key as ICompositeKey)?.ContractKeys.SingleOrDefault();
            if (genericContractKey == null)
            {
                throw new InvalidOperationException();
            }

            var enumItemType = genericContractKey.GenericTypeArguments.First();
            var enumType = typeof(Enumerable<>).MakeGenericType(enumItemType);
            var contractKeys = new List<IContractKey> { new ContractKey(enumItemType, true) };
            var container = ctx.Container;

            var keys = (
                from contractKey in contractKeys
                from key in FilterByContract(GetAllRegistrations(container), contractKey)
                select key).Distinct();

            var source =
                from key in keys
                select container.Resolve().Key(key).Instance();

            var factory = container.Resolve().Instance<IInstanceFactoryProvider>(creationContext.StateProvider);
            var ctor = enumType.GetTypeInfo().DeclaredConstructors.Single(i => i.GetParameters().Length == 1);
            return factory.GetFactory(ctor).Create(source);
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
                var curContractKey = key as IContractKey;
                if (curContractKey != null && curContractKey.ContractType == contractKey.ContractType)
                {
                    yield return contractKey;
                    continue;
                }

                var compositeKey = key as ICompositeKey;
                if (compositeKey != null)
                {
                    if (compositeKey.ContractKeys.Contains(contractKey))
                    {
                        yield return RootContainerConfiguration.KeyFactory.CreateCompositeKey(compositeKey.ContractKeys.Where(i => i.ContractType != contractKey.ContractType).Concat(Enumerable.Repeat(contractKey, 1)), compositeKey.TagKeys, compositeKey.StateKeys);
                    }
                }
            }
        }

        private class Enumerable<T> : IEnumerable<T>
        {
            private readonly IEnumerable<object> _source;

            public Enumerable(IEnumerable<object> source)
            {
                if (source == null) throw new ArgumentNullException(nameof(source));

                _source = source;
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
                if (source == null) throw new ArgumentNullException(nameof(source));

                _source = source;
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
