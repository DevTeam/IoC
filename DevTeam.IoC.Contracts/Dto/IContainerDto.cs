namespace DevTeam.IoC.Contracts.Dto
{
    using System.Collections.Generic;

    public interface IContainerDto : IConfigurationStatementDto
    {
        IEnumerable<IConfigurationStatementDto> Statements { get; }
    }
}