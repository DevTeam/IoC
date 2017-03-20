namespace DevTeam.IoC.Contracts
{
    using System;
    using System.Reflection;

    [PublicAPI]
    public interface IMethodFactory
    {
        [NotNull] Constructor CreateConstructor([NotNull] ConstructorInfo constructor);

        [NotNull] Method CreateMethod([NotNull] Type instanceType, [NotNull] MethodInfo method);
    }
}