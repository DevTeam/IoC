namespace DevTeam.IoC.Contracts
{
    using System.Collections.Generic;

    [PublicAPI]
    public interface IRegistryContext
    {
        IContainer Container { [NotNull] get; }

        IEnumerable<IKey> Keys { [NotNull] get; }

        IInstanceFactory InstanceFactory { [NotNull] get; }

        IEnumerable<IExtension> Extensions { [NotNull] get; }
    }
}
