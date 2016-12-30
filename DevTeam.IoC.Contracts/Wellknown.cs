namespace DevTeam.IoC.Contracts
{
    public static class Wellknown
    {
        public enum Configurations
        {
            All,

            ChildrenContainers,

            Lifetimes,

            Scopes,

            KeyComaprers,

            Enumerables,

            Tasks,

            Resolvers,

            Cache,

            Dto
        }

        public enum Lifetimes
        {
            /// <summary>
            /// Shared for all resolves and self controlled.
            /// </summary>
            Singleton,

            /// <summary>
            /// Disposable contracts will be disposed if a regestry is removed or a container is destroed.
            /// </summary>
            Controlled,

            /// <summary>
            /// Shared for all resolves for the one container and self controlled.
            /// </summary>
            PerContainer,

            /// <summary>
            /// Shared for all resolves for the one resolving and self controlled.
            /// </summary>
            PerResolveLifetime,

            /// <summary>
            /// Shared for all resolves for the one thread and self controlled.
            /// </summary>
            PerThreadLifetime,

            /// <summary>
            /// Shared for all resolves for the specific state.
            /// </summary>
            PerState
        }

        public enum KeyComparers
        {
            AnyTag,

            AnyState,

            AnyTagAnyState,
        }

        public enum Scopes
        {
            /// <summary>
            /// Registration will be visible in the current container only.
            /// </summary>
            Internal,

            /// <summary>
            /// Registration will be visible in whole hierarchy of containers.
            /// </summary>
            Global
        }
    }
}
