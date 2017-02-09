namespace DevTeam.IoC.Contracts.Dto
{
    using Contracts;

    [PublicAPI]
    public interface ILifetimeDto : IRegisterStatementDto
    {
        Wellknown.Lifetime Lifetime { [NotNull] get; }
    }
}