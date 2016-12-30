namespace DevTeam.IoC.Contracts.Dto
{
    public interface IStateDto : IRegisterStatementDto
    {
        int Index { get; }

        string TypeName { get; }
    }
}