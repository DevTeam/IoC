﻿namespace DevTeam.IoC.Tests
{
    using System;
    using System.IO;
    using Contracts;

    internal sealed class ReferenceDescriptionResolver: IReferenceDescriptionResolver
    {
        public string ResolveReference(string reference)
        {
            if (reference == null) throw new ArgumentNullException(nameof(reference));
            if (reference.IsNullOrWhiteSpace()) throw new ArgumentException("Value cannot be null or whitespace.", nameof(reference));
            var fileContent = File.ReadAllText(Path.Combine(TestsExtensions.GetBinDirectory(), reference));
            if (fileContent.IsNullOrWhiteSpace()) throw new ArgumentException("Value cannot be null or whitespace.", nameof(fileContent));
            return fileContent;
        }
    }
}
