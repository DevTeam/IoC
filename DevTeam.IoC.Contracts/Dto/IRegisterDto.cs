namespace DevTeam.IoC.Contracts.Dto
{
    using System.Collections.Generic;

    public interface IRegisterDto : IConfigurationStatementDto
    {
        IEnumerable<IRegisterStatementDto> Keys { get; }

        string AutowiringTypeName { get; }

        string FactoryMethodName { get; }

        IEnumerable<IParameterDto> ConstructorParameters { get; }
    }
}