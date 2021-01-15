namespace Business.Parsers.ProtoParser.Parser
{
    using EnsureThat;
    using Google.Protobuf;
    using Microsoft.Extensions.Logging;
    using Models;
    using System;
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
        private static readonly object LockObject = new object();

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
        /// <returns>
        /// custom message containing the proto parsed message
        /// </returns>
        public async Task<CustomMessage> GetCustomMessage(string protoFilePath)
        {
            var result = await GetProtoParsedMessage(protoFilePath).ConfigureAwait(false);
            
            return result;
        }

        private async Task<CustomMessage> GetProtoParsedMessage(string protoFilePath, params string[] args)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(protoFilePath);

            string outputFolder = string.Empty;
            var prefix = nameof(ProtoMessageParser);

            try
            {
                protoFilePath = FileReaderExtensions.CombinePathFromAppRoot(protoFilePath);

                _logger.LogInformation($"Inside method: {nameof(GetProtoParsedMessage)}. Creating temp directory for protofilePath = {protoFilePath}.");
                outputFolder = Path.Combine($"{Global.WebRoot}tmp", Guid.NewGuid().ToString("n"));
                outputFolder = FileReaderExtensions.NormalizeFolderPath(outputFolder);

                Directory.CreateDirectory(outputFolder);

                if (HasGeneratedCSharpFile(outputFolder, protoFilePath, args))
                {
                    var protoFileName = Path.GetFileName(protoFilePath);
                    var dllPath = await GenerateDllFromCsFileAsync(protoFileName, outputFolder);

                    if (!string.IsNullOrWhiteSpace(dllPath))
                    {
                        var message = GetIMessage(dllPath);
                        return message;
                    }
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

        private bool HasGeneratedCSharpFile(string outputFolder, string protoFilePath, params string[] args)
        {
            string protocPath = GetProtoCompilerPath();

            string protoFileName = Path.GetFileName(protoFilePath);
            string protoFolder = Path.GetDirectoryName(protoFilePath);

            string arguments = $" --proto_path={protoFolder} --csharp_out={outputFolder} --error_format=gcc {protoFileName} {string.Join(" ", args)}";

            Monitor.Enter(LockObject);
            try
            {
                _logger.LogInformation($"Inside method: {nameof(HasGeneratedCSharpFile)}. Now generating cs file.");

                using (var process = new ProcessExecutor(protocPath))
                {
                    process.Run(arguments);
                    process.Wait();
                }

                _logger.LogInformation($"Inside method: {nameof(HasGeneratedCSharpFile)}. cs file generated successfully.");
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Protoc in linux error: " + outputFolder, ex);
            }
            finally
            {
                Monitor.Exit(LockObject);
            }

            return true;
        }

        private CustomMessage GetIMessage(string dllPath)
        {
            EnsureArg.IsNotNullOrWhiteSpace(dllPath);
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
                        var customMessage = new CustomMessage()
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
            _logger.LogInformation($"Inside method {nameof(GetProtoCompilerPath)}.");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                _logger.LogInformation($"Inside method {nameof(GetProtoCompilerPath)}. Operating system is linux");
                return "protoc";
            }

            _logger.LogInformation($"Inside method {nameof(GetProtoCompilerPath)}. Operating system is windows.");
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
            _logger.LogInformation($"Inside method: {nameof(GenerateDllFromCsFileAsync)}. Generating dll for {fileName} and outputFolder: {outputFolderPath}");
            fileName = char.ToUpper(fileName[0]) + fileName[1..];

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            string csFilePath = Path.Combine(outputFolderPath, fileNameWithoutExtension + CsExtension);
            string dllFilePath = Path.Combine(outputFolderPath, fileNameWithoutExtension + DllExtension);

            string fileContent = await File.ReadAllTextAsync(csFilePath);

            try
            {
                _protoFileCompiler.GenerateDllFromFile(dllFilePath, fileContent, fileName);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error in compiling cs file to dll. " + fileName, ex);
            }

            _logger.LogInformation($"Inside method: {nameof(GenerateDllFromCsFileAsync)}. Generated dll for {fileName} and outputFolder: {outputFolderPath}");
            return dllFilePath;
        }
    }
}
