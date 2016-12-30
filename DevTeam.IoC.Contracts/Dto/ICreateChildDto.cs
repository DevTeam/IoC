namespace DevTeam.IoC.Contracts.Dto
{
    using System.Collections.Generic;

    public interface ICreateChildDto : IApplyStatementDto
    {
        IEnumerable<IApplyStatementDto> Statements { get; }
    }
}