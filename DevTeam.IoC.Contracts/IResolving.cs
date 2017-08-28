namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IResolving<T> :
        IToken<IResolving<T>>,
        IInstanceResolving,
        ITryResolving
#if !NET35
        ,IAsyncResolving
#endif
          where T : IResolver
    {
    }
}