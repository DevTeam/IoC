namespace DevTeam.IoC.Contracts.Dto
{
    public interface IStateDto : IKeyDto
    {
        int Index { get; }

        string StateTypeName { get; }

        IValueDto Value { get; }
    }
}