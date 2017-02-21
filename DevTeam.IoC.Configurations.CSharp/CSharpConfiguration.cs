namespace DevTeam.IoC.Configurations.CSharp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Contracts;
    using Contracts.Dto;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    public class CSharpConfiguration : IConfiguration
    {
        private static readonly MetadataReference[] References;

        static CSharpConfiguration()
        {
            References = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(IConfiguration).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(@"C:\Windows\Microsoft.NET\Framework\v4.0.30319\mscorlib.dll"),
                MetadataReference.CreateFromFile(@"C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.Runtime.dll")
            };
        }

        public IEnumerable<IConfiguration> GetDependencies(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            yield break;
        }

        public IEnumerable<IDisposable> Apply(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            yield return container
                .Register()
                .Tag(GetType())
                .State<IConfigurationDescriptionDto>(0)
                .Contract<IConfiguration>()
                .FactoryMethod(ctx => CreateConfiguration(ctx.GetState<IConfigurationDescriptionDto>(0).Description))
                .Apply();
        }

        private IConfiguration CreateConfiguration(string description)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(description);
            var commentsTrivia = (
                from trivia in syntaxTree.GetRoot().DescendantTrivia()
                where trivia.Kind() == SyntaxKind.SingleLineCommentTrivia || trivia.Kind() == SyntaxKind.MultiLineCommentTrivia
                select trivia).ToList();
            var refs = new List<MetadataReference>(References);
            foreach (var syntaxTrivia in commentsTrivia)
            {
                var comment = syntaxTrivia.ToFullString().Replace("/", string.Empty).Trim();
                if (!comment.StartsWith("@"))
                {
                    continue;
                }

                var additionalAssemblyName = comment.Replace("@", "");
                refs.Add(MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName(additionalAssemblyName)).Location));
            }

            var assemblyName = Path.GetRandomFileName();
            var compilation = CSharpCompilation.Create(
                assemblyName,
                new[] { syntaxTree },
                refs,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            Assembly assembly;
            using (var ms = new MemoryStream())
            {
                var result = compilation.Emit(ms);

                if (!result.Success)
                {
                    var errors = 
                        from diagnostic in result.Diagnostics
                        where diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error
                        select $"{diagnostic.Id}: {diagnostic.GetMessage()}";

                    throw new InvalidOperationException(string.Join(Environment.NewLine, errors));
                }

                ms.Seek(0, SeekOrigin.Begin);
                assembly = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromStream(ms);
            }

            var typeOfConfiguration = (
                from type in assembly.DefinedTypes
                where type.ImplementedInterfaces.Contains(typeof(IConfiguration))
                select type).Single();

            return (IConfiguration)Activator.CreateInstance(typeOfConfiguration.AsType());
        }
    }
}
