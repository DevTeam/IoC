namespace DevTeam.IoC.Contracts.Dto
{
    using Contracts;

    public interface IKeyComparerDto : IKeyDto
    {
        Wellknown.KeyComparers KeyComparer { get; }
    }
}