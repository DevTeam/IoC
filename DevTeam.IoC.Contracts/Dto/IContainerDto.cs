﻿namespace DevTeam.IoC.Contracts.Dto
{
    using System.Collections.Generic;

    [PublicAPI]
    public interface IContainerDto : IConfigurationStatementDto
    {
        IEnumerable<IConfigurationStatementDto> Statements { [NotNull] get; }

        ITagDto Tag { [CanBeNull] get; }
    }
}