namespace Business.Parsers
{
    using EnsureThat;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Threading;

    public class InputFileLoader
    {
        public void GenerateFiles(string protoFileName, string outputFolderPath, string protoFilePath, params string[] args)
        {
            EnsureArg.IsNotEmptyOrWhiteSpace(protoFileName);

            try
            {
                outputFolderPath = CombinePathFromAppRoot(outputFolderPath);
                protoFilePath = CombinePathFromAppRoot(protoFilePath);

                // try to use protoc
                GenerateCSharpFile(fileName: protoFileName, outputFolderPath: outputFolderPath, protoFilePath: protoFilePath,  args);
            }
            catch
            {
                throw;
            }
        }

        public string GetProtoCompilerPath(out string folder)
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

        public void GenerateCSharpFile(string fileName, string outputFolderPath, string protoFilePath, params string[] args)
        {
            string tmpFolder = null;

            try
            {
                if (!Directory.Exists(outputFolderPath))
                {
                    Directory.CreateDirectory(outputFolderPath);
                }

                string protocPath = GetProtoCompilerPath(out tmpFolder);

                var psi = new ProcessStartInfo(
                    protocPath,
                    arguments: $" --include_imports --include_source_info --proto_path={protoFilePath} --csharp_out={outputFolderPath} --error_format=gcc {fileName} {string.Join(" ", args)}"
                )
                {
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    WorkingDirectory = Environment.CurrentDirectory,
                    UseShellExecute = false
                };

                psi.RedirectStandardOutput = psi.RedirectStandardError = true;

                using (Process proc = Process.Start(psi))
                {
                    var errThread = new Thread(DumpStream(proc.StandardError));
                    var outThread = new Thread(DumpStream(proc.StandardOutput));

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
                }
            }
            catch
            {
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
                && loaderPath[loaderPath.Length - 1] != Path.DirectorySeparatorChar
                && loaderPath[loaderPath.Length - 1] != Path.AltDirectorySeparatorChar)
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
