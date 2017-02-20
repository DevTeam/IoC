namespace ConsoleApp
{
    using System;
    using System.Collections.Generic;
    using DevTeam.IoC.Contracts;

    public class Config: IConfiguration
    {
        public IEnumerable<IConfiguration> GetDependencies(IContainer container)
        {
            yield break;
        }

        public IEnumerable<IDisposable> Apply(IContainer container)
        {
            yield break;
        }
    }
}
