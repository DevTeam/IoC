namespace DevTeam.IoC
{
    using System;
    using Contracts;

    internal class InstanceFactory: IInstanceFactory
    {
        private readonly InstanceFactoryMethod _instanceFactoryMethod;

        public InstanceFactory(InstanceFactoryMethod instanceFactoryMethod)
        {
#if DEBUG
            if (instanceFactoryMethod == null) throw new ArgumentNullException(nameof(instanceFactoryMethod));
#endif
            _instanceFactoryMethod = instanceFactoryMethod;
        }

        public object Create(params object[] args)
        {
#if DEBUG
            if (args == null) throw new ArgumentNullException(nameof(args));
#endif
            var instance = _instanceFactoryMethod(args);
#if DEBUG
            if (instance == null) throw new InvalidOperationException($"{nameof(instance)} can not be null");
#endif
            return instance;
        }
    }
}
