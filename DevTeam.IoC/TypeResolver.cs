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

        public bool TryResolveType(IEnumerable<Assembly> references, IEnumerable<string> usings, string typeName, out Type type)
        {
            if (references == null) throw new ArgumentNullException(nameof(references));
            if (usings == null) throw new ArgumentNullException(nameof(usings));
            var refList = references.ToList();
            var usingList = usings.ToList();

            if (typeName.IsNullOrWhiteSpace())
            {
                type = default(Type);
                return false;
            }

            if (TryResolveSimpleType(typeName, out type))
            {
                return true;
            }

            var typeDescription = new TypeDescription(refList, usingList, typeName, this);
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
            private readonly ICollection<Assembly> _refList;
            private readonly ICollection<string> _usingList;

            public TypeDescription(ICollection<Assembly> refList, ICollection<string> usingList, string typeName, TypeResolver typeResolver)
            {
                _refList = refList;
                _usingList = usingList;
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
                            var genericTypeDescriptor = new TypeDescription(refList, usingList, genericTypeArgStr, typeResolver);
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

                foreach (var usingName in _usingList)
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
                if (LoadTypeInternal(typeName, typeResolver))
                {
                    return true;
                }

                string newTypeName;
                while (TryGetNestedTypeName(typeName, out newTypeName))
                {
                    if (LoadTypeInternal(newTypeName, typeResolver))
                    {
                        return true;
                    }

                    typeName = newTypeName;
                }

                return false;
            }

            private bool TryGetNestedTypeName([NotNull] string typeName, out string nestedTypeName)
            {
                if (typeName == null) throw new ArgumentNullException(nameof(typeName));
                int pointIndex = -1;
                int nested = 0;
                for (var i = typeName.Length - 1; i >= 0; i--)
                {
                    switch (typeName[i])
                    {
                        case ']':
                            nested++;
                            continue;

                        case '[':
                            nested--;
                            continue;
                    }

                    if (nested > 0)
                    {
                        continue;
                    }

                    if (typeName[i] == '.')
                    {
                        pointIndex = i;
                        break;
                    }
                }

                if (pointIndex < 0)
                {
                    nestedTypeName = default(string);
                    return false;
                }

                nestedTypeName = typeName.Substring(0, pointIndex) + "+" + typeName.Substring(pointIndex + 1, typeName.Length - pointIndex - 1);
                return true;
            }

            private bool LoadTypeInternal(string typeName, TypeResolver typeResolver)
            {
                Type type;
                if (typeResolver.TryResolveSimpleType(typeName, out type))
                {
                    OnTypeLoaded(type);
                    return true;
                }

                foreach (var reference in _refList)
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