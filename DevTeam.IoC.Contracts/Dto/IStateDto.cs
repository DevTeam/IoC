namespace DevTeam.IoC.Contracts.Dto
{
    public interface IStateDto : IKeyDto
    {
        int Index { get; }

        string TypeName { get; }
    }
}