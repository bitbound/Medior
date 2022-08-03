using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Services
{
    public interface IRegistryService
    {
        void SetStartAtLogon(bool isEnabled);
        bool GetStartAtLogon();
    }

    internal class RegistryService : IRegistryService
    {
        public bool GetStartAtLogon()
        {
            var runKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);

            if (runKey is null)
            {
                return false;
            }
            return runKey.GetValueNames().Contains("Medior");
        }

        public void SetStartAtLogon(bool isEnabled)
        {
            var runKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);

            if (runKey is null)
            {
                return;
            }

            if (isEnabled)
            {
                var exePath = Environment.GetCommandLineArgs().First().Replace(".dll", ".exe", StringComparison.OrdinalIgnoreCase);
                runKey.SetValue("Medior", $@"""{exePath}"" --hidden", Microsoft.Win32.RegistryValueKind.ExpandString);
            }
            else
            {
                runKey.DeleteValue("Medior", false);
            }
        }
    }
}
