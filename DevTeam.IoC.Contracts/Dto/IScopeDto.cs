namespace DevTeam.IoC.Contracts.Dto
{
    using Contracts;

    public interface IScopeDto : IKeyDto
    {
        Wellknown.Scopes Scope { get; }
    }
}