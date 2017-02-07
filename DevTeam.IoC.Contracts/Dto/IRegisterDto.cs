namespace DevTeam.IoC.Contracts.Dto
{
    using System.Collections.Generic;

    [PublicAPI]
    public interface IRegisterDto : IConfigurationStatementDto
    {
        IEnumerable<IRegisterStatementDto> Keys { get; }

        string AutowiringTypeName { get; }

        string FactoryMethodName { get; }

        IEnumerable<IParameterDto> ConstructorParameters { get; }
    }
}