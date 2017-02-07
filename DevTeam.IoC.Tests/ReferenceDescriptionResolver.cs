namespace DevTeam.IoC.Tests
{
    using System;
    using System.IO;
    using Contracts;

    internal class ReferenceDescriptionResolver: IReferenceDescriptionResolver
    {
        public string ResolveReference(string reference)
        {
            if (reference == null) throw new ArgumentNullException(nameof(reference));
            if (string.IsNullOrWhiteSpace(reference)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(reference));
            var fileContent = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, reference));
            if (string.IsNullOrWhiteSpace(fileContent)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(fileContent));
            return fileContent;
        }
    }
}
