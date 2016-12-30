namespace DevTeam.IoC.Contracts.Dto
{
    using System.Collections.Generic;

    public interface IConfigurationDto : IEnumerable<IConfigurationStatementDto>
    {
    }
}