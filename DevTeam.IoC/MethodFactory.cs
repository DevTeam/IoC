namespace DevTeam.IoC
{
    using System;

    using Contracts;

    internal class MethodFactory<TContract> : IResolverFactory
    {
        private readonly Func<IResolverContext, TContract> _factoryMethod;

        public MethodFactory(Func<IResolverContext, TContract> factoryMethod)
        {
            if (factoryMethod == null) throw new ArgumentNullException(nameof(factoryMethod));
            _factoryMethod = factoryMethod;
        }

        public object Create(IResolverContext resolverContext)
        {
            return _factoryMethod(resolverContext);
        }
    }
}