﻿namespace DevTeam.IoC.Configurations.Json
{
    using System.Collections.Generic;
    using Contracts.Dto;
    using Newtonsoft.Json;

    [JsonArray(AllowNullItems = false)]
    internal sealed class ConfigurationDto: List<IConfigurationStatementDto>, IConfigurationDto
    {
    }
}
