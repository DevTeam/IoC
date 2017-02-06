[Build](http://tcavs2015.cloudapp.net/) status: <img src="http://tcavs2015.cloudapp.net/app/rest/builds/buildType:(id:DevTeam_IoC_Build)/statusIcon"/>

There are many different implementations of [Inversion of Control](https://github.com/DevTeam/IoC/wiki/Inversion-of-Control).

Why it would be preferable to use this implementation?

Because it has many outstanding [features](https://github.com/DevTeam/IoC/wiki/Features), just look at [these samples](https://github.com/DevTeam/IoC/wiki/Samples) and use these [<img src="https://www.nuget.org/Content/Logos/nugetlogo.png" height="18"> packages](https://github.com/DevTeam/IoC/wiki/NuGet-packages) to make your code more efficient. See [Wiki](https://github.com/DevTeam/IoC/wiki) for details.

Here is just one simplest example [_Hellow World_](https://github.com/DevTeam/IoC/tree/master/Samples/HelloWorld):

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

The **Json configuration** jsonConfigStr:
```json
[
    { "reference": "ClassLibrary" },

    { "using": "ClassLibrary" },

    { "Register": [ { "contract": [ "IConsole" ] } ], "AsAutowiring": "Console" },
    { "Register": [ { "contract": [ "IHelloWorld" ] } ], "AsAutowiring": "HelloWorld" }
]
```

**Contracts**:
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

**Implementations**:
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
