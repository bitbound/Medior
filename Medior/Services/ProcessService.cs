using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System;

namespace Medior.Services
{
    public interface IProcessService
    {
        Process GetCurrentProcess();

        Process[] GetProcesses();

        Process[] GetProcessesByName(string processName);

        Task<bool> LaunchUri(Uri uri);

        Process Start(string fileName);
        Process Start(string fileName, string arguments);
        Process? Start(ProcessStartInfo startInfo);
        Task<LaunchQuerySupportStatus> QueryUriSupportAsync(Uri uri, LaunchQuerySupportType supportType);
    }

    public class ProcessService : IProcessService
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
}
