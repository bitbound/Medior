using Medior.Services;
using Medior.Services.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Native
{
    // Code for Hotkey class derived from here: https://blogs.msdn.microsoft.com/toub/2006/05/03/low-level-keyboard-hook-in-c/
    public static class PrintScreenHotkey
    {
        private const int PrintScreen = 0x2C;
        private const int ScrollLock = 0x91;
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static readonly LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookId = IntPtr.Zero;
        private static bool _isHotkeySet;
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        static PrintScreenHotkey()
        {
            System.Windows.Application.Current.Exit += (send, arg) =>
            {
                UnhookWindowsHookEx(_hookId);
            };
        }

        public static void Set()
        {
            if (!_isHotkeySet)
            {
                _isHotkeySet = true;
                _hookId = SetHook(_proc);
            }
        }

        public static void Unset()
        {
            if (_isHotkeySet)
            {
                UnhookWindowsHookEx(_hookId);
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);


        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
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
                        messenger.Send<PrintScreenInvokedMessage>();
                        return new IntPtr(1);
                    }
                }
            }
            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using Process curProcess = Process.GetCurrentProcess();
            using ProcessModule curModule = curProcess.MainModule;
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                GetModuleHandle(curModule.ModuleName), 0);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
    }
}
