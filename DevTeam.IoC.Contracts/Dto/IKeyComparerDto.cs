namespace DevTeam.IoC.Contracts.Dto
{
    using Contracts;

    public interface IKeyComparerDto : IRegisterStatementDto
    {
        Wellknown.KeyComparers KeyComparer { get; }
    }
}