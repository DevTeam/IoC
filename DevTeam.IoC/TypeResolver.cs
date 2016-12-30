namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using Contracts;

    internal class TypeResolver : ITypeResolver
    {
        private static readonly Dictionary<string, Type> PrimitiveTypes = new Dictionary<string, Type>
        {
            {"byte", typeof(byte)},
            {"sbyte", typeof(sbyte)},
            {"int", typeof(int)},
            {"uint", typeof(uint)},
            {"short", typeof(short)},
            {"ushort", typeof(ushort)},
            {"long", typeof(long)},
            {"ulong", typeof(ulong)},
            {"float", typeof(float)},
            {"double", typeof(double)},
            {"char", typeof(char)},
            {"object", typeof(object)},
            {"string", typeof(string)},
            {"decimal", typeof(decimal)}
        };

        private static readonly Regex GenericTypeRegex = new Regex(@"(.+)<(.*)>", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private readonly List<string> _usings = new List<string>();
        private readonly List<Assembly> _references = new List<Assembly>();

        public void AddReference(string reference)
        {
            if (reference == null) throw new ArgumentNullException(nameof(reference));
            _references.Add(Assembly.Load(new AssemblyName(reference)));
        }

        public void AddUsing(string usingName)
        {
            if (usingName == null) throw new ArgumentNullException(nameof(usingName));
            _usings.Add(usingName);
        }

        public bool TryResolveType(string typeName, out Type type)
        {
            if (typeName == null)
            {
                type = default(Type);
                return false;
            }

            if (PrimitiveTypes.TryGetValue(typeName, out type))
            {
                return true;
            }

            if (TryFindType(typeName, out type))
            {
                return true;
            }

            string canonicalFullTypeName;
            if (TryGetCanonicalTypeName(typeName, true, out canonicalFullTypeName))
            {
                if (TryFindType(canonicalFullTypeName, out type))
                {
                    return true;
                }
            }

            string canonicalTypeName;
            if (TryGetCanonicalTypeName(typeName, false, out canonicalTypeName))
            {
                if (TryFindType(canonicalTypeName, out type))
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryFindType(string typeName, out Type type)
        {
            type = Type.GetType(typeName, false);
            if (type != null)
            {
                return true;
            }

            foreach (var usingName in _usings)
            {
                var fullTypeName = $"{usingName}.{typeName}";
                type = Type.GetType(fullTypeName, false);
                if (type != null)
                {
                    return true;
                }
            }

            foreach (var reference in _references)
            {
                if (TryGetInternalType(reference, typeName, out type))
                {
                    return true;
                }
            }

            foreach (var usingName in _usings)
            {
                var fullTypeName = $"{usingName}.{typeName}";
                foreach (var reference in _references)
                {
                    if (TryGetInternalType(reference, fullTypeName, out type))
                    {
                        return true;
                    }
                }
            }

            type = default(Type);
            return false;
        }

        private static bool TryGetInternalType(Assembly reference, string fullTypeName, out Type type)
        {
            type = reference.DefinedTypes.Where(i => i.FullName == fullTypeName).Select(i => i.AsType()).SingleOrDefault();
            return type != null;
        }

        private bool TryGetCanonicalTypeName(string typeName, bool definedType, out string canonicalTypeName)
        {
            var genericTypeMatch = GenericTypeRegex.Match(typeName);
            if (genericTypeMatch.Success)
            {
                var genericTypeArgsStr = genericTypeMatch.Groups[2].Value;
                var genericTypeArgs = genericTypeArgsStr.Split(',');
                var genericTypeCanonicalArgsSb = new StringBuilder("[");
                var cnt = 0;
                var typesArePresented = false;
                foreach (var genericTypeArg in genericTypeArgs)
                {
                    if (cnt > 0)
                    {
                        genericTypeCanonicalArgsSb.Append(',');
                    }

                    cnt++;
                    if (String.IsNullOrWhiteSpace(genericTypeArg))
                    {
                        continue;
                    }

                    Type genericType;
                    if (!TryResolveType(genericTypeArg, out genericType))
                    {
                        canonicalTypeName = default(string);
                        return false;
                    }

                    genericTypeCanonicalArgsSb.Append(genericType.FullName);
                    typesArePresented = true;
                }

                genericTypeCanonicalArgsSb.Append(']');
                var genericTypeCanonicalArgsStr = definedType && typesArePresented ? genericTypeCanonicalArgsSb.ToString() : String.Empty;
                canonicalTypeName = $"{genericTypeMatch.Groups[1].Value}`{cnt}{genericTypeCanonicalArgsStr}";
                return true;
            }

            canonicalTypeName = typeName;
            return true;
        }
    }
}