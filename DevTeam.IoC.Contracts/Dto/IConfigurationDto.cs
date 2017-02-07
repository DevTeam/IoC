namespace DevTeam.IoC.Contracts.Dto
{
    using System.Collections.Generic;

    [PublicAPI]
    public interface IConfigurationDto : IEnumerable<IConfigurationStatementDto>
    {
    }
}