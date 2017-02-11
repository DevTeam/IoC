namespace DevTeam.IoC.Tests
{
    using System;
    using Contracts;
    using Models;
    using NUnit.Framework;
    using Shouldly;

    [TestFixture]
    public class TypeResolverTests
    {
        [Test]
        [TestCase("", "", "int", true, typeof(int))]
        [TestCase("", "", "System.Int32", true, typeof(int))]
        [TestCase("", "", "string", true, typeof(string))]
        [TestCase("", "", "System.String", true, typeof(string))]
        [TestCase("DevTeam.IoC.Contracts", "DevTeam.IoC.Contracts", "IResolver", true, typeof(IResolver))]
        [TestCase("DevTeam.IoC.Contracts", "", "DevTeam.IoC.Contracts.IResolver", true, typeof(IResolver))]
        [TestCase("DevTeam.IoC.Contracts", "DevTeam.IoC.Contracts", "IResolver<>", true, typeof(IResolver<>))]
        [TestCase("DevTeam.IoC.Contracts", "", "DevTeam.IoC.Contracts.IResolver<>", true, typeof(IResolver<>))]
        [TestCase("DevTeam.IoC.Contracts", "DevTeam.IoC.Contracts", "IResolver<int,string>", true, typeof(IResolver<int, string>))]
        [TestCase("DevTeam.IoC.Contracts", "DevTeam.IoC.Contracts", "IResolver<,>", true, typeof(IResolver<,>))]
        [TestCase("DevTeam.IoC.Contracts", "", "DevTeam.IoC.Contracts.IResolver<,,>", true, typeof(IResolver<,,>))]
        [TestCase("DevTeam.IoC.Contracts", "DevTeam.IoC.Contracts", "IResolver<string>", true, typeof(IResolver<string>))]
        [TestCase("DevTeam.IoC.Contracts", "DevTeam.IoC.Contracts", "IResolver<IResolver<string>>", true, typeof(IResolver<IResolver<string>>))]
        [TestCase("DevTeam.IoC.Contracts", "DevTeam.IoC.Contracts", "IResolver<IResolver<string, int, float>, double, IResolver<IResolver<IResolver<string, int, float>>>>", true, typeof(IResolver<IResolver<string, int, float>, double, IResolver<IResolver<IResolver<string, int, float>>>>))]
        [TestCase("DevTeam.IoC.Tests.Models", "DevTeam.IoC.Tests.Models", "ITimer", true, typeof(ITimer))]
        public void ShouldTryResolveType(
            string references,
            string usingStatements,
            string typeName,
            bool expectedResult,
            Type expectedType)
        {
            // Given
            var typeResolver = new TypeResolver();
            foreach (var reference in references.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                typeResolver.AddReference(reference);
            }

            foreach (var usingItem in usingStatements.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                typeResolver.AddUsingStatement(usingItem);
            }
            
            // When
            Type actualType;
            var actualResult = typeResolver.TryResolveType(typeName, out actualType);

            // Then
            actualResult.ShouldBe(expectedResult);
            actualType.ShouldBe(expectedType);
        }
    }
}
