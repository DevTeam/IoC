namespace DevTeam.IoC.Contracts
{
    using System.Reflection;

    [PublicAPI]
    public interface IInstanceFactoryProvider
    {
        [NotNull]
        IInstanceFactory GetFactory([NotNull] ConstructorInfo constructor);
    }
}