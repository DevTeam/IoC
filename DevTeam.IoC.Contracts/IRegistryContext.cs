namespace DevTeam.IoC.Contracts
{
    using System.Collections.Generic;

    public interface IRegistryContext
    {
        IContainer Container { get; }

        IContainer ParentContainer { get; }

        IEnumerable<ICompositeKey> Keys { get; }

        IResolverFactory InstanceFactory { get; }

        IEnumerable<IExtension> Extensions { get; }
    }
}
