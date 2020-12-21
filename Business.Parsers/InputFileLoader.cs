namespace Business.Parsers
{
    using Business.Core;
    using Business.Parsers.Models;
    using EnsureThat;
    using Google.Protobuf;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using ZTR.Framework.Business.File.FileReaders;

    public class InputFileLoader
    {
        private readonly ILogger<InputFileLoader> _logger;
        private readonly string csFileExtension = ".cs", dllExtension = ".dll", fileDescriptorExtension = ".desc";

        public InputFileLoader(ILogger<InputFileLoader> logger)
        {
            _logger = logger;
        }

        public async Task<CustomMessage> GenerateCodeFiles(string moduleName, string protoFileName, string protoFilePath, params string[] args)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(moduleName);
            EnsureArg.IsNotEmptyOrWhiteSpace(protoFileName);
            EnsureArg.IsNotEmptyOrWhiteSpace(protoFilePath);

            string outputFolder = string.Empty;
            try
            {
                protoFilePath = CombinePathFromAppRoot(protoFilePath);

                // try to use protoc
                outputFolder = GenerateCSharpFile(protoFileName, protoFilePath, args);
                outputFolder = FileReaderExtensions.NormalizeFolderPath(outputFolder);

                var dllPath = await GenerateDllFromCsFileAsync(protoFileName, outputFolder);

                if (!string.IsNullOrWhiteSpace(dllPath))
                {
                    var message = GetIMessage(dllPath);

                    return new CustomMessage() { Message = message, Name = moduleName };
                }

                return null;
            }
            catch
            {
                throw;
            }
            finally
            {
                try
                {
                    if (!string.IsNullOrEmpty(outputFolder))
                    {
                        Directory.Delete(outputFolder, true);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error", ex);
                }
            }
        }

        public string GenerateCSharpFile(string fileName, string protoFilePath, params string[] args)
        {
            string tmpOutputFolder;

            tmpOutputFolder = Path.Combine($"{Global.WebRoot}tmp", Guid.NewGuid().ToString("n"));
            Directory.CreateDirectory(tmpOutputFolder);

            string protocPath = GetProtoCompilerPath();
            string tmpDescriptorFile = Path.Combine(tmpOutputFolder, fileName + fileDescriptorExtension);
            string inputs = $" --descriptor_set_out={tmpDescriptorFile} --include_imports --proto_path={protoFilePath} --csharp_out={tmpOutputFolder}  --error_format=gcc {fileName} {string.Join(" ", args)}";

            var psi = new ProcessStartInfo(
                protocPath,
                arguments: inputs
            )
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = Global.WebRoot,
                UseShellExecute = false
            };

            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.WorkingDirectory = Global.WebRoot;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = psi.RedirectStandardError = true;

            _logger.LogInformation("Starting Proto compiler");
            _logger.LogInformation(inputs);
            var proc = Process.Start(psi);

            var result = proc.StandardOutput.ReadToEnd();
            result += " " + proc.StandardError.ReadToEnd();
            proc.WaitForExit();

            _logger.LogInformation(result);

            if (proc.ExitCode != 0)
            {
                if (HasByteOrderMark(fileName))
                {
                    //stderr.WriteLine("The input file should be UTF8 without a byte-order-mark (in Visual Studio use \"File\" -> \"Advanced Save Options...\" to rectify)");
                }

                throw new ApplicationException("Protoc in linux error" + fileName);
            }

            return tmpOutputFolder;
        }

        private IMessage GetIMessage(string dllPath)
        {
            EnsureArg.IsNotNullOrWhiteSpace(dllPath);

            byte[] assemblyBytes = File.ReadAllBytes(dllPath);
            var assembly = Assembly.Load(assemblyBytes);

            var instances = from t in assembly.GetTypes()
                            where t.GetInterfaces().Contains(typeof(IMessage))
                                     && t.GetConstructor(Type.EmptyTypes) != null
                            select Activator.CreateInstance(t) as IMessage;

            foreach (var instance in instances)
            {
                if (instance.Descriptor.Name == "Config" && CanConvertToMessageType(instance.GetType()))
                {
                    return instance;
                }
            }

            return null;
        }

        private string GetProtoCompilerPath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return "protoc";
            }

            string name = "protoc.exe";
            string path = $"{Global.WebRoot}/{name}";

            if (!File.Exists(path) && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // look inside ourselves...
                using (Stream resStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    typeof(InputFileLoader).Namespace + "." + name))
                using (Stream outFile = File.OpenWrite(path))
                {
                    long len = 0;
                    int bytesRead;
                    byte[] buffer = new byte[4096];
                    while ((bytesRead = resStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        outFile.Write(buffer, 0, bytesRead);
                        len += bytesRead;
                    }
                    outFile.SetLength(len);
                }
            }

            return path;
        }

        // <summary>
        // Called by method to ask if this object can serialize
        // an object of a given type.
        // </summary>
        // <returns>True if the objectType is a Protocol Message.</returns>
        private bool CanConvertToMessageType(Type objectType)
        {
            return typeof(IMessage)
                .IsAssignableFrom(objectType);
        }

        private async Task<string> GenerateDllFromCsFileAsync(string fileName, string outputFolderPath)
        {
            fileName = char.ToUpper(fileName[0]) + fileName.Substring(1);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            string filePath = outputFolderPath + fileNameWithoutExtension;
            string csFilePath = filePath + csFileExtension;
            string dllFilePath = filePath + dllExtension;

            string localDllFolder = FileReaderExtensions.NormalizeFolderPath(CombinePathFromAppRoot(string.Empty));

            using (TextReader readFile = new StreamReader(csFilePath))
            {
                string content = await readFile.ReadToEndAsync();

                var dotnetCoreDirectory = RuntimeEnvironment.GetRuntimeDirectory();

                var compilation = CSharpCompilation.Create(fileNameWithoutExtension)
                    .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                    .AddReferences(
                        MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(SyntaxTree).GetTypeInfo().Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(CSharpSyntaxTree).GetTypeInfo().Assembly.Location),
                        MetadataReference.CreateFromFile(Path.Combine(dotnetCoreDirectory, "netstandard.dll")),
                        MetadataReference.CreateFromFile(Path.Combine(dotnetCoreDirectory, "System.Runtime.dll")),
                        MetadataReference.CreateFromFile(Path.Combine(localDllFolder, "Google.Protobuf.dll")))
                    .AddSyntaxTrees(CSharpSyntaxTree.ParseText(content));

                var eResult = compilation.Emit(dllFilePath);

                if (eResult.Success)
                {
                    return dllFilePath;
                }
            }
            return string.Empty;
        }

        private bool HasByteOrderMark(string path)
        {
            try
            {
                using Stream s = File.OpenRead(path);
                return s.ReadByte() > 127;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex); // log only
                return false;
            }
        }

        public string CombinePathFromAppRoot(string path)
        {
            string loaderPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

            if (!string.IsNullOrEmpty(loaderPath)
#pragma warning disable IDE0056 // Use index operator
                && loaderPath[loaderPath.Length - 1] != Path.DirectorySeparatorChar
                && loaderPath[loaderPath.Length - 1] != Path.AltDirectorySeparatorChar)
#pragma warning restore IDE0056 // Use index operator
            {
                loaderPath += Path.DirectorySeparatorChar;
            }
            if (loaderPath.StartsWith(@"file:\"))
            {
                loaderPath = loaderPath.Substring(6);
            }
            return Path.Combine(Path.GetDirectoryName(loaderPath), path);
        }
    }
    public sealed class ProtoParseException : Exception
    {
        public ProtoParseException(string file) : base("An error occurred parsing " + file) { }
    }
}
