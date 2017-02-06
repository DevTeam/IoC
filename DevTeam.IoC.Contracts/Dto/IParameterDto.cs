namespace DevTeam.IoC.Contracts.Dto
{
    using System.Collections.Generic;

    public interface IParameterDto
    {
        string TypeName { get; }

        string Value { get; }

        IStateDto State { get; }

        IEnumerable<IRegisterStatementDto> Dependency { get; }
    }
}
