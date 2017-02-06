namespace DevTeam.IoC.Contracts.Dto
{
    public interface IStateDto : IRegisterStatementDto
    {
        int Index { get; }

        string StateTypeName { get; }

        IValueDto Value { get; }
    }
}