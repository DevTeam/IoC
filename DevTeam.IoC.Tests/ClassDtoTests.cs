namespace DevTeam.IoC.Tests
{
    using System.Linq;
    using Configurations.Json;
    using Shouldly;
    using Xunit;

    public class ClassDtoTests
    {
#if !NET35
        [Theory]
        [InlineData("Cat")]
        [InlineData(" Cat    ")]
        [InlineData(" Cat")]
        public void ShouldSpecifySimpleAutowiringWhenOnlyClassTypeDefined(string className)
        {
            // Given

            // When
            var classDto = new ClassDto { Class = className };

            // Then
            className = className.Trim();
            classDto.AutowiringTypeName.ShouldBe(className);
            classDto.Keys.OfType<ContractDto>().Count(i => i.Contract.Contains(className)).ShouldBe(1);
        }

        [Theory]
        [InlineData("ICat")]
        [InlineData(" ICat     ")]
        [InlineData(" ICat")]
        public void ShouldSpecifySimpleAutowiringWhenClassTypeAndInterfaceDefined(string interfaceName)
        {
            // Given

            // When
            var classDto = new ClassDto { Class = "Cat :" + interfaceName };

            // Then
            classDto.AutowiringTypeName.ShouldBe("Cat");
            classDto.Keys.OfType<ContractDto>().Count().ShouldBe(1);
            classDto.Keys.OfType<ContractDto>().Count(i => i.Contract.Contains(interfaceName.Trim())).ShouldBe(1);
        }

        [Theory]
        [InlineData("ICat,IDisposable")]
        [InlineData(" ICat,   IDisposable  ")]
        [InlineData(" ICat,IDisposable ")]
        public void ShouldSpecifySimpleAutowiringWhenClassTypeAndSeveralInterfaceDefined(string interfacesName)
        {
            // Given

            // When
            var classDto = new ClassDto { Class = "Cat :" + interfacesName };

            // Then
            classDto.AutowiringTypeName.ShouldBe("Cat");
            classDto.Keys.OfType<ContractDto>().Count().ShouldBe(2);
            classDto.Keys.OfType<ContractDto>().Count(i => i.Contract.Contains("ICat")).ShouldBe(1);
            classDto.Keys.OfType<ContractDto>().Count(i => i.Contract.Contains("IDisposable")).ShouldBe(1);
        }
#endif
    }
}
