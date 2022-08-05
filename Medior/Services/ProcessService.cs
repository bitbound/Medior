using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Medior.Services
{
    public interface IProcessService
    {
        Task<bool> LaunchUri(Uri uri);

        Process Start(string fileName);
        Process Start(string fileName, string arguments);
        Process? Start(ProcessStartInfo startInfo);
    }

    public class ProcessService : IProcessService
    {
        public async Task<bool> LaunchUri(Uri uri)
        {
            return await Windows.System.Launcher.LaunchUriAsync(uri);
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
