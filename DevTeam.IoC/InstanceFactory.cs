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
            return _instanceFactoryMethod(args);
        }
    }
}
