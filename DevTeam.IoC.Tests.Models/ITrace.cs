namespace DevTeam.IoC.Tests.Models
{
    using System.Collections.Generic;

    public interface ITrace
    {
        IList<string> Output { get; }
    }
}
