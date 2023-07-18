using Medior.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Medior.Native.RstrtMgrEx;

namespace Medior.Services
{
    internal interface IFileLockSearcher
    {
        Result<List<Process>> GetLockingProcesses(string path);
    }

    internal class FileLockSearcher : IFileLockSearcher
    {
        private const int RmRebootReasonNone = 0;
        private const int ErrorMoreData = 234;

        public Result<List<Process>> GetLockingProcesses(string path)
        {
            var key = Guid.NewGuid().ToString();
            var processes = new List<Process>();
            var result = RmStartSession(out var handle, 0, key);

            if (result != 0)
            {
                return Result.Fail<List<Process>>($"{nameof(RmStartSession)} returned {result}.");
            }

            try
            {

                uint lpdwRebootReasons = RmRebootReasonNone;

                var resources = new[] { path };

                result = RmRegisterResources(handle, (uint)resources.Length, resources, 0, null, 0, null);

                if (result != 0)
                {
                    return Result.Fail<List<Process>>($"{nameof(RmRegisterResources)} returned {result}.");
                }


                uint pnProcInfo = 0;
                result = RmGetList(handle, out var pnProcInfoNeeded, ref pnProcInfo, null, ref lpdwRebootReasons);

                if (result != ErrorMoreData)
                {
                    return Result.Fail<List<Process>>("Failed to list locking processes.");
                }

                var processInfo = new RmProcessInfo[pnProcInfoNeeded];
                pnProcInfo = pnProcInfoNeeded;

                result = RmGetList(handle, out pnProcInfoNeeded, ref pnProcInfo, processInfo, ref lpdwRebootReasons);

                if (result != 0)
                {
                    return Result.Fail<List<Process>>("Failed to list locking processes.");
                }

                processes = new List<Process>((int)pnProcInfo);

                for (var i = 0; i < pnProcInfo; i++)
                {
                    try
                    {
                        processes.Add(Process.GetProcessById(processInfo[i].Process.dwProcessId));
                    }
                    catch { }
                }
            }
            finally
            {
                _ = RmEndSession(handle);
            }

            return Result.Ok(processes);
        }
    }
}
