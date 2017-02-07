namespace DevTeam.IoC.Contracts.Dto
{
    [PublicAPI]
    public interface IStateDto : IRegisterStatementDto
    {
        int Index { get; }

        string StateTypeName { get; }

        IValueDto Value { get; }
    }
}