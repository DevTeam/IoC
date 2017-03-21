namespace DevTeam.IoC.Contracts.Dto
{
    using System.Collections.Generic;
    using Contracts;

    [PublicAPI]
    public interface IMethodDto
    {
        string Name { [NotNull] get; }

        IEnumerable<IParameterDto> MethodParameters { [CanBeNull] get; }
    }
}