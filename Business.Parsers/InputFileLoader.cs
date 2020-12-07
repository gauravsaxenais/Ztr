namespace Business.Parsers
{
    using EnsureThat;
    using Google.Protobuf;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;
    using ZTR.Framework.Business.File.FileReaders;

    public class InputFileLoader
    {
        private readonly string csFileExtension = ".g.cs", dllExtension = ".dll", fileDescriptorExtension = ".desc";
        public IMessage GenerateCodeFiles(string protoFileName, string protoFilePath, params string[] args)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(protoFileName);

            try
            {
                protoFilePath = CombinePathFromAppRoot(protoFilePath);

                // try to use protoc
                string outputFolder = GenerateCSharpFile(protoFileName, protoFilePath, args);
                outputFolder = FileReaderExtensions.NormalizeFolderPath(outputFolder);

                var dllPath = GenerateDllFromCsFile(protoFileName, outputFolder);
                
                if (!string.IsNullOrWhiteSpace(dllPath))
                {
                    var message = GetIMessage(dllPath);

                    return message;
                }

                return null;
            }
            catch
            {
                throw;
            }

        }

        public string GenerateCSharpFile(string fileName, string protoFilePath, params string[] args)
        {
            string tmpFolder = null, tmpOutputFolder = null;

            try
            {
                tmpOutputFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
                Directory.CreateDirectory(tmpOutputFolder);

                string protocPath = GetProtoCompilerPath(out tmpFolder);
                string tmpDescriptorFile = Path.Combine(tmpOutputFolder, fileName + fileDescriptorExtension);

                var psi = new ProcessStartInfo(
                    protocPath,
                    arguments: $" --descriptor_set_out={tmpDescriptorFile} --include_imports --proto_path={protoFilePath} --csharp_out={tmpOutputFolder} --csharp_opt=file_extension={csFileExtension} --error_format=gcc {fileName} {string.Join(" ", args)}"
                )
                {
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    WorkingDirectory = Environment.CurrentDirectory,
                    UseShellExecute = false
                };

                psi.CreateNoWindow = true;
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                psi.WorkingDirectory = Environment.CurrentDirectory;
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = psi.RedirectStandardError = true;

                using Process proc = Process.Start(psi);
                Thread errThread = new Thread(DumpStream(proc.StandardError));
                Thread outThread = new Thread(DumpStream(proc.StandardOutput));
                errThread.Name = "stderr reader";
                outThread.Name = "stdout reader";
                errThread.Start();
                outThread.Start();
                proc.WaitForExit();
                outThread.Join();
                errThread.Join();
                if (proc.ExitCode != 0)
                {
                    if (HasByteOrderMark(fileName))
                    {
                        //stderr.WriteLine("The input file should be UTF8 without a byte-order-mark (in Visual Studio use \"File\" -> \"Advanced Save Options...\" to rectify)");
                    }
                    throw new ProtoParseException(Path.GetFileName(fileName));
                }
                return tmpOutputFolder;
            }
            catch
            {
                if (!string.IsNullOrWhiteSpace(tmpOutputFolder))
                {
                    try { Directory.Delete(tmpOutputFolder, true); }
                    catch { } // swallow
                }
                throw;
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(tmpFolder))
                {
                    try { Directory.Delete(tmpFolder, true); }
                    catch { } // swallow
                }

            }
        }

        public IMessage GetIMessage(string dllPath)
        {
            EnsureArg.IsNotNullOrWhiteSpace(dllPath);

            var assembly = Assembly.LoadFile(dllPath);

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

        private string GetProtoCompilerPath(out string folder)
        {
            const string Name = "protoc.exe";
            string lazyPath = CombinePathFromAppRoot(Name);

            if (File.Exists(lazyPath))
            {
                // use protoc.exe from the existing location (faster)
                folder = null;
                return lazyPath;
            }

            folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
            Directory.CreateDirectory(folder);
            string path = Path.Combine(folder, Name);

            // look inside ourselves...
            using (Stream resStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                typeof(InputFileLoader).Namespace + "." + Name))
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

        private string GenerateDllFromCsFile(string fileName, string outputFolderPath)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            string filePath = outputFolderPath + fileNameWithoutExtension;
            string csFilePath = filePath + csFileExtension;
            string dllFilePath = filePath + dllExtension;

            string localDllFolder = FileReaderExtensions.NormalizeFolderPath(CombinePathFromAppRoot(string.Empty));

            TextReader readFile = new StreamReader(csFilePath);
            string content = readFile.ReadToEnd();

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

        private ThreadStart DumpStream(TextReader reader)
        {
            return (ThreadStart)delegate
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Debug.WriteLine(line);
                }
            };
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
