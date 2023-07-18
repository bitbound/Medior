using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;
using System.Collections.Generic;

namespace Medior.Native
{


    public static class RstrtMgrEx
    {
        private const int CchRmMaxAppName = 255;
        private const int CchRmMaxSvcName = 63;

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
        internal static extern int RmRegisterResources(uint pSessionHandle,
                                              uint nFiles,
                                              string[] rgsFilenames,
                                              uint nApplications,
                                              [In] RmUniqueProcess[]? rgApplications,
                                              uint nServices,
                                              string[]? rgsServiceNames);

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Auto)]
        internal static extern int RmStartSession(out uint pSessionHandle, int dwSessionFlags, string strSessionKey);

        [DllImport("rstrtmgr.dll")]
        internal static extern int RmEndSession(uint pSessionHandle);

        [DllImport("rstrtmgr.dll")]
        internal static extern int RmGetList(uint dwSessionHandle,
                                    out uint pnProcInfoNeeded,
                                    ref uint pnProcInfo,
                                    [In, Out] RmProcessInfo[]? rgAffectedApps,
                                    ref uint lpdwRebootReasons);



        [StructLayout(LayoutKind.Sequential)]
        internal struct RmUniqueProcess
        {
            public int dwProcessId;
            public FILETIME ProcessStartTime;
        }

        internal enum RmAppType
        {
            RmUnknownApp = 0,
            RmMainWindow = 1,
            RmOtherWindow = 2,
            RmService = 3,
            RmExplorer = 4,
            RmConsole = 5,
            RmCritical = 1000
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct RmProcessInfo
        {
            public RmUniqueProcess Process;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CchRmMaxAppName + 1)]
            public string strAppName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CchRmMaxSvcName + 1)]
            public string strServiceShortName;

            public RmAppType ApplicationType;
            public uint AppStatus;
            public uint TSSessionId;
            [MarshalAs(UnmanagedType.Bool)]
            public bool bRestartable;
        }
    }
}
