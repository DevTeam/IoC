namespace DevTeam.IoC.Contracts.Dto
{
    [PublicAPI]
    public interface ITagKeyDto
    {
        string TagValue { get; }

        string TagTypeName { get; }
    }
}
