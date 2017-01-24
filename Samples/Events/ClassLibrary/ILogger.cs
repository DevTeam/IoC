namespace ClassLibrary
{
    internal interface ILogger
    {
        void LogInfo<T>(IName<T> name, string info);
    }
}
