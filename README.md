[<img src="http://tcavs2015.cloudapp.net/app/rest/builds/buildType:(id:DevTeam_IoC_Build)/statusIcon"/>](http://tcavs2015.cloudapp.net/viewType.html?buildTypeId=DevTeam_IoC_Build) [<img src="https://www.nuget.org/Content/Logos/nugetlogo.png" height="18">](https://github.com/DevTeam/IoC/wiki/NuGet-packages)

There are many different implementations of [Inversion of Control](https://github.com/DevTeam/IoC/wiki/Inversion-of-Control). Why it would be preferable to use this implementation? It is fast and has many outstanding [features](https://github.com/DevTeam/IoC/wiki/Features) to make your code more efficient. See [Wiki](https://github.com/DevTeam/IoC/wiki) for details.

[Comparison test](https://github.com/DevTeam/IoC/tree/master/DevTeam.IoC.Tests/ComparisonTests.cs) with IoC containers in the synthetic test - creating a graph from 2 transient and singleton objects in the serie of 100k iterations:

- This    110 ms
- [Unity](https://www.nuget.org/packages/Unity/)    245 ms
- [Ninject](https://www.nuget.org/packages/Ninject/)    1412 ms

Here is the example [Shroedingers Cat](https://github.com/DevTeam/IoC/tree/master/Samples/ShroedingersCat):

```csharp
public class Program
{
    public static void Main()
    {
        using (var container = new Container().Configure().DependsOn<Glue>().ToSelf())
        {
            var box = container.Resolve().Instance<IBox<ICat>>();
            Console.WriteLine(box.Content.IsAlive);
        }
    }
}
```

**Abstractions**:
```csharp
interface IBox<T> { T Content { get; } }

interface ICat { bool IsAlive { get; } }
```

**Implementations**:

![Cat](https://github.com/DevTeam/IoC/blob/master/Samples/ShroedingersCat/cat.jpg)

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
            .Contract(typeof(IBox<>)).Autowiring(typeof(CardboardBox<>))
            .And().Contract<ICat>().Autowiring<ShroedingersCat>()
            .Apply();
    }
}
```

Or another way to glue via [Json file](https://github.com/DevTeam/IoC/blob/master/Samples/ShroedingersCat/ConsoleApp/configuration.json):
```json
[
  { "register": [ { "contract": [ "IBox<>" ] } ], "autowiring": "CardboardBox<>" },
  { "register": [ { "contract": [ "ICat" ] } ], "autowiring": "ShroedingersCat" }
]
```
