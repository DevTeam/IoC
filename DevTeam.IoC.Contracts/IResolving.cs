﻿namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IResolving<T> : IToken<IResolving<T>>
          where T : IResolver
    {
        [NotNull]
        object Instance([NotNull][ItemCanBeNull] params object[] state);

        [NotNull]
        object Instance([NotNull] IStateProvider stateProvider);

        [NotNull]
        TContract Instance<TContract>([NotNull][ItemCanBeNull] params object[] state);

        [NotNull]
        TContract Instance<TContract>([NotNull] IStateProvider stateProvider);

        bool TryInstance(out object instance, [NotNull][ItemCanBeNull] params object[] state);

        bool TryInstance<TContract>(out TContract instance, [NotNull][ItemCanBeNull] params object[] state);

        bool TryInstance(out object instance, [NotNull] IStateProvider stateProvider);

        bool TryInstance<TContract>(out TContract instance, [NotNull] IStateProvider stateProvider);
    }
}