namespace DevTeam.IoC.Configurations.Json
{
    using Contracts;
    using Contracts.Dto;
    using Newtonsoft.Json;

    internal sealed class DependencyAssemblyDto: IDependencyAssemblyDto
    {
        [JsonProperty("dependency.assembly", Required = Required.Always)]
        public string AssemblyName { get; [NotNull] set; }
    }
}
