using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ZTR.Framework.Business
{
    public static class ProcessExtensions
    {
        public static async Task WaitForExitAsync(this Process process, CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<bool>();

            void ProcessExited(object sender, EventArgs e)
            {
                Task.Run(() => tcs.TrySetResult(true));
            }

            process.EnableRaisingEvents = true;
            process.Exited += ProcessExited;

            try
            {
                if (process.HasExited)
                {
                    return;
                }

                using (cancellationToken.Register(() => Task.Run(() => tcs.TrySetCanceled())))
                {
                    await tcs.Task;
                }
            }
            finally
            {
                process.Exited -= ProcessExited;
            }
        }
    }
}
