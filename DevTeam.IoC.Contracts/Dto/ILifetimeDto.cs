namespace DevTeam.IoC.Contracts.Dto
{
    using Contracts;

    public interface ILifetimeDto : IKeyDto
    {
        Wellknown.Lifetimes Lifetime { get; }
    }
}