namespace Business.Parsers.ProtoParser.Parser
{
    using System;
    using System.Diagnostics;
    using System.Text;

    public class ProcessExecutor : IDisposable
    {
        public ProcessExecutor(string path)
        {
            _path = path;
        }

        public void Dispose()
        {
            Close();
        }

        public void Run(string args)
        {
            // Make sure we are don't have any old baggage.
            Close();

            // Fresh start.
            _stdOut.Clear();
            _stdErr.Clear();
            
            _process = new Process
            {
                StartInfo = new ProcessStartInfo(_path)
                {
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    ErrorDialog = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Global.WebRoot
                }
            };

            _process.OutputDataReceived += (sender, e) =>
            {
                _stdOut.AppendLine(e.Data);
            };
            _process.ErrorDataReceived += (sender, e) =>
            {
                _stdErr.AppendLine(e.Data);
            };

            _process.Start();
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
            _process.Refresh();
        }

        public void Cancel()
        {
            var proc = _process;
            _process = null;
            if (proc != null)
            {
                // Invalidate cached data to requery.
                proc.Refresh();

                Kill();
            }
        }

        public void Wait()
        {
            Wait(-1);
        }

        public bool Wait(int timeoutMs)
        {
            try
            {
                if (timeoutMs < 0)
                {
                    // Wait for process and all I/O to finish.
                    _process.WaitForExit();
                    return true;
                }

                if (_process != null)
                {
                    // Timed waiting. We need to wait for I/O ourselves.
                    if (!_process.WaitForExit(timeoutMs))
                    {
                        Kill();
                    }
                }
                
                return true;
            }
            finally
            {
                // Cleanup.
                Cancel();
            }
        }

        private void Close()
        {
            Cancel();
            var proc = _process;
            _process = null;
            if (proc != null)
            {
                // Dispose in all cases.
                proc.Close();
                proc.Dispose();
            }
        }

        private void Kill()
        {
            try
            {
                // We need to do this in case of a non-UI proc
                // or one to be forced to cancel.
                var proc = _process;
                if (proc != null && !proc.HasExited)
                {
                    proc.Kill();
                }
            }
            catch
            {
                // Kill will throw when/if the process has already exited.
            }
        }

        private readonly string _path;
        private readonly StringBuilder _stdOut = new StringBuilder();
        private readonly StringBuilder _stdErr = new StringBuilder();

        private Process _process;
    }
}
