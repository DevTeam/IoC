namespace DevTeam.IoC.Contracts.Dto
{
    [PublicAPI]
    public interface IValueDto
    {
        string Data { get; }

        string TypeName { get; }
    }
}
