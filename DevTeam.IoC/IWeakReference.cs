namespace DevTeam.IoC
{
    internal interface IWeakReference<T>
        where T : class
    {
        bool TryGetTarget(out T target);
    }
}