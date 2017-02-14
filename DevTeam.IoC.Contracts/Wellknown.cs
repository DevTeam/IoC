namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public static class Wellknown
    {
        public enum Feature
        {
            // Support default set of features.
            Default,

            // Support child resolving child containers.
            ChildContainers,

            // Support lifetimes.
            Lifetimes,

            // Support scopes.
            Scopes,

            // Support key comparers.
            // ReSharper disable once IdentifierTypo
            KeyComaprers,

            // Support resolving via IEnumerable<>.
            // ReSharper disable once IdentifierTypo
            Enumerables,

            // Support injection via Task.
            Tasks,

            // Support resolving via IResolver<>, IProvider<>, Func<>
            Resolvers,

            // Use cache to optimize a performance.
            Cache,

            // Support configuration using Dto.
            Dto
        }

        public enum Lifetime
        {
            /// <summary>
            /// Singleton instance.
            /// </summary>
            Singleton,

            /// <summary>
            /// IDisposable instance is disposed when a registration is removed or a container is disposed.
            /// </summary>
            AutoDisposing,

            /// <summary>
            /// Singleton instance per container.
            /// </summary>
            PerContainer,

            /// <summary>
            /// Singleton instance per resolving.
            /// </summary>
            PerResolve,

            /// <summary>
            /// Singleton instance per thread.
            /// </summary>
            PerThread,

            /// <summary>
            /// Singleton instance per state.
            /// </summary>
            PerState
        }

        public enum KeyComparer
        {
            // Tag key does not impact on resolving.
            AnyTag,

            // State key does not impact on resolving.
            AnyState,

            // Tag and State keys do not impact on resolving.
            AnyTagAnyState,
        }

        public enum Scope
        {
            /// <summary>
            /// Registration is visible in the current container only.
            /// </summary>
            Internal,

            /// <summary>
            /// Registration is be visible for whole hierarchy of containers.
            /// </summary>
            Global,

            /// <summary>
            /// Registration is be invisible via Registrations.
            /// </summary>
            Private
        }
    }
}
