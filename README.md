According to Wikipedia _"Inversion of Control, or IoC, is an abstract principle describing an aspect of some software architecture designs in which the flow of control of a system is inverted in comparison to procedural programming."_ Let's consider the following example:

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
            if (line == null) throw new ArgumentNullException(nameof(line));
            System.Console.WriteLine(line);
        }
    }

    // Has the only one dependency implementing type "IConsole"
    internal class HelloWorld : IHelloWorld
    {
        private readonly IConsole _console;

        public HelloWorld(IConsole console)
        {
            if (console == null) throw new ArgumentNullException(nameof(console));
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
