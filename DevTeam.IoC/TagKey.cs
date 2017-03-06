namespace DevTeam.IoC
{
    using System;
    using System.Linq;
    using Contracts;

    internal struct TagKey : ITagKey
    {
        private readonly int _hashCode;

        public TagKey([NotNull] object tag)
        {
#if DEBUG
            if (tag == null) throw new ArgumentNullException(nameof(tag));
#endif
            Tag = tag;
            _hashCode = Tag.GetHashCode();
        }

        public object Tag { get; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            var key = obj as ITagKey ?? (obj as ICompositeKey)?.TagKeys.SingleOrDefault();
            return key != null && Equals(key);
        }

        public override int GetHashCode()
        {
            if (KeyFilterContext.Current.Filter(typeof(ITagKey)))
            {
                return 0;
            }

            return _hashCode;
        }

        private bool Equals(ITagKey other)
        {
            return KeyFilterContext.Current.Filter(typeof(ITagKey)) || Tag.Equals(other.Tag);
        }

        public override string ToString()
        {
            return $"{nameof(TagKey)} [Tag: {Tag}]";
        }
    }
}