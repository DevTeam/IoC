namespace DevTeam.IoC.Tests
{
    using Contracts;
    using Shouldly;
    using Xunit;

    public class ResolveTests
    {
        [Fact]
        public void ResolveUsingSimpleTag()
        {
            // Given
            using (var container = CreateContainer()
                .Configure().DependsOn(Wellknown.Feature.Default).ToSelf()
                .Register().Autowiring<MyClassWithSimpleTagDependency, MyClassWithSimpleTagDependency>().ToSelf()
                .Register().Contract<string>().FactoryMethod(ctx => "z").ToSelf()
                .Register().Tag(MyTag.A).Contract<string>().FactoryMethod(ctx => "a").ToSelf()
                .Register().Tag(MyTag.B).Contract<string>().FactoryMethod(ctx => "b").ToSelf()
                .Register().Tag(MyTag.C).Contract<string>().FactoryMethod(ctx => "c").ToSelf())
            {
                // When
                var obj = container.Resolve().Instance<MyClassWithSimpleTagDependency>();

                // Then
                container.Resolve().Instance<string>().ShouldBe("z");
                obj.Val.ShouldBe("a");
            }
        }

#if !NET35
        [Theory]
        [InlineData(Wellknown.Lifetime.Singleton)]
        [InlineData(Wellknown.Lifetime.PerContainer)]
        [InlineData(Wellknown.Lifetime.AutoDisposing)]
        public void ResolveUsingTag(Wellknown.Lifetime lifetime)
        {
            // Given
            using (var container = CreateContainer()
                .Configure().DependsOn(Wellknown.Feature.Default).ToSelf()
                .Register().Lifetime(lifetime).Autowiring<MyClassWithTagDependency, MyClassWithTagDependency>().ToSelf()
                .Register().Lifetime(lifetime).Autowiring<IDep, Dep1>().ToSelf()
                .Register().Lifetime(lifetime).Tag("a").Autowiring<IDep, Dep1>().ToSelf()
                .Register().Lifetime(lifetime).Tag("b").Autowiring<IDep, Dep2>().ToSelf())
            {
                // When
                var obj = container.Resolve().Instance<MyClassWithTagDependency>();

                // Then
                obj.Val.ShouldBeOfType<Dep2>();
            }
        }
#endif

        private static Container CreateContainer()
        {
            return new Container();
        }

        public enum MyTag
        {
            A,
            B,
            C
        }

        public class MyClassWithSimpleTagDependency
        {
            public string Val { get; }

            public MyClassWithSimpleTagDependency([Tag(MyTag.A)] string val)
            {
                Val = val;
            }
        }

        public class MyClassWithTagDependency
        {
            public IDep Val { get; }

            public MyClassWithTagDependency([Tag("b")] IDep val)
            {
                Val = val;
            }
        }

        public interface IDep
        {
        }

        public class Dep1: IDep
        {
        }

        public class Dep2: IDep
        {
        }
    }
}
