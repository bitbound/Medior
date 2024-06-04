using Medior.Shared;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;

namespace Medior.Services;

public interface IProcessService
{
    Process GetCurrentProcess();

    Process[] GetProcesses();

    Process[] GetProcessesByName(string processName);

    Task<Result<string>> GetProcessOutput(string command, string arguments, int timeoutMs = 10_000);

    Task<bool> LaunchUri(Uri uri);

    Task<LaunchQuerySupportStatus> QueryUriSupportAsync(Uri uri, LaunchQuerySupportType supportType);

    Process Start(string fileName);
    Process Start(string fileName, string arguments);
    Process? Start(ProcessStartInfo startInfo);
}

public class ProcessManager : IProcessService
{
    public Process GetCurrentProcess()
    {
        return Process.GetCurrentProcess();
    }

    public Process[] GetProcesses()
    {
        return Process.GetProcesses();
    }

    public Process[] GetProcessesByName(string processName)
    {
        return Process.GetProcessesByName(processName);
    }

    public async Task<Result<string>> GetProcessOutput(string command, string arguments, int timeoutMs = 10_000)
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

    public async Task<bool> LaunchUri(Uri uri)
    {
        return await Windows.System.Launcher.LaunchUriAsync(uri);
    }

    public async Task<LaunchQuerySupportStatus> QueryUriSupportAsync(Uri uri, LaunchQuerySupportType supportType)
    {
        return await Launcher.QueryUriSupportAsync(uri, supportType);
    }

    public Process Start(string fileName)
    {
        return Process.Start(fileName);
    }

    public Process Start(string fileName, string arguments)
    {
        return Process.Start(fileName, arguments);
    }

    public Process? Start(ProcessStartInfo startInfo)
    {
        if (startInfo is null)
        {
            throw new ArgumentNullException(nameof(startInfo));
        }
        return Process.Start(startInfo);
    }
}
