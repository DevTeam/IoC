namespace DevTeam.IoC.Contracts.Dto
{
    using System.Collections.Generic;

    [PublicAPI]
    public interface IRegisterDto : IConfigurationStatementDto
    {
        IEnumerable<IRegisterStatementDto> Keys { [CanBeNull] get; }

        string AutowiringTypeName { [CanBeNull] get; }

        string FactoryMethodName { [CanBeNull] get; }

        IEnumerable<IParameterDto> ConstructorParameters { [CanBeNull] get; }

        IEnumerable<IMethodDto> Methods { [CanBeNull] get; }

        IEnumerable<IPropertyDto> Properties { [CanBeNull] get; }
    }
}