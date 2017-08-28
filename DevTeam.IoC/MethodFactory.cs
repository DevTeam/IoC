namespace DevTeam.IoC
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Contracts;

    internal sealed class MethodFactory<TContract> : IInstanceFactory
    {
        private readonly Func<CreationContext, TContract> _factoryMethod;

        [SuppressMessage("ReSharper", "JoinNullCheckWithUsage")]
        public MethodFactory(Func<CreationContext, TContract> factoryMethod)
        {
#if DEBUG
            if (factoryMethod == null) throw new ArgumentNullException(nameof(factoryMethod));
#endif
            _factoryMethod = factoryMethod;
        }

        public object Create(CreationContext creationContext)
        {
            return _factoryMethod(creationContext);
        }
    }
}