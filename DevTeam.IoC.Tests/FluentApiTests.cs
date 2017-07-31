namespace DevTeam.IoC.Tests
{
    using System.Linq;
    using Contracts;
    using Shouldly;
    using Xunit;

    public class FluentApiTests
  {
        [Fact]
        public void CreateNewRegistrationWhenUseAndFunc()
        {
            // Given
            using (var container = CreateContainer().Configure().DependsOn(Wellknown.Feature.Default).ToSelf())
            {
                // When
                var registration1 = (Registration<IContainer>)container.Register().Lifetime(Wellknown.Lifetime.PerState).Tag(1).Contract<ISimpleService>();
                var registration2 = (Registration<IContainer>)registration1.New().Lifetime(Wellknown.Lifetime.PerContainer).Tag(2).Contract<ISimpleService>();

                // Then
                registration1.ShouldNotBe(registration2);
                registration1.Keys.ShouldNotBe(registration2.Keys);
                registration1.Extensions.ShouldNotBe(registration2.Extensions);
                registration2.Keys.Count().ShouldBe(1);
                registration2.Extensions.Count.ShouldBe(1);
            }
        }

      [Fact]
      public void UsePredefinedRegistrationWhenToFunc()
      {
          // Given
          using (var container = CreateContainer().Configure().DependsOn(Wellknown.Feature.Default).ToSelf())
          {
              // When
              var registration1 = (Registration<IContainer>)container.Register().Lifetime(Wellknown.Lifetime.PerState).Tag(1).Contract<ISimpleService>();
              var registration2 = (Registration<IContainer>)registration1.To();

              // Then
              registration1.ShouldNotBe(registration2);
              registration1.Keys.ShouldBe(registration2.Keys);
              registration1.Extensions.ShouldBe(registration2.Extensions);
              registration2.Keys.Count().ShouldBe(1);
              registration2.Extensions.Count.ShouldBe(1);
            }
      }

        private IContainer CreateContainer()
        {
            return new Container();
        }
    }
}