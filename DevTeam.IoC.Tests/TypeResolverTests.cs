namespace DevTeam.IoC.Tests
{
    using System;
    using Contracts;
    using Models;
    using Shouldly;
    using Xunit;
    using System.Collections.Generic;
    using System.Reflection;

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
        [InlineData("DevTeam.IoC.Tests", "DevTeam.IoC.Tests", "SomeClass.NestedClass", true, typeof(SomeClass.NestedClass))]
        [InlineData("DevTeam.IoC.Tests", "DevTeam.IoC.Tests", "SomeClass.NestedClass.NesteClass2", true, typeof(SomeClass.NestedClass.NesteClass2))]
        [InlineData("DevTeam.IoC.Tests", "DevTeam.IoC", "Tests.SomeClass.NestedClass.NesteClass2", true, typeof(SomeClass.NestedClass.NesteClass2))]
        [InlineData("DevTeam.IoC.Tests", "DevTeam.IoC.Tests", "SomeClass.NestedClass.NesteClass3<>", true, typeof(SomeClass.NestedClass.NesteClass3<>))]
        [InlineData("DevTeam.IoC.Tests", "DevTeam.IoC.Tests", "SomeClass.NestedClass.NesteClass3<int>", true, typeof(SomeClass.NestedClass.NesteClass3<int>))]
        public void ShouldTryResolveType(
            string references,
            string usingStatements,
            string typeName,
            bool expectedResult,
            Type expectedType)
        {
            // Given
            var typeResolver = new TypeResolver();
            List<Assembly> refList = new List<Assembly>();
            List<string> usingList = new List<string>();
            foreach (var reference in references.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                refList.Add(Assembly.Load(new AssemblyName(reference)));
            }

            foreach (var usingItem in usingStatements.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                usingList.Add(usingItem);
            }

            // When
            var actualResult = typeResolver.TryResolveType(refList, usingList, typeName, out Type actualType);

            // Then
            actualResult.ShouldBe(expectedResult);
            actualType.ShouldBe(expectedType);
        }
#endif
    }

    public class SomeClass
    {
        public class NestedClass
        {
            public class NesteClass2
            {
            }

            public class NesteClass3<T>
            {
            }
        }
    }
}
