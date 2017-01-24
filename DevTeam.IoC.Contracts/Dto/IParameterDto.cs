namespace DevTeam.IoC.Contracts.Dto
{
    using System.Collections.Generic;

    public interface IParameterDto
    {
        string TypeName { get; }

        IEnumerable<IKeyDto> Keys { get; }

        string Value { get; }
    }
}
