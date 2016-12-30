namespace DevTeam.IoC.Contracts
{
    using System.Reflection;

    public interface IInstanceFactoryProvider
    {
        IInstanceFactory GetFactory(ConstructorInfo constructor);
    }
}