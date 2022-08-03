using PInvoke;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Medior.Native
{
    public class WindowHelpers
    {
        public static bool GetDwmFrameBounds(IntPtr hwnd, out RECT dwmRect)
        {
            var rectSize = Marshal.SizeOf<RECT>();
            var ptr = Marshal.AllocHGlobal(rectSize);
            var result = DwmApi.DwmGetWindowAttribute(hwnd, DwmApi.DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS, ptr, rectSize);

            if (result.Succeeded)
            {
                dwmRect = Marshal.PtrToStructure<RECT>(ptr);
                Marshal.FreeHGlobal(ptr);
                return true;
            }
            dwmRect = new RECT();
            return false;
        }

        public static bool IsWindowDwmCloaked(IntPtr hwnd)
        {
            var cloakPointer = Marshal.AllocHGlobal(sizeof(int));
            var result = DwmApi.DwmGetWindowAttribute(hwnd, DwmApi.DWMWINDOWATTRIBUTE.DWMWA_CLOAKED, cloakPointer, sizeof(int));
            var cloakResult = Marshal.ReadInt32(cloakPointer);
            Marshal.FreeHGlobal(cloakPointer);
            return result.Succeeded && cloakResult != 0;
        }

        internal static (RECT windowRegion, IntPtr windowHandle) GetChildWindow(IntPtr parentWindow)
        {
            var targetRect = new RECT();
            var targetHwnd = IntPtr.Zero;

            var enumProc = new User32.WNDENUMPROC((hwnd, lparam) =>
            {
                if (!User32.IsWindowVisible(hwnd))
                {
                    return true;
                }

                if (targetRect.IsEmpty() && TryGetRect(hwnd, true, out var childRect))
                {
                    var (grandChildRect, grandChildHwnd) = GetChildWindow(hwnd);

                    if (!grandChildRect.IsEmpty())
                    {
                        targetHwnd = grandChildHwnd;
                        targetRect = grandChildRect;
                    }
                    else
                    {
                        targetRect = childRect;
                        targetHwnd = hwnd;
                    }
                }

                return true;
            });

            var enumProcPtr = Marshal.GetFunctionPointerForDelegate(enumProc);

            _ = User32.EnumChildWindows(parentWindow, enumProcPtr, IntPtr.Zero);

            return (targetRect, targetHwnd);
        }

        internal static (RECT windowRegion, IntPtr windowHandle) GetWindowUnderCursor(params IntPtr[] ignoreWindows)
        {
            var shellHandle = User32.GetShellWindow();
            var targetRect = new RECT();
            var targetHwnd = IntPtr.Zero;

            User32.EnumWindows((hwnd, lParam) =>
            {
                if (ignoreWindows.Contains(hwnd) || !User32.IsWindowVisible(hwnd))
                {
                    return true;
                }

                _ = User32.GetWindowThreadProcessId(hwnd, out var procId);
                if (procId == Environment.ProcessId)
                {
                    return true;
                }

                //if (User32.GetWindowText(hwnd) == "Windows Input Experience")
                //{
                //    return true;
                //}

                if (targetRect.IsEmpty() && TryGetRect(hwnd, true, out targetRect))
                {
                    targetHwnd = hwnd;
                }

                return true;
            }, IntPtr.Zero);

            if (targetRect.IsEmpty())
            {
                User32.GetWindowRect(shellHandle, out targetRect);
                return (targetRect, shellHandle);
            }
            return (targetRect, targetHwnd);
        }

        [DllImport("dwmapi.dll", PreserveSig = false)]
        private static extern bool DwmIsCompositionEnabled();

        private static bool TryGetRect(IntPtr hwnd, bool isParentWindow, out RECT outRect)
        {
            outRect = new RECT();

            User32.GetCursorPos(out var point);
            User32.GetWindowRect(hwnd, out var foundRect);

            if (isParentWindow && DwmIsCompositionEnabled())
            {
                
                if (IsWindowDwmCloaked(hwnd))
                {
                    return false;
                }

                if (GetDwmFrameBounds(hwnd, out var dwmRect))
                {
                    foundRect = dwmRect;
                }
               
            }

            if (point.IsOver(foundRect))
            {
                _ = User32.GetWindowThreadProcessId(hwnd, out var processId);
                Debug.WriteLine($"Window text: {User32.GetWindowText(hwnd)}");
                Debug.WriteLine($"Window processId: {processId}");
                Debug.WriteLine($"Found rect: {foundRect.Width()}x{foundRect.Height()}.");
                outRect = foundRect;
                return true;
            }

            Debug.WriteLine($"Rect not found.");
            return false;
        }
    }
}
