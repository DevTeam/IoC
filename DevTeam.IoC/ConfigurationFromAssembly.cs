namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;

    internal class ConfigurationFromAssembly: IConfiguration
    {
        private readonly Assembly _assembly;

        public ConfigurationFromAssembly([NotNull] Assembly assembly)
        {
            _assembly = assembly;
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
        }

        public IEnumerable<IConfiguration> GetDependencies(IContainer container)
        {
            yield break;
        }

        public IEnumerable<IDisposable> Apply(IContainer container)
        {
            return 
                from typeInfo in _assembly.DefinedTypes
                where typeInfo.GetCustomAttributes<ContractAttribute>().Any()
                let type = typeInfo.AsType()
                select container.Register().Attributes(typeInfo.AsType()).Autowiring(type).Apply();
        }
    }
}
