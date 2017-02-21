[<img src="http://tcavs2015.cloudapp.net/app/rest/builds/buildType:(id:DevTeam_IoC_Build)/statusIcon"/>](http://tcavs2015.cloudapp.net/viewType.html?buildTypeId=DevTeam_IoC_Build) [<img src="https://www.nuget.org/Content/Logos/nugetlogo.png" height="18">](https://github.com/DevTeam/IoC/wiki/NuGet-packages)

There are many different implementations of [Inversion of Control](https://github.com/DevTeam/IoC/wiki/Inversion-of-Control).

Why it would be preferable to use this implementation? It has many outstanding [features](https://github.com/DevTeam/IoC/wiki/Features), just look at [these samples](https://github.com/DevTeam/IoC/wiki/Samples) and use these [<img src="https://www.nuget.org/Content/Logos/nugetlogo.png" height="18"> packages](https://github.com/DevTeam/IoC/wiki/NuGet-packages) to make your code more efficient. See [Wiki](https://github.com/DevTeam/IoC/wiki) for details.

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
    public bool IsAlive => true; // for humanitarian reasons
}
```

Let's glue them together:
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
