namespace DevTeam.IoC.Contracts.Dto
{
    using System.Collections.Generic;

    public interface IApplyDto : IConfigurationStatementDto
    {
        IEnumerable<IApplyStatementDto> Statements { get; }
    }
}