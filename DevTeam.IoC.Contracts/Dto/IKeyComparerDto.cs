namespace DevTeam.IoC.Contracts.Dto
{
    using Contracts;

    [PublicAPI]
    public interface IKeyComparerDto : IRegisterStatementDto
    {
        Wellknown.KeyComparer KeyComparer { [NotNull] get; }
    }
}