namespace DevTeam.IoC
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Contracts;

    internal class ExpressionInstanceFactoryProvider : IInstanceFactoryProvider
    {
        public IInstanceFactory GetFactory(ConstructorInfo constructor)
        {
#if DEBUG
            if (constructor == null) throw new ArgumentNullException(nameof(constructor));
#endif
            var args = Expression.Parameter(typeof(object[]), "args");
            var parameters = constructor.GetParameters().Select((parameter, index) => Expression.Convert(Expression.ArrayIndex(args, Expression.Constant(index)), parameter.ParameterType));
            return new InstanceFactory(Expression.Lambda<InstanceFactoryMethod>(Expression.New(constructor, parameters), args).Compile());
        }
    }

}
