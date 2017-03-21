namespace DevTeam.IoC
{
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;

    internal class TypeMetadata
    {
        public TypeMetadata(MethodMetadata constructor, IEnumerable<MethodMetadata> methods, IEnumerable<PropertyMetadata> properties)
        {
            Constructor = constructor;
            Methods = methods;
            Properties = properties;
        }

        public MethodMetadata Constructor { [CanBeNull] get; }

        public IEnumerable<MethodMetadata> Methods { [CanBeNull] get; }

        public IEnumerable<PropertyMetadata> Properties { [CanBeNull] get; }

        public bool IsEmpty => Constructor == null && (Methods == null || !Methods.Any()) && (Properties == null || !Properties.Any());
    }
}
