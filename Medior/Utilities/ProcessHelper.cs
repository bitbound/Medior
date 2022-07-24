using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Medior.Utilities
{
    internal static class ProcessHelper
    {
        public static async Task<Result<string>> GetProcessOutput(string command, string arguments, int timeoutMs = 10_000)
        {
            try
            {
                var psi = new ProcessStartInfo(command, arguments)
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };

                var proc = Process.Start(psi);

                using var cts = new CancellationTokenSource(timeoutMs);
                await proc!.WaitForExitAsync(cts.Token);

                var output = await proc.StandardOutput.ReadToEndAsync();
                return Result.Ok(output);
            }
            catch (OperationCanceledException)
            {
                return Result.Fail<string>($"Timed out while waiting for command to finish.  " +
                    $"Command: {command}.  Arguments: {arguments}");
            }
            catch (Exception ex)
            {
                return Result.Fail<string>(ex);
            }
        }
    }
}
