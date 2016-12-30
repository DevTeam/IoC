namespace DevTeam.IoC.Contracts
{
    using System;

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
    public class TagAttribute : Attribute
    {
        public TagAttribute(params object[] tags)
        {
            if (tags == null) throw new ArgumentNullException(nameof(tags));
            if (tags.Length == 0) throw new ArgumentException("Value cannot be an empty collection.", nameof(tags));
            Tags = tags;
        }

        public object[] Tags { get; }
    }
}
