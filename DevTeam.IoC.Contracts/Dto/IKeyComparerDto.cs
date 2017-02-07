namespace DevTeam.IoC.Contracts.Dto
{
    using Contracts;

    [PublicAPI]
    public interface IKeyComparerDto : IRegisterStatementDto
    {
        Wellknown.KeyComparers KeyComparer { get; }
    }
}