namespace DevTeam.IoC.Contracts.Dto
{
    using System.Collections.Generic;

    public interface IBindingDto
    {
        IEnumerable<IConstructorArgumentDto> ConstructorParameters { get; }
    }
}
