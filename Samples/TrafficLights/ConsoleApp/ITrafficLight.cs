namespace ConsoleApp
{
    internal interface ITrafficLight
    {
        string Description { get; }

        void ChangeState();
    }
}