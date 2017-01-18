namespace DevTeam.IoC.Contracts.Dto
{
    using System.Collections.Generic;

    public interface IBindingDto
    {
        IEnumerable<IParameterDto> ConstructorParameters { get; }
    }
}
