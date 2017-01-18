namespace DevTeam.IoC.Contracts.Dto
{
    public interface ITagDto : IKeyDto
    {
        string Value { get; }

        string TypeName { get; }
    }
}