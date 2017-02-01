There are many different implementations of the _Inversion of Control_ (**_IoC_**) pattern. Why you could choose precisely this? It has a lot of outstanding features:

* It is not necessary to add any references to IoC libraries, thus nearly any code could work according to _IoC_ pattern without any additional changes. For example, [this sample project](https://github.com/DevTeam/IoC/tree/master/Samples/HelloWorld/ClassLibrary) does not know about _IoC_ at all.

* It provides an ability to use state during resolving. For example [class](https://github.com/DevTeam/IoC/blob/master/Samples/Events/ClassLibrary/Event.cs), has one dependency _"logger"_ and state _"data"_ that are injected via constructor.

```csharp
class Event<T>
{
        Event(
            ILogger<Event<T>> logger,
            T data)
        { ... }
}
```

* There is a kit of pluggable [features](https://github.com/DevTeam/IoC/blob/master/DevTeam.IoC.Contracts/Wellknown.cs). 
**_Full_** - supports full set of features.
**_ChildrenContainers_** - supports child resolving chuild containers.
**_Lifetimes_** - supports lifetimes.
**_Scopes_** - supports scopes.
**_KeyComaprers_** - supports key comparers.
**_Enumerables_** - supports resolving via IEnumerable<>.
**_Tasks_** - supports injection via Task.
**_Resolvers_** - supports resolving via IResolver<>, IProvider<>, Func<>.
**_Cache_** - uses cache to optimize a performance.
And others.
             
For example, when feature **_"Cache"_** is enabled, _IoC_ containers works faster? but consumes more memory. Or another one feature **_"Tasks"_** allows injection of [tasks](https://msdn.microsoft.com/en-us/library/dd537609(v=vs.110).aspx), [for example](https://github.com/DevTeam/IoC/blob/master/DevTeam.IoC.Tests.Models/EventRegistry.cs):

```csharp
class EventRegistry : IEventRegistry, IDisposable
{
    EventRegistry(
        Task<IResolver> resolver,
        Task<IEventBroker> eventBroker,
        ...)
        {
            logResolver.Start();
            resolver.Start();
        }
}
```

# Inversion of Control

According to [Wikipedia](https://en.wikipedia.org/wiki/Inversion_of_control) _"Inversion of Control, or IoC, is an abstract principle describing an aspect of some software architecture designs in which the flow of control of a system is inverted in comparison to procedural programming."_

Inversion of control serves the following design purposes:
* To decouple the execution of a task from implementation.
* To focus a module on the task it is designed for.
* To free modules from assumptions about how other systems do what they do and instead rely on contracts.
* To prevent side effects when replacing a module.

Let's consider the example, when a class, illustrated as **_HelloWorld_** below, uses another class **_Console_**:

```csharp
namespace ClassLibrary
{
    using System;

    class Console
    {
        public void WriteLine(string line)
        {
            System.Console.WriteLine(line);
        }
    }

    class HelloWorld
    {
        ClassLibrary.Console _console;

        public HelloWorld()
        {
            _console = new ClassLibrary.Console();
        }

        public void SayHello()
        {
            _console.WriteLine("Hello");
        }
    }
}
```

The consumer **_HelloWorld_** needs the consumed class **_Console_** to write text _"Hello"_ to the console. That’s all good and natural, but does **_HelloWorld_** really need to know that it uses **_Console_**?

Isn’t it enough that **_HelloWorld_** knows that it uses something that has the behavior, the methods, properties etc, of **_Console_** without knowing who actually implements the behavior?

By extracting an abstract definition of the behavior used by **_HelloWorld_** in **_Console_**, represented as **_IConsole_** below, and letting the consumer **_HelloWorld_** use an instance of that instead of **_Console_** it can continue to do what it does without having to know the specifics about **_Console_**.

In the next example **_Console_** implements **_IConsole_** and **_HelloWorld_** uses an instance of **_IConsole_**. While it’s quite possible that **_HelloWorld_** still uses **_Console_** what’s interesting is that **_HelloWorld_** doesn’t know that. It just knows that it uses something that implements **_IConsole_**.

That could be **_Console_**, but it could also be, for example, **_RemoteConsole_**, **_TraceConsole_** or something given that they implement **_IConsole_**. Of course this discussion is rather abstract at the moment, but we’ll get to how to implement this in a second using either Dependency Injection or Service Locator.

For now, let’s just assume that we can and that we can change what implementation of **_IConsole_** **_Console_** uses at runtime. What benefits can we get from that?

# Benefits of Inversion of Control

Well, first of all **_HelloWorld_** is not dependent on Y anymore and it’s therefore less likely that we’ll have to make changes to X when we make changes in Y as we’re less likely to write code in X that relies on implementation details in Y.

Furthermore **_HelloWorld_** is now much more flexible as we can change which implementation of I it uses without changing anything in **_HelloWorld_**.

Perhaps **_Console_** is a component for writing a text to the console and we want **_HelloWorld_**, who used to write a text, to start writing a text to a trace console instead using **_TraceConsole_** who also implements  **_IConsole_**. Since **_HelloWorld_** no longer relies specifically on **_TraceConsole_** but on something that implements  **_IConsole_** we don’t have to modify anything in **_HelloWorld_**.

Another benefit of inversion of control is also that we can isolate our code when creating unit tests.

Suppose, as before, that **_HelloWorld_** used  **_Console_** to write texts which  **_Console_**. When creating unit test, which should only test a single unit of code, we want to be able to test the logic in **_HelloWorld_** without having to care about the component that write a text to the console.

Having used inversion of control **_HelloWorld_** doesn’t rely on **_Console_** anymore but on an implementation of **_IConsole_** that just happens to be **_Console_**.

That means that we, in the setup phase of our tests, can change it so that **_HelloWorld_** uses a different implementation of **_IConsole_**, such as a mock object which doesn’t send any messages at all, and which also allows us to check that **_HelloWorld_** has used it in a correct way.

# Applying _Inversion of Control_ using the _Dependency Injection_ pattern via a constructor injection

Enough with the **_HelloWorld_** and the **_Console_** and **_IConsole_**! Let’s look at the following example:

# [Hellow World](https://github.com/DevTeam/IoC/tree/master/Samples/HelloWorld)

**The entry point**:

```csharp
// Create the root container
using (var container = new Container())
// Appply the configuration from the json string
using (container.Configure().DependsOn<JsonConfiguration>(jsonConfigStr).Apply())
{
  // Resolve an instance implementing the interface "IHelloWorld"
  var helloWorld = container.Resolve().Instance<IHelloWorld>();
  
  // Run method to say "Hello"
  helloWorld.SayHello();
}
```

The **Json configuration** string:
```json
[
    // "Add reference to assembly ClassLibrary"
    { "reference": "ClassLibrary" },

    // "Add namespace ClassLibrary to resolve types"
    { "using": "ClassLibrary" },

    // "Register implementations"
    { "Register": [ { "contract": [ "IConsole" ] } ], "AsAutowiring": "Console" },
    { "Register": [ { "contract": [ "IHelloWorld" ] } ], "AsAutowiring": "HelloWorld" }
]
```

Code for **Contracts**:
```csharp
namespace ClassLibrary
{
    internal interface IConsole
    {
        void WriteLine(string line);
    }

    public interface IHelloWorld
    {
        void SayHello();
    }
}
```

Code for **Implementation**:
```csharp
namespace ClassLibrary
{
    using System;

    // Has no any dependencies
    internal class Console : IConsole
    {
        public void WriteLine(string line)
        {
            System.Console.WriteLine(line);
        }
    }

    // Has the only one dependency implementing interface "IConsole"
    internal class HelloWorld : IHelloWorld
    {
        private readonly IConsole _console;

        public HelloWorld(IConsole console)
        {
            _console = console;
        }

        public void SayHello()
        {
            _console.WriteLine("Hello");
        }
    }
}
```

For details see [wiki page](https://github.com/DevTeam/IoC/wiki)

# Samples

* [Hellow World](https://github.com/DevTeam/IoC/tree/master/Samples/HelloWorld)
* [Events](https://github.com/DevTeam/IoC/tree/master/Samples/Events)

# <img src="https://www.nuget.org/Content/Logos/nugetlogo.png" height="18"> packages

* [IoC Contracts](https://www.nuget.org/packages/DevTeam.IoC.Contracts/)
* [IoC](https://www.nuget.org/packages/DevTeam.IoC/)
* [IoC Json configuration](https://www.nuget.org/packages/DevTeam.IoC.Configurations.Json/)
