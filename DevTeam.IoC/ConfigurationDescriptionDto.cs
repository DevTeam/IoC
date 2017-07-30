namespace DevTeam.IoC
{
    using System;
    using Contracts;
    using Contracts.Dto;

    internal sealed class ConfigurationDescriptionDto: IConfigurationDescriptionDto
    {
        public ConfigurationDescriptionDto([NotNull] string description)
        {
            Description = description ?? throw new ArgumentNullException(nameof(description));
        }

        public string Description { get; }
    }
}
