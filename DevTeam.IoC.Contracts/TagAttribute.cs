namespace DevTeam.IoC.Contracts
{
    using System;

    [PublicAPI]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Class, AllowMultiple = true)]
    public class TagAttribute : Attribute
    {
        public TagAttribute([NotNull] params object[] tags)
        {
            if (tags == null) throw new ArgumentNullException(nameof(tags));
            if (tags.Length == 0) throw new ArgumentException("Value cannot be an empty collection.", nameof(tags));
            Tags = tags;
        }

        public object[] Tags { [NotNull] get; }
    }
}
