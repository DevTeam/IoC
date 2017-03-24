namespace DevTeam.IoC
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Contracts;

    internal sealed class ExpressionMethodFactory : IMethodFactory
    {
        public Constructor CreateConstructor(ConstructorInfo constructor)
        {
#if DEBUG
            if (constructor == null) throw new ArgumentNullException(nameof(constructor));
#endif
            var argsParameter = Expression.Parameter(typeof(object[]), "args");
            var paramsExpression = CreateParameterExpressions(constructor, argsParameter);
            var create = Expression.New(constructor, paramsExpression);
            var lambda = Expression.Lambda<Constructor>(
                create,
                argsParameter);
            return lambda.Compile();
        }

        public Method CreateMethod(Type instanceType, MethodInfo method)
        {
#if DEBUG
            if (instanceType == null) throw new ArgumentNullException(nameof(instanceType));
            if (method == null) throw new ArgumentNullException(nameof(method));
#endif
            var instanceParameter = Expression.Parameter(typeof(object), "instance");
            var argsParameter = Expression.Parameter(typeof(object[]), "args");
            var paramsExpression = CreateParameterExpressions(method, argsParameter);
            var call = Expression.Call(Expression.Convert(instanceParameter, instanceType), method, paramsExpression);
            var lambda = Expression.Lambda<Method>(
                call,
                instanceParameter,
                argsParameter);
            return lambda.Compile();
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private static Expression[] CreateParameterExpressions(MethodBase method, Expression argumentsParameter)
        {
            return method.GetParameters().Select(
                (parameter, index) => (Expression)Expression.Convert(
                    Expression.ArrayIndex(
                        argumentsParameter,
                        Expression.Constant(index)),
                    parameter.ParameterType)).ToArray();
        }
    }
}
