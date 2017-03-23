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
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            _assembly = assembly;
            
        }

        public IEnumerable<IConfiguration> GetDependencies(IContainer container)
        {
            yield break;
        }

        public IEnumerable<IDisposable> Apply(IContainer container)
        {
            var reflection = container.Resolve().Instance<IReflection>();
            return 
                from typeInfo in reflection.GetDefinedTypes(_assembly)
                where typeInfo.GetCustomAttributes<ContractAttribute>().Any()
                let type = typeInfo.Type
                select (IDisposable)container.Register().Attributes(typeInfo.Type).Autowiring(type);
        }
    }
}
