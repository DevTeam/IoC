namespace DevTeam.IoC
{
    using System;

    using Contracts;

    internal class MethodFactory<TContract> : IResolverFactory
    {
        private readonly Func<ICreationContext, TContract> _factoryMethod;

        public MethodFactory(Func<ICreationContext, TContract> factoryMethod)
        {
            if (factoryMethod == null) throw new ArgumentNullException(nameof(factoryMethod));
            _factoryMethod = factoryMethod;
        }

        public object Create(ICreationContext creationContext)
        {
            if (creationContext == null) throw new ArgumentNullException(nameof(creationContext));
            return _factoryMethod(creationContext);
        }
    }
}