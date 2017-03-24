namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Contracts;

    internal class TypeResolverContext
    {
        public TypeResolverContext([NotNull] ICollection<Assembly> references, [NotNull] ICollection<string> usings)
        {
            if (references == null) throw new ArgumentNullException(nameof(references));
            if (usings == null) throw new ArgumentNullException(nameof(usings));
            References = references;
            Usings = usings;
        }

        public ICollection<Assembly> References { [NotNull] get; }

        public ICollection<string> Usings { [NotNull] get; }
    }
}
