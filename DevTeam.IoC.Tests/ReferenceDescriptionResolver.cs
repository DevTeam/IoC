namespace DevTeam.IoC.Tests
{
    using System;
    using System.IO;
    using Contracts;

    internal class ReferenceDescriptionResolver: IReferenceDescriptionResolver
    {
        public string ResolveReference(string reference)
        {
            return File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, reference));
        }
    }
}
