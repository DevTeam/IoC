namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Contracts;
    using static System.String;

    internal class TypeResolver : ITypeResolver
    {
        // ReSharper disable StringLiteralTypo
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
        // ReSharper restore StringLiteralTypo

        private readonly List<string> _usingStatements = new List<string>();
        private readonly List<Assembly> _references = new List<Assembly>();

        private IEnumerable<Assembly> References => _references;

        private IEnumerable<string> UsingStatements => _usingStatements;

        public void AddReference(string reference)
        {
            if (reference == null) throw new ArgumentNullException(nameof(reference));
            if (reference.IsNullOrWhiteSpace()) throw new ArgumentException("Value cannot be null or whitespace.", nameof(reference));
            _references.Add(Assembly.Load(new AssemblyName(reference)));
        }

        public void AddUsingStatement(string usingName)
        {
            if (usingName == null) throw new ArgumentNullException(nameof(usingName));
            if (usingName.IsNullOrWhiteSpace()) throw new ArgumentException("Value cannot be null or whitespace.", nameof(usingName));
            _usingStatements.Add(usingName);
        }

        public bool TryResolveType(string typeName, out Type type)
        {
            if (typeName.IsNullOrWhiteSpace())
            {
                type = default(Type);
                return false;
            }

            if (TryResolveSimpleType(typeName, out type))
            {
                return true;
            }

            var typeDescription = new TypeDescription(typeName, this);
            if (!typeDescription.IsValid)
            {
                return false;
            }

            type = typeDescription.Type;
            return true;
        }

        private bool TryResolveSimpleType(string typeName, out Type type)
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

            if (typeName.Contains("<") || typeName.Contains(">"))
            {
                return false;
            }

            type = Type.GetType(typeName, false);
            if (type != null)
            {
                return true;
            }

            return false;
        }

        private class TypeDescription
        {
            public TypeDescription(string typeName, TypeResolver typeResolver)
            {
                GenericTypeArgs = new List<TypeDescription>();

                if (typeName == null)
                {
                    return;
                }

                typeName = typeName.Trim();
                if (IsNullOrEmpty(typeName))
                {
                    IsValid = true;
                    IsUndefined = true;
                    return;
                }

                var genericStartIndex = typeName.IndexOf('<');
                var genericFinishIndex = typeName.LastIndexOf('>');
                if (genericStartIndex >= 0 && genericFinishIndex >= 0)
                {
                    if (genericStartIndex < genericFinishIndex)
                    {
                        var genericTypeArgsStr = typeName.Substring(genericStartIndex + 1, genericFinishIndex - genericStartIndex - 1);
                        typeName = typeName.Substring(0, genericStartIndex);
                        var isValid = true;
                        var nested = 0;
                        var index = 0;
                        var curIndex = 0;
                        var requiredArgs = 1;
                        var args = new List<string>();
                        foreach (var genericTypeArgsChar in genericTypeArgsStr)
                        {
                            switch (genericTypeArgsChar)
                            {
                                case '<':
                                    nested++;
                                    break;

                                case '>':
                                    nested--;
                                    if (nested < 0)
                                    {
                                        isValid = false;
                                    }

                                    break;

                                case ',':
                                    if (nested == 0)
                                    {
                                        args.Add(genericTypeArgsStr.Substring(index, curIndex - index).Trim());
                                        index = curIndex + 1;
                                        requiredArgs++;
                                    }

                                    break;
                            }

                            if (!isValid)
                            {
                                break;
                            }

                            curIndex++;
                        }

                        if (isValid && requiredArgs > args.Count)
                        {
                            args.Add(genericTypeArgsStr.Substring(index, curIndex - index).Trim());
                        }

                        foreach (var genericTypeArgStr in args)
                        {
                            var genericTypeDescriptor = new TypeDescription(genericTypeArgStr, typeResolver);
                            isValid &= genericTypeDescriptor.IsValid;
                            if (!isValid)
                            {
                                break;
                            }

                            GenericTypeArgs.Add(genericTypeDescriptor);
                        }

                        if (!isValid)
                        {
                            return;
                        }

                        if (GenericTypeArgs.Count > 0)
                        {
                            var cnt = 0;
                            var genericArgsSb = new StringBuilder($"`{GenericTypeArgs.Count}");
                            if (GenericTypeArgs.Any(i => !i.IsUndefined))
                            {
                                genericArgsSb.Append('[');
                                foreach (var genericTypeArg in GenericTypeArgs)
                                {
                                    if (cnt > 0)
                                    {
                                        genericArgsSb.Append(',');
                                    }

                                    cnt++;
                                    if (!genericTypeArg.IsUndefined)
                                    {
                                        genericArgsSb.Append('[');
                                        genericArgsSb.Append(genericTypeArg.Type.AssemblyQualifiedName);
                                        genericArgsSb.Append(']');
                                    }
                                    else
                                    {
                                        genericArgsSb.Append("[]");
                                    }
                                }
                                genericArgsSb.Append(']');
                            }

                            typeName = typeName + genericArgsSb;
                        }

                        IsValid = LoadTypeUsingVariants(typeName, typeResolver);
                        return;
                    }
                }

                if (genericStartIndex == -1 && genericFinishIndex == -1)
                {
                    IsValid = LoadTypeUsingVariants(typeName, typeResolver);
                }
            }

            public bool IsValid { get; }

            private IList<TypeDescription> GenericTypeArgs { get; }

            private bool IsUndefined { get; }

            public Type Type { get; private set; }

            private bool LoadTypeUsingVariants(string typeName, TypeResolver typeResolver)
            {
                if (LoadType(typeName, typeResolver))
                {
                    return true;
                }

                foreach (var usingName in typeResolver.UsingStatements)
                {
                    var fullTypeName = $"{usingName}.{typeName}";
                    if (LoadType(fullTypeName, typeResolver))
                    {
                        return true;
                    }
                }

                return false;
            }

            private bool LoadType(string typeName, TypeResolver typeResolver)
            {
                Type type;
                if (typeResolver.TryResolveSimpleType(typeName, out type))
                {
                    OnTypeLoaded(type);
                    return true;
                }

                foreach (var reference in typeResolver.References)
                {
                    var assemblyQualifiedName = $"{typeName}, {reference.GetName()}";
                    type = Type.GetType(assemblyQualifiedName);
                    if (type != null)
                    {
                        OnTypeLoaded(type);
                        return true;
                    }

                    type = reference.GetType(assemblyQualifiedName);
                    if (type != null)
                    {
                        OnTypeLoaded(type);
                        return true;
                    }
                }

                return false;
            }

            private void OnTypeLoaded(Type type)
            {
                Type = type;
            }
        }
    }
}