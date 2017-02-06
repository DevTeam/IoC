namespace DevTeam.IoC.Contracts.Dto
{
    using Contracts;

    public interface ILifetimeDto : IRegisterStatementDto
    {
        Wellknown.Lifetimes Lifetime { get; }
    }
}