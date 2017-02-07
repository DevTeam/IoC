namespace DevTeam.IoC.Contracts.Dto
{
    using Contracts;

    [PublicAPI]
    public interface IScopeDto : IRegisterStatementDto
    {
        Wellknown.Scopes Scope { get; }
    }
}