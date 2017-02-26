﻿namespace DevTeam.IoC
{
    using System;
    using System.Linq;
    using Contracts;

    internal class ParamsStateProvider: IStateProvider
    {
        private readonly object[] _state;
        private int? _hashCode;

        public ParamsStateProvider(params object[] state)
        {
            _state = state;
        }

        public object GetState(IResolverContext resolverContext, IStateKey stateKey)
        {
            return _state[stateKey.Index];
        }

        public object GetKey(IResolverContext resolverContext)
        {
            return this;
        }

        public bool Equals(ParamsStateProvider other)
        {
            return _state.SequenceEqual(other._state);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            var stateProvider = obj as ParamsStateProvider;
            return stateProvider != null && Equals(stateProvider);
        }

        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyMemberInGetHashCode
            _hashCode = _hashCode ?? GetHashInternal();
            return _hashCode.Value;
            // ReSharper restore NonReadonlyMemberInGetHashCode
        }

        private int GetHashInternal()
        {
            return _state.Aggregate(0, (code, key) =>
            {
                unchecked
                {
                    return (code*397) ^ key.GetHashCode();
                }
            });
        }
    }
}
