namespace DevTeam.IoC.Contracts
{
    [PublicAPI]
    public interface IReferenceDescriptionResolver
    {
        string ResolveReference(string reference);
    }
}
