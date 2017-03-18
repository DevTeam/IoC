namespace DevTeam.IoC.Tests
{
    using System;
    using Contracts;
    using Models;
    using Shouldly;
    using Xunit;

    public class TypeResolverTests
    {
#if !NET35
        [Theory]
        [InlineData("", "", "int", true, typeof(int))]
        [InlineData("", "", "System.Int32", true, typeof(int))]
        [InlineData("", "", "string", true, typeof(string))]
        [InlineData("", "", "System.String", true, typeof(string))]
        [InlineData("DevTeam.IoC.Contracts", "DevTeam.IoC.Contracts", "IResolver", true, typeof(IResolver))]
        [InlineData("DevTeam.IoC.Contracts", "", "DevTeam.IoC.Contracts.IResolver", true, typeof(IResolver))]
        [InlineData("DevTeam.IoC.Contracts", "DevTeam.IoC.Contracts", "IResolver<>", true, typeof(IResolver<>))]
        [InlineData("DevTeam.IoC.Contracts", "", "DevTeam.IoC.Contracts.IResolver<>", true, typeof(IResolver<>))]
        [InlineData("DevTeam.IoC.Contracts", "DevTeam.IoC.Contracts", "IResolver<int,string>", true, typeof(IResolver<int, string>))]
        [InlineData("DevTeam.IoC.Contracts", "DevTeam.IoC.Contracts", "IResolver<,>", true, typeof(IResolver<,>))]
        [InlineData("DevTeam.IoC.Contracts", "", "DevTeam.IoC.Contracts.IResolver<,,>", true, typeof(IResolver<,,>))]
        [InlineData("DevTeam.IoC.Contracts", "DevTeam.IoC.Contracts", "IResolver<string>", true, typeof(IResolver<string>))]
        [InlineData("DevTeam.IoC.Contracts", "DevTeam.IoC.Contracts", "IResolver<IResolver<string>>", true, typeof(IResolver<IResolver<string>>))]
        [InlineData("DevTeam.IoC.Contracts", "DevTeam.IoC.Contracts", "IResolver<IResolver<string, int, float>, double, IResolver<IResolver<IResolver<string, int, float>>>>", true, typeof(IResolver<IResolver<string, int, float>, double, IResolver<IResolver<IResolver<string, int, float>>>>))]
        [InlineData("DevTeam.IoC.Tests.Models", "DevTeam.IoC.Tests.Models", "ITimer", true, typeof(ITimer))]
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
            var actualResult = typeResolver.TryResolveType(typeName, out Type actualType);

            // Then
            actualResult.ShouldBe(expectedResult);
            actualType.ShouldBe(expectedType);
        }
#endif
    }
}
