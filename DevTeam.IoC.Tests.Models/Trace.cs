namespace DevTeam.IoC.Tests.Models
{
    using System.Collections.Generic;

    internal class Trace: ITrace
    {
        public IList<string> Output { get; } = new List<string>(100);
    }
}
