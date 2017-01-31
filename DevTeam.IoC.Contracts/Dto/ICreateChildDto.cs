namespace DevTeam.IoC.Contracts.Dto
{
    using System.Collections.Generic;

    public interface ICreateChildDto : IConfigurationStatementDto
    {
        IEnumerable<IConfigurationStatementDto> Statements { get; }
    }
}