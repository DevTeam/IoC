[<img src="http://tcavs2015.cloudapp.net/app/rest/builds/buildType:(id:DevTeam_IoC_Build)/statusIcon"/>](http://tcavs2015.cloudapp.net/viewType.html?buildTypeId=DevTeam_IoC_Build&guest=1) [<img src="https://www.nuget.org/Content/Logos/nugetlogo.png" height="18">](https://github.com/DevTeam/IoC/wiki/NuGet-packages)

# Simple, powerful and fast Inversion of Control container for .NET

There are many different implementations of [Inversion of Control](https://github.com/DevTeam/IoC/wiki/Inversion-of-Control). Why it would be preferable to use this implementation? It has many outstanding [features](https://github.com/DevTeam/IoC/wiki/Features) to make your code more efficient. See [Wiki](https://github.com/DevTeam/IoC/wiki) for details.

Supported platforms:
  - .NET 3.5+
  - .NET Core 1.0+
  - .NET Standard 1.0+

![Comparison test](https://github.com/DevTeam/IoC/blob/master/Docs/Images/speed.png) [Comparison test](https://github.com/DevTeam/IoC/blob/master/DevTeam.IoC.Tests/Integration/ComparisonTests.cs) of some IoC containers in the synthetic test (creating a graph from 2 transient and singleton objects in the serie of 100k iterations) has the following [result](http://tcavs2015.cloudapp.net/httpAuth/app/rest/builds/buildType:DevTeam_IoC_Build,status:SUCCESS/artifacts/content/reports/Comparison.zip).

[Shroedingers Cat](https://github.com/DevTeam/IoC/tree/master/Samples/ShroedingersCat) shows how it works:

```csharp
using (var container = new Container().Configure().DependsOn<Glue>().ToSelf())
{
    var box = container.Resolve().Instance<IBox<ICat>>();
    Console.WriteLine(box.Content.IsAlive);
}
```

**Abstraction**:
```csharp
interface IBox<T> { T Content { get; } }

interface ICat { bool IsAlive { get; } }
```

**Implementation**:

![Cat](https://github.com/DevTeam/IoC/blob/master/Docs/Images/cat.jpg)

```csharp
class CardboardBox<T> : IBox<T>
{
    public CardboardBox(T content) { Content = content; }

    public T Content { get; }
}

class ShroedingersCat : ICat
{
    public bool IsAlive => true; // for humanistic reasons
}
```

Let's glue it together:
```csharp
class Glue: IConfiguration
{
    public IEnumerable<IConfiguration> GetDependencies(IContainer container)
    {
        yield break;
    }

    public IEnumerable<IDisposable> Apply(IContainer container)
    {
        yield return container.Register()
            .Autowiring(typeof(IBox<>), typeof(CardboardBox<>))
            .And().Autowiring<ICat, ShroedingersCat>();
    }
}
```

And the another way is to glue via [Json file](https://github.com/DevTeam/IoC/blob/master/Samples/ShroedingersCat/ConsoleApp/configuration.json):
```json
[
  { "reference": "ConsoleApp" },
  { "using": "ConsoleApp" },

  { "class": "CardboardBox<> : IBox<>" },
  { "class": "ShroedingersCat : ICat" }
]
```
