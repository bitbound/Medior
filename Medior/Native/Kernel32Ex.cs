using System;
using System.Runtime.InteropServices;

namespace Medior.Native
{
    internal static class Kernel32Ex
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
