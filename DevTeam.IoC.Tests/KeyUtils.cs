namespace DevTeam.IoC.Tests
{
    using System;
    using System.Linq;
    using Contracts;

    internal static class KeyUtils
    {
        public static ICompositeKey CreateCompositeKey(IContainer container, bool toResolve, Type[] genericTypes, object[] tags)
        {
            var keyFactory = container.GetKeyFactory();
            var genericKeys = genericTypes.Select(i => keyFactory.CreateContractKey(i, toResolve)).ToArray();
            var tagKeys = tags.Select(i => keyFactory.CreateTagKey(i)).ToArray();
            var stateKeys = new IStateKey[0];
            return keyFactory.CreateCompositeKey(
                genericKeys,
                tagKeys,
                stateKeys);
        }
    }
}
