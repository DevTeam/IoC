namespace Contracts
{
    // ReSharper disable once UnusedTypeParameter
    public interface ILogger<T>
    {
        string InstanceName { get; }

        void LogInfo(string info);
    }
}
