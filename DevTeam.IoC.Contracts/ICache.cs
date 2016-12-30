namespace DevTeam.IoC.Contracts
{
    public interface ICache<in TKey, TValue>
    {
        bool TryGet(TKey key, out TValue value);

        void Set(TKey key, TValue value);

        bool TryRemove(TKey key);
    }
}
