namespace DevTeam.IoC.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Contracts;

    using Moq;

    using NUnit.Framework;

    using Shouldly;

    [TestFixture]
    public class ContainerTests
    {
        private Mock<IResolverFactory> _factory;

        [SetUp]
        public void SetUp()
        {
            _factory = new Mock<IResolverFactory>();
        }

        [Test]
        public void ContainerShouldRegister()
        {
            // Given
            using (var container = CreateContainer())
            {
                object obj = new object();
                _factory.Setup(i => i.Create(It.IsAny<IResolverContext>())).Returns(obj);

                // When
                var registryContext =
                    container.CreateContext(
                        CreateCompositeKeys(container, false, new []{ typeof(string) }),
                        _factory.Object,
                        new IExtension[0]);

                using (container.Register(registryContext))
                {
                    var resolverContext = container.CreateContext(CreateCompositeKey(container, true, new []{ typeof(string) }));
                    var actualObj = container.Resolve(resolverContext);

                    // Then
                    container.Registrations.ShouldContain(CreateCompositeKey(container, false, new[] { typeof(string) }));
                    actualObj.ShouldBe(obj);
                }
            }
        }

        [Test]
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public void ContainerShouldRaiseEvents()
        {
            // Given
            var eventObserver = new EventObserver<IRegistrationEvent>();
            ICompositeKey[] keys;
            using (var container = CreateContainer())
            using (container.Subscribe(eventObserver))
            {
                object obj = new object();
                _factory.Setup(i => i.Create(It.IsAny<IResolverContext>())).Returns(obj);
                keys = CreateCompositeKeys(container, false, new[] {typeof(string) }).ToArray();
                // When
                var registryContext =
                    container.CreateContext(
                        keys,
                        _factory.Object,
                        new IExtension[0]);

                using (container.Register(registryContext))
                {
                    var resolverContext = container.CreateContext(CreateCompositeKey(container, true, new[] { typeof(string) }));
                    container.Resolve(resolverContext);
                }
            }

            eventObserver.Events.Count.ShouldBe(4);

            var event0 = eventObserver.Events[0];
            event0.EventType.ShouldBe(EventObserver<IRegistrationEvent>.EventType.OnNext);
            event0.Value.Stage.ShouldBe(EventStage.Before);
            event0.Value.Action.ShouldBe(EventAction.Add);
            event0.Value.Key.ShouldBe(keys[0]);

            var event1 = eventObserver.Events[1];
            event1.EventType.ShouldBe(EventObserver<IRegistrationEvent>.EventType.OnNext);
            event1.Value.Stage.ShouldBe(EventStage.After);
            event1.Value.Action.ShouldBe(EventAction.Add);
            event1.Value.Key.ShouldBe(keys[0]);

            var event2 = eventObserver.Events[2];
            event2.EventType.ShouldBe(EventObserver<IRegistrationEvent>.EventType.OnNext);
            event2.Value.Stage.ShouldBe(EventStage.Before);
            event2.Value.Action.ShouldBe(EventAction.Remove);
            event2.Value.Key.ShouldBe(keys[0]);

            var event3 = eventObserver.Events[3];
            event3.EventType.ShouldBe(EventObserver<IRegistrationEvent>.EventType.OnNext);
            event3.Value.Stage.ShouldBe(EventStage.After);
            event3.Value.Action.ShouldBe(EventAction.Remove);
            event3.Value.Key.ShouldBe(keys[0]);
        }

        [Test]
        public void ContainerShouldUnregister()
        {
            // Given
            using (var container = CreateContainer())
            {
                var obj = new object();
                _factory.Setup(i => i.Create(It.IsAny<IResolverContext>())).Returns(obj);

                // When
                var registryContext =
                    container.CreateContext(
                        CreateCompositeKeys(container, false, new[] { typeof(string) }),
                        _factory.Object,
                        new IExtension[0]);

                var registration = container.Register(registryContext);
                registration.Dispose();

                // Then
                container.Registrations.ShouldNotContain(CreateCompositeKey(container, false, new[] { typeof(string) }));
            }
        }

        [Test]
        public void ContainerShouldResolveItself()
        {
            // Given
            using (var container = CreateContainer())
            {
                // When
                container.CreateContext(
                    CreateCompositeKeys(container, false, new[] { typeof(IResolver) }, new object[0]),
                    _factory.Object,
                    new IExtension[0]);

                var resolverContext = container.CreateContext(CreateCompositeKey(container, true, new[] { typeof(IResolver) }));
                var resolver = container.Resolve(resolverContext);

                // Then
                resolver.ShouldBe(container);
            }
        }

        [Test]
        public void ContainerShouldResolveFromChild()
        {
            // Given
            using (var container = CreateContainer())
            using (var childContainer1 = new Container(container, "child1"))
            using (var childContainer2 = new Container(childContainer1, "child2"))
            {
                object obj = new object();
                _factory.Setup(i => i.Create(It.IsAny<IResolverContext>())).Returns(obj);

                // When
                var registryContext =
                    container.CreateContext(
                        CreateCompositeKeys(container, false, new[] { typeof(string) }, new object[0]),
                        _factory.Object,
                        new IExtension[0]);

                using (container.Register(registryContext))
                {
                    var resolverContext = childContainer2.CreateContext(CreateCompositeKey(container, true, new[] { typeof(string) }));
                    var actualObj = childContainer2.Resolve(resolverContext);

                    // Then
                    actualObj.ShouldBe(obj);
                }
            }
        }

        [Test]
        public void ContainerShouldRegisterUndefinedGenericType()
        {
            // Given
            using (var container = CreateContainer())
            {
                Mock<IGenericService<string>> mock = new Mock<IGenericService<string>>();
                _factory.Setup(i => i.Create(It.IsAny<IResolverContext>())).Returns(mock.Object);

                // When
                var registryContext =
                    container.CreateContext(
                        CreateCompositeKeys(container, false, new[] { typeof(IGenericService<>) }),
                        _factory.Object,
                        new IExtension[0]);

                using (container.Register(registryContext))
                {
                    var resolverContext = container.CreateContext(CreateCompositeKey(container, true, new[] { typeof(IGenericService<string>) }));
                    var actualObj = container.Resolve(resolverContext);

                    // Then
                    container.Registrations.ShouldContain(CreateCompositeKey(container, false, new[] { typeof(IGenericService<>) }));
                    actualObj.ShouldBe(mock.Object);
                }
            }
        }

        [Test]
        public void ContainerShouldRegisterDefinedGenericType()
        {
            // Given
            using (var container = CreateContainer())
            {
                Mock<IGenericService<string>> mock = new Mock<IGenericService<string>>();
                _factory.Setup(i => i.Create(It.IsAny<IResolverContext>())).Returns(mock.Object);

                // When
                var registryContext =
                    container.CreateContext(
                        CreateCompositeKeys(container, false, new[] { typeof(IGenericService<string>) }),
                        _factory.Object,
                        new IExtension[0]);

                using (container.Register(registryContext))
                {
                    var resolverContext = container.CreateContext(CreateCompositeKey(container, true, new[] { typeof(IGenericService<string>) }));
                    var actualObj = container.Resolve(resolverContext);

                    // Then
                    container.Registrations.ShouldContain(CreateCompositeKey(container, false, new[] { typeof(IGenericService<string>) }));
                    actualObj.ShouldBe(mock.Object);
                }
            }
        }

        [Test]
        public void ContainerShouldNotResolveWhenGenericTypeArgsAreNotEq()
        {
            // Given
            using (var container = CreateContainer())
            {
                var genericMock = new Mock<IGenericService<string>>();
                _factory.Setup(i => i.Create(It.IsAny<IResolverContext>())).Returns(genericMock.Object);

                // When
                var registryContext =
                    container.CreateContext(
                        CreateCompositeKeys(container, false, new[] { typeof(IGenericService<string>) }),
                        _factory.Object,
                        new IExtension[0]);

                using (container.Register(registryContext))
                {
                    IResolverContext resolverContext;
                    var contextCreated = container.TryCreateContext(CreateCompositeKey(container, true, new[] { typeof(IGenericService<int>) }), out resolverContext);

                    // Then
                    contextCreated.ShouldBe(false);
                }
            }
        }

        [Test]
        public void ContainerShouldThrowExceptionWhenResolveThrowsException()
        {
            // Given
            using (var container = CreateContainer())
            {
                var ex = new Exception("test");
                _factory.Setup(i => i.Create(It.IsAny<IResolverContext>())).Throws(ex);

                // When
                var registryContext =
                    container.CreateContext(
                        CreateCompositeKeys(container, false, new[] { typeof(string) }),
                        _factory.Object,
                        new IExtension[0]);

                using (container.Register(registryContext))
                {
                    var resolverContext = container.CreateContext(CreateCompositeKey(container, true, new[] { typeof(string) }));

                    // Then
                    Should.Throw<Exception>(() => container.Resolve(resolverContext), "test");
                }
            }
        }

        private static ICompositeKey CreateCompositeKey(IContainer container, bool toResolve, Type[] genericTypes, object[] tags = null)
        {
            var keyFactory = container.GetKeyFactory();
            var genericKeys = (genericTypes ?? new Type[0]).Select(i => keyFactory.CreateContractKey(i, toResolve)).ToArray();
            var tagKeys = (tags ?? new object[0]).Select(i => keyFactory.CreateTagKey(i)).ToArray();
            var stateKeys = new IStateKey[0];
            return keyFactory.CreateCompositeKey(
                genericKeys,
                tagKeys,
                stateKeys);
        }

        private static IEnumerable<ICompositeKey> CreateCompositeKeys(IContainer container, bool toResolve, Type[] genericTypes, object[] tags = null)
        {
            yield return CreateCompositeKey(container, toResolve, genericTypes, tags);
        }

        private Container CreateContainer()
        {
            return new Container();
        }
    }
}