namespace ClassLibrary
{
    // ReSharper disable once UnusedTypeParameter
    internal interface ILogger<T>
    {
        string InstanceName { get; }

        void LogInfo(string info);
    }
}
