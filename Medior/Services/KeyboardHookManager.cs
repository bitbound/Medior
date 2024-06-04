using Medior.Native;
using Microsoft.Extensions.DependencyInjection;
using PInvoke;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Medior.Services;

public interface IKeyboardHookManager
{
    void SetPrintScreenHook();
    void UnsetPrintScreenHook();
}

// Code for Hotkey class derived from here: https://blogs.msdn.microsoft.com/toub/2006/05/03/low-level-keyboard-hook-in-c/
public class KeyboardHookManager : IKeyboardHookManager
{
    private const int PrintScreen = 0x2C;
    //private const int ScrollLock = 0x91;
    //private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private readonly ConcurrentDictionary<HotKeyHookKind, IntPtr> _hooks = new();
    private readonly User32Ex.LowLevelKeyboardProc _proc = HookCallback;

    public KeyboardHookManager()
    {
        System.Windows.Application.Current.Exit += (send, arg) =>
        {
            foreach (var hook in _hooks.Values)
            {
                User32Ex.UnhookWindowsHookEx(hook);
            }
        };
    }

    public void SetPrintScreenHook()
    {
        if (!_hooks.ContainsKey(HotKeyHookKind.PrintScreen))
        {
            var hookId = SetHook(_proc);
            _hooks.AddOrUpdate(HotKeyHookKind.PrintScreen, hookId, (k, v) =>
            {
                User32Ex.UnhookWindowsHookEx(v);
                return hookId;
            });
        }
    }

    public void UnsetPrintScreenHook()
    {
        if (_hooks.TryRemove(HotKeyHookKind.PrintScreen, out var hookId))
        {
            User32Ex.UnhookWindowsHookEx(hookId);
        }
    }

    private static int HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
        {
            int vkCode = Marshal.ReadInt32(lParam);
            if (vkCode == PrintScreen)
            {
                var settings = StaticServiceProvider.Instance.GetRequiredService<ISettings>();
                if (settings.HandlePrintScreen)
                {
                    var messenger = StaticServiceProvider.Instance.GetRequiredService<IMessenger>();
                    messenger.SendGenericMessage(HotKeyHookKind.PrintScreen);
                    return 1;
                }
            }
        }
        
        return User32.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
    }

    private static IntPtr SetHook(User32Ex.LowLevelKeyboardProc proc)
    {
        using var curProcess = Process.GetCurrentProcess();
        using var curModule = curProcess?.MainModule;

        if (curModule?.ModuleName is null)
        {
            return IntPtr.Zero;
        }

       
        return User32Ex.SetWindowsHookEx(
            (int)User32.WindowsHookType.WH_KEYBOARD_LL,
            proc, 
            Kernel32Ex.GetModuleHandle(curModule.ModuleName), 
            0);
    }
}
