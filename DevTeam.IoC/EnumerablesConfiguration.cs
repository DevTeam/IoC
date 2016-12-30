namespace DevTeam.IoC
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Contracts;

    internal class EnumerablesConfiguration: IConfiguration
    {
        public static readonly IConfiguration Shared = new EnumerablesConfiguration();

        private EnumerablesConfiguration()
        {
        }

        public IEnumerable<IConfiguration> GetDependencies(IResolver resolver)
        {
            yield break;
        }

        public IEnumerable<IDisposable> Apply(IResolver resolver)
        {
            yield return
                resolver
                .Register()
                .Contract(typeof(IEnumerable<>))
                .AsFactoryMethod(ResolveEnumerable);
        }

        public override int GetHashCode()
        {
            return GetType().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj != null && GetType() == obj.GetType();
        }

        private static object ResolveEnumerable(IResolverContext ctx)
        {
            var genericContractKey = ctx.Key?.ContractKeys.SingleOrDefault();
            if (genericContractKey == null)
            {
                throw new InvalidOperationException();
            }

            var enumItemType = genericContractKey.GenericTypeArguments.First();
            var enumType = typeof(Enumerable<>).MakeGenericType(enumItemType);
            var contractKeys = new List<IContractKey> { new ContractKey(enumItemType, true) };
            if (enumItemType.IsConstructedGenericType)
            {
                contractKeys.Add(new ContractKey(enumItemType, true));
            }

            var keys = (
                from contractKey in contractKeys
                from key in FilterByContract(ctx.Container.Registrations, contractKey)
                select key).Distinct();

            var source =
                from key in keys
                select ctx.Container.Resolve().Key(key).Instance();

            var factory = ctx.Container.Resolve().Instance<IInstanceFactoryProvider>(ctx.StateProvider);
            var ctor = enumType.GetTypeInfo().DeclaredConstructors.Single(i => i.GetParameters().Length == 1);
            return factory.GetFactory(ctor).Create(source);
        }

        private static IEnumerable<IKey> FilterByContract(IEnumerable<ICompositeKey> keys, IContractKey contractKey)
        {
            foreach (var key in keys)
            {
                if (key.ContractKeys.Contains(contractKey))
                {
                    yield return new CompositeKey(key.ContractKeys.Where(i => i.ContractType != contractKey.ContractType).Concat(Enumerable.Repeat(contractKey, 1)).ToArray(), key.TagKeys, key.StateKeys);
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
