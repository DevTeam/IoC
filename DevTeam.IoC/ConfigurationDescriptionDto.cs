namespace DevTeam.IoC
{
    using System;
    using Contracts.Dto;

    internal class ConfigurationDescriptionDto: IConfigurationDescriptionDto
    {
        public ConfigurationDescriptionDto(string description)
        {
            if (description == null) throw new ArgumentNullException(nameof(description));
            Description = description;
        }

        public string Description { get; }
    }
}
