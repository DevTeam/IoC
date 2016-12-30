namespace DevTeam.IoC.Contracts.Dto
{
    using System.Collections.Generic;

    public interface IGetDependeciesDto : IConfigurationStatementDto
    {
        IEnumerable<IDependencyDto> Dependecies { get; set; }
    }
}