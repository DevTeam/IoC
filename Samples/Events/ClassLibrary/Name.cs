namespace ClassLibrary
{
    internal class Name<T> : IName<T>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static int _curId;
        private readonly int _id;

        public Name()
        {
            _id = System.Threading.Interlocked.Increment(ref _curId);
        }

        public string Short => $"{typeof(T).Name}_{_id}";
    }
}