#if !NET35
namespace DevTeam.IoC.Contracts
{
    using System.Threading;
    using System.Threading.Tasks;

    [PublicAPI]
    public interface IAsyncResolving
    {
        [NotNull]
        Task AsyncInstance(CancellationToken cancellationToken, [NotNull] [ItemCanBeNull] params object[] state);

        [NotNull]
        Task AsyncInstance(CancellationToken cancellationToken, [NotNull] IStateProvider stateProvider);

        [NotNull]
        Task<TContract> AsyncInstance<TContract>(CancellationToken cancellationToken, [NotNull] [ItemCanBeNull] params object[] state);

        [NotNull]
        Task<TContract> AsyncInstance<TContract>(CancellationToken cancellationToken, [NotNull] IStateProvider stateProvider);
    }
}
#endif