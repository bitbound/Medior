using Microsoft.Win32;
using System;
using System.Linq;

namespace Medior.Services;

public interface IRegistryService
{
    void SetStartAtLogon(bool isEnabled);
    bool GetStartAtLogon();
    void SetSettingsFilePath(string filePath);
    string? GetSettingsFilePath();
}

internal class RegistryService : IRegistryService
{
    public string? GetSettingsFilePath()
    {
        var key = Registry.CurrentUser.OpenSubKey(@"Software\Medior");
        if (key is null)
        {
            return null;
        }
        return (string?)key.GetValue("SettingsFilePath", null);
    }

    public bool GetStartAtLogon()
    {
        var runKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);

        if (runKey is null)
        {
            return false;
        }
        return runKey.GetValueNames().Contains("Medior");
    }

    public void SetSettingsFilePath(string filePath)
    {
        var key = Registry.CurrentUser.CreateSubKey(@"Software\Medior", true);
        key.SetValue("SettingsFilePath", filePath, RegistryValueKind.ExpandString);
    }

    public void SetStartAtLogon(bool isEnabled)
    {
        var runKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);

        if (runKey is null)
        {
            return;
        }

        if (isEnabled)
        {
            var exePath = Environment.GetCommandLineArgs().First().Replace(".dll", ".exe", StringComparison.OrdinalIgnoreCase);
            runKey.SetValue("Medior", $@"""{exePath}"" --hidden", RegistryValueKind.ExpandString);
        }
        else
        {
            runKey.DeleteValue("Medior", false);
        }
    }
}
