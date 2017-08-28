namespace DevTeam.IoC.Contracts
{
    using System;

#if !NETCOREAPP1_0 && !NETSTANDARD1_0 && !NETSTANDARD1_5
    using System.Runtime.Serialization;

    [Serializable]
#endif
    [PublicAPI]
    public class ContainerException: InvalidOperationException
    {
        public ContainerException([NotNull] string message)
            : base(message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
        }

        public ContainerException([NotNull] string message, [NotNull] Exception innerException)
            : base(message, innerException)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (innerException == null) throw new ArgumentNullException(nameof(innerException));
        }

#if !NETCOREAPP1_0 && !NETSTANDARD1_0 && !NETSTANDARD1_5
        protected ContainerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}
