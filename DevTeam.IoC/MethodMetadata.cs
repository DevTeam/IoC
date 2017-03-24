namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using Contracts;

    internal sealed class MethodMetadata
    {
        public MethodMetadata([NotNull] string name, [NotNull] IEnumerable<IParameterMetadata> parameters)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            Name = name;
            Parameters = parameters;
        }

        public string Name { [NotNull] get; }

        public IEnumerable<IParameterMetadata> Parameters { [NotNull] get; }
    }
}
