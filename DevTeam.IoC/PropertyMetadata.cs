namespace DevTeam.IoC
{
    using System;
    using Contracts;

    internal sealed class PropertyMetadata
    {
        public PropertyMetadata([NotNull] string name, [NotNull] IParameterMetadata parameter)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
        }

        public string Name { [NotNull] get; }

        public IParameterMetadata Parameter { [NotNull] get; }
    }
}
