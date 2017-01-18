namespace DevTeam.IoC.Contracts.Dto
{
    using System.Collections.Generic;

    public interface IConstructorArgumentDto
    {
        string TypeName { get; }

        IEnumerable<IKeyDto> Keys { get; }
    }
}
