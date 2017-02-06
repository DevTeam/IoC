namespace DevTeam.IoC.Contracts.Dto
{
    using System.Collections.Generic;

    public interface IContractDto : IRegisterStatementDto
    {
        IEnumerable<string> Contract { get; }
    }
}