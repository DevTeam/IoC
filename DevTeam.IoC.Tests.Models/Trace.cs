namespace DevTeam.IoC.Tests.Models
{
    using System.Collections.Generic;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal sealed class Trace: ITrace
    {
        public IList<string> Output { get; } = new List<string>(100);
    }
}
