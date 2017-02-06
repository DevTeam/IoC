namespace DevTeam.IoC.Contracts.Dto
{
    using Contracts;

    public interface IScopeDto : IRegisterStatementDto
    {
        Wellknown.Scopes Scope { get; }
    }
}