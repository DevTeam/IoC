namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Contracts;

    internal sealed class TypeResolverContext
    {
        public TypeResolverContext([NotNull] ICollection<Assembly> references, [NotNull] ICollection<string> usings)
        {
            References = references ?? throw new ArgumentNullException(nameof(references));
            Usings = usings ?? throw new ArgumentNullException(nameof(usings));
        }

        public ICollection<Assembly> References { [NotNull] get; }

        public ICollection<string> Usings { [NotNull] get; }
    }
}
