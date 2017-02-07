namespace DevTeam.IoC.Contracts.Dto
{
    using System.Collections.Generic;

    [PublicAPI]
    public interface IParameterDto
    {
        string TypeName { [NotNull] get; }

        string Value { [CanBeNull] get; }

        IStateDto State { [CanBeNull] get; }

        IEnumerable<IRegisterStatementDto> Dependency { [CanBeNull] get; }
    }
}
