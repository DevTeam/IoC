namespace DevTeam.IoC.Contracts.Dto
{
    using Contracts;

    [PublicAPI]
    public interface IPropertyDto
    {
        string Name { [NotNull] get; }

        IParameterDto Property { [CanBeNull] get; }
    }
}