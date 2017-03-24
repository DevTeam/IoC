namespace DevTeam.IoC
{
    using System;
    using System.Linq;
    using Contracts;

    internal struct TagKey : ITagKey
    {
        private readonly int _hashCode;
        private readonly object _tag;

        public TagKey([NotNull] object tag)
        {
#if DEBUG
            if (tag == null) throw new ArgumentNullException(nameof(tag));
#endif
            _tag = tag;
            _hashCode = tag.GetHashCode();
        }

        public object Tag => _tag;

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            var key = obj as ITagKey ?? (obj as ICompositeKey)?.TagKeys.SingleOrDefault();
            return key != null && Equals(key);
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public override int GetHashCode()
        {
            if (KeyFilterContext.Current.Filter(typeof(ITagKey)))
            {
                return 0;
            }

            return _hashCode;
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private bool Equals(ITagKey other)
        {
            return KeyFilterContext.Current.Filter(typeof(ITagKey)) || _tag.Equals(other.Tag);
        }

        public override string ToString()
        {
            return $"{nameof(TagKey)} [Tag: {_tag}]";
        }
    }
}