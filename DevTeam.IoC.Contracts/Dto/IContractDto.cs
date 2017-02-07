namespace DevTeam.IoC.Contracts.Dto
{
    using System.Collections.Generic;

    [PublicAPI]
    public interface IContractDto : IRegisterStatementDto
    {
        IEnumerable<string> Contract { get; }
    }
}