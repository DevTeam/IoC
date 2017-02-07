namespace DevTeam.IoC.Contracts.Dto
{
    [PublicAPI]
    public interface IStateDto : IRegisterStatementDto
    {
        int Index { get; }

        string StateTypeName { [NotNull] get; }

        IValueDto Value { [CanBeNull] get; }
    }
}