[Build](http://tcavs2015.cloudapp.net/) status: <img src="http://tcavs2015.cloudapp.net/app/rest/builds/buildType:(id:DevTeam_IoC_Build)/statusIcon"/>

There are many different implementations of [Inversion of Control](https://github.com/DevTeam/IoC/wiki/Inversion-of-Control).

Why it would be preferable to use this implementation?

Because it has many outstanding [features](https://github.com/DevTeam/IoC/wiki/Features), just look at [these samples](https://github.com/DevTeam/IoC/wiki/Samples) and use these [<img src="https://www.nuget.org/Content/Logos/nugetlogo.png" height="18"> packages](https://github.com/DevTeam/IoC/wiki/NuGet-packages) to make your code more efficient. See [Wiki](https://github.com/DevTeam/IoC/wiki) for details.

Here is just one simplest example [_Hellow World Simplest_](https://github.com/DevTeam/IoC/tree/master/Samples/HelloWorldSimplest):

**The entry point**:

```csharp
    [Contract(typeof(Program))]
    public class Program
    {
        public static void Main()
        {
            var iocJson = File.ReadAllText("IoC.json");

            // Create the root container and apply the configuration from the json string
            using (var container = new Container())
            using (container.Configure().DependsOn<JsonConfiguration>(iocJson).Apply())
            {
                container.Resolve().Instance<Program>();
            }
        }

        public Program(IHelloWorld helloWorld)
        {
            helloWorld.SayHello();
        }
    }
```

[_IoC.json_file](https://github.com/DevTeam/IoC/tree/master/Samples/HelloWorldSimplest/ConsoleApp/IoC.json):
```json
[
    { "reference": "ClassLibrary" },
    { "reference": "ConsoleApp" },
    { "using": "ClassLibrary" },
    { "using": "ConsoleApp" },

    { "autowiring": "Console" },
    { "autowiring": "HelloWorld" },
    { "autowiring": "Program" }
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
    [Contract(typeof(IConsole))]
    internal class Console : IConsole
    {
        public void WriteLine(string line)
        {
            System.Console.WriteLine(line);
        }
    }

    // Has the only one dependency implementing interface "IConsole"
    [Contract(typeof(IHelloWorld))]
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
