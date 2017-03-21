namespace DevTeam.IoC
{
    using System;
    using Contracts;

    internal class PropertyMetadata
    {
        public PropertyMetadata([NotNull] string name, [NotNull] IParameterMetadata parameter)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (parameter == null) throw new ArgumentNullException(nameof(parameter));
            Name = name;
            Parameter = parameter;
        }

        public string Name { [NotNull] get; }

        public IParameterMetadata Parameter { [NotNull] get; }
    }
}
