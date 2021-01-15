namespace Business.Parsers.ProtoParser.Parser
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using ZTR.Framework.Business.File.FileReaders;

    public class ProtoFileCompiler : IProtoFileCompiler
    {
        private readonly PortableExecutableReference[] _references;
        private static readonly string LocalDllFolder = FileReaderExtensions.NormalizeFolderPath(FileReaderExtensions.CombinePathFromAppRoot(string.Empty));

        private readonly Assembly _systemRuntime = Assembly.Load(new AssemblyName("System.Runtime"));
        private readonly Assembly _netStandard = Assembly.Load(new AssemblyName("netstandard"));
        private readonly Assembly _protoBuf = Assembly.LoadFile(Path.Combine(LocalDllFolder, "Google.Protobuf.dll"));

        public ProtoFileCompiler(IEnumerable<Type> referencedTypes)
        {
            _references = GetReferences(referencedTypes);
        }

        public void GenerateDllFromFile(string dllPath, string fileContent, string fileName)
        {
            var compilation = CSharpCompilation.Create(fileName,
                                                        new List<SyntaxTree> {CSharpSyntaxTree.ParseText(fileContent)},
                                                        _references,
                                                        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using var assemblyLoadContext = new CollectibleAssemblyLoadContext();
            
            var cr = compilation.Emit(dllPath);
            if (!cr.Success)
            {
                throw new InvalidOperationException("Error in expression: " + cr.Diagnostics.First(e =>
                    e.Severity == DiagnosticSeverity.Error).GetMessage());
            }
        }

        private PortableExecutableReference[] GetReferences(IEnumerable<Type> referencedTypes)
        {
            var standardReferenceHints = new[] { typeof(object), typeof(SyntaxTree), typeof(CSharpSyntaxTree) };
            var allHints = standardReferenceHints.Concat(referencedTypes);
            var includedAssemblies = new[] { _systemRuntime, _netStandard, _protoBuf }.Concat(allHints.Select(t => t.Assembly)).Distinct();

            return includedAssemblies.Select(a => MetadataReference.CreateFromFile(a.Location)).ToArray();
        }
    }
}
