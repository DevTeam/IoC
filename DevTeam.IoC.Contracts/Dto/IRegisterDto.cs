﻿namespace DevTeam.IoC.Contracts.Dto
{
    using System.Collections.Generic;

    public interface IRegisterDto : IConfigurationStatementDto
    {
        ITagDto TargetTag { get; }

        IEnumerable<IKeyDto> Keys { get; }

        string AutowiringTypeName { get; }

        string FactoryMethodName { get; }

        IEnumerable<IParameterDto> ConstructorParameters { get; }
    }
}