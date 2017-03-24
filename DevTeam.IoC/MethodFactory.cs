namespace DevTeam.IoC
{
    using System;

    using Contracts;

    internal sealed class MethodFactory<TContract> : IInstanceFactory
    {
        private readonly Func<ICreationContext, TContract> _factoryMethod;

        public MethodFactory(Func<ICreationContext, TContract> factoryMethod)
        {
#if DEBUG
            if (factoryMethod == null) throw new ArgumentNullException(nameof(factoryMethod));
#endif
            _factoryMethod = factoryMethod;
        }

        public object Create(ICreationContext creationContext)
        {
#if DEBUG
            if (creationContext == null) throw new ArgumentNullException(nameof(creationContext));
#endif
            return _factoryMethod(creationContext);
        }
    }
}