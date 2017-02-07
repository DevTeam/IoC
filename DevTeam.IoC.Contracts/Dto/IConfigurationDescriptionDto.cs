namespace DevTeam.IoC.Contracts.Dto
{
    [PublicAPI]
    public interface IConfigurationDescriptionDto
    {
        string Description { [NotNull] get; }
    }
}
