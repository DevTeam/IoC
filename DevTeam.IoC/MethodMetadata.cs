namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using Contracts;

    internal sealed class MethodMetadata
    {
        public MethodMetadata([NotNull] string name, [NotNull] IEnumerable<IParameterMetadata> parameters)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        }

        public string Name { [NotNull] get; }

        public IEnumerable<IParameterMetadata> Parameters { [NotNull] get; }
    }
}
