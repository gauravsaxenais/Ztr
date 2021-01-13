namespace Business.Parsers.ProtoParser.Parser
{
    using EnsureThat;
    using Google.Protobuf;
    using Microsoft.Extensions.Logging;
    using Models;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using ZTR.Framework.Business.File.FileReaders;

    public sealed class ProtoMessageParser : IProtoMessageParser
    {
        private readonly ILogger<ProtoMessageParser> _logger;
        private readonly IProtoFileCompiler _protoFileCompiler;
        private const string CsExtension = ".cs";
        private const string DllExtension = ".dll";

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtoMessageParser"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="protoFileCompiler">The proto file compiler.</param>
        public ProtoMessageParser(ILogger<ProtoMessageParser> logger, IProtoFileCompiler protoFileCompiler)
        {
            _logger = logger;
            _protoFileCompiler = protoFileCompiler;
        }

        /// <summary>
        /// Gets the custom messages.
        /// </summary>
        /// <param name="protoFilePath">The proto file path.</param>
        /// <param name="moduleName"></param>
        /// <returns>
        /// custom message containing the proto parsed message
        /// </returns>
        public async Task<CustomMessage> GetCustomMessage(string protoFilePath, string moduleName)
        {
            var fileName = Path.GetFileName(protoFilePath);

            var info = new FileInfo(protoFilePath).Directory;

            if (info != null)
            {
                var protoDirectory = info.FullName;

                var result = await GetProtoParsedMessage(fileName, protoDirectory).ConfigureAwait(false);

                if (result != null)
                {
                    result.Name = moduleName;
                    return result;
                }
            }

            return null;
        }

        private async Task<CustomMessage> GetProtoParsedMessage(string protoFileName, string protoFilePath, params string[] args)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(protoFileName);
            EnsureArg.IsNotEmptyOrWhiteSpace(protoFilePath);

            string outputFolder = string.Empty;
            var prefix = nameof(ProtoMessageParser);

            try
            {
                // try to use protoc
                outputFolder = await GenerateCSharpFileAsync(protoFileName, protoFilePath, args).ConfigureAwait(false);
                outputFolder = FileReaderExtensions.NormalizeFolderPath(outputFolder);

                var dllPath = await GenerateDllFromCsFileAsync(protoFileName, outputFolder);

                if (!string.IsNullOrWhiteSpace(dllPath))
                {
                    var message = GetIMessage(dllPath);
                    return message;
                }

                return null;
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
                    _logger.LogError($"{prefix} Error while deleting the output folder: {outputFolder}.", ex);
                }
            }
        }

        public async Task<string> GenerateCSharpFileAsync(string fileName, string protoFilePath, params string[] args)
        {
            var tmpOutputFolder = Path.Combine($"{Global.WebRoot}tmp", Guid.NewGuid().ToString("n"));
            Directory.CreateDirectory(tmpOutputFolder);

            var protocPath = GetProtoCompilerPath();
            var inputs = $" --proto_path={protoFilePath} --csharp_out={tmpOutputFolder}  --error_format=gcc {fileName} {string.Join(" ", args)}";

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

            psi.RedirectStandardOutput = psi.RedirectStandardError = true;

            _logger.LogInformation("Starting Proto compiler");
            _logger.LogInformation(inputs);

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
                    _logger.LogCritical("The input file should be UTF8 without a byte-order-mark (in Visual Studio use \"File\" -> \"Advanced Save Options...\" to rectify)");
                }

                throw new ApplicationException("There is an issue in parsing proto file." + fileName);
            }

            return tmpOutputFolder;
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

        private CustomMessage GetIMessage(string dllPath)
        {
            EnsureArg.IsNotNullOrWhiteSpace(dllPath);
            CustomMessage customMessage;
            byte[] result = File.ReadAllBytes(dllPath);

            using (var context = new CollectibleAssemblyLoadContext())
            using (var ms = new MemoryStream(result))
            {
                var assembly = context.LoadFromStream(ms);

                var instances = from t in assembly.GetTypes()
                                where t.GetInterfaces().Contains(typeof(IMessage))
                                      && t.GetConstructor(Type.EmptyTypes) != null
                                select Activator.CreateInstance(t) as IMessage;

                foreach (var instance in instances)
                {
                    if (instance.Descriptor.Name == "Config" && CanConvertToMessageType(instance.GetType()))
                    {
                        customMessage = new CustomMessage()
                        {
                            Message = instance
                        };

                        return customMessage;
                    }
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
            string path = FileReaderExtensions.ToSafeFullPath(Global.WebRoot, name);

            if (!File.Exists(path) && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // look inside ourselves...
                using (var resStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    typeof(ProtoMessageParser).Namespace + "." + name))
                using (Stream outFile = File.OpenWrite(path))
                {
                    long len = 0;
                    int bytesRead;
                    var buffer = new byte[4096];
                    while (resStream != null && (bytesRead = resStream.Read(buffer, 0, buffer.Length)) > 0)
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
        // <returns>True if the objectType is a Protocol ProtoParsedMessage.</returns>
        private bool CanConvertToMessageType(Type objectType)
        {
            return typeof(IMessage)
                .IsAssignableFrom(objectType);
        }

        private async Task<string> GenerateDllFromCsFileAsync(string fileName, string outputFolderPath)
        {
            fileName = char.ToUpper(fileName[0]) + fileName[1..];

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            string csFilePath = outputFolderPath + fileNameWithoutExtension + CsExtension;
            string dllFilePath = outputFolderPath + fileNameWithoutExtension + DllExtension;

            string fileContent;

            using (TextReader readFile = new StreamReader(csFilePath))
            {
                fileContent = await readFile.ReadToEndAsync();
            }

            try
            {
                _protoFileCompiler.GenerateDllFromFile(dllFilePath, fileContent, fileName);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error in compiling cs file to dll. " + fileName, ex);
            }

            return dllFilePath;
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
    }
}
