namespace DevTeam.IoC.Contracts
{
    public interface ICache<in TKey, TValue>
    {
        bool TryGet([NotNull] TKey key, out TValue value);

        void Set([NotNull] TKey key, [NotNull] TValue value);

        bool TryRemove([NotNull] TKey key);
    }
}
