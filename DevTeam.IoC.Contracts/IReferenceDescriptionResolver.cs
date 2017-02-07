namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IReferenceDescriptionResolver
    {
        [NotNull]
        string ResolveReference([NotNull] string reference);
    }
}
