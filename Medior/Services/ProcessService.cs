using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Services
{
    public interface IProcessService
    {
        Process Start(string fileName);
        Process Start(string fileName, string arguments);
        Process? Start(ProcessStartInfo startInfo);
    }

    public class ProcessService : IProcessService
    {
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
