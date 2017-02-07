﻿namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IResolverContext
    {
        IContainer Container { [NotNull] get; }

        IContainer ParentContainer { [CanBeNull] get; }

        ICompositeKey Key { [NotNull] get; }

        object RegistrationKey { [NotNull] get; }

        IResolverFactory InstanceFactory { [NotNull] get; }

        IStateProvider StateProvider { [NotNull] get; }

        IRegistryContext RegistryContext { [NotNull] get; }
    }
}
