namespace ClassLibrary
{
    internal interface ILogger<T>
    {
        string InstanceName { get; }

        void LogInfo(string info);
    }
}
