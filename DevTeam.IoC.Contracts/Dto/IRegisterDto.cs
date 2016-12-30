namespace DevTeam.IoC.Contracts.Dto
{
    using System.Collections.Generic;

    public interface IRegisterDto : IApplyStatementDto
    {
        ITagDto TargetTag { get; }

        IEnumerable<IRegisterStatementDto> Statements { get; }

        string AutowiringTypeName { get; }

        string FactoryMethodName { get; }
    }
}