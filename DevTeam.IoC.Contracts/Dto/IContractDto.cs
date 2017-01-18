namespace DevTeam.IoC.Contracts.Dto
{
    using System.Collections.Generic;

    public interface IContractDto : IKeyDto
    {
        IEnumerable<string> Contract { get; }
    }
}