namespace DevTeam.IoC
{
    using Contracts;

    internal interface IConverter<in TSrc, TDst, in TContext>
        where TContext: class
    {
        bool TryConvert([CanBeNull] TSrc src, out TDst value, [NotNull] TContext context);
    }
}
