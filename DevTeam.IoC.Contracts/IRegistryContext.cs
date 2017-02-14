namespace DevTeam.IoC.Contracts
{
    using System.Collections.Generic;

    [PublicAPI]
    public interface IRegistryContext
    {
        IContainer Container { [NotNull] get; }

        IEnumerable<ICompositeKey> Keys { [NotNull] get; }

        IResolverFactory InstanceFactory { [NotNull] get; }

        IEnumerable<IExtension> Extensions { [NotNull] get; }
    }
}
