namespace DevTeam.IoC
{
    using System;
    using Contracts;

    internal class InstanceFactory: IInstanceFactory
    {
        private readonly InstanceFactoryMethod _instanceFactoryMethod;

        public InstanceFactory(InstanceFactoryMethod instanceFactoryMethod)
        {
            if (instanceFactoryMethod == null) throw new ArgumentNullException(nameof(instanceFactoryMethod));
            _instanceFactoryMethod = instanceFactoryMethod;
        }

        public object Create(params object[] args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));
            var instance = _instanceFactoryMethod(args);
            if (instance == null) throw new InvalidOperationException($"{nameof(instance)} can not be null");
            return instance;
        }
    }
}
