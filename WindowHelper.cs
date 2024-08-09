using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace MoveWindow
{
    internal class WindowHelper
    {
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindow(IntPtr hWnd, GetWindowCmd uCmd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_TOOLWINDOW = 0x00000080;
        public const int WS_EX_APPWINDOW = 0x00040000;
        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOZORDER = 0x0004;
        public const uint SWP_NOACTIVATE = 0x0010;
        public static readonly IntPtr HWND_TOP = IntPtr.Zero;

        public enum GetWindowCmd : uint
        {
            GW_OWNER = 4
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public static bool IsTopLevelWindow(IntPtr hWnd)
        {
            return GetWindow(hWnd, GetWindowCmd.GW_OWNER) == IntPtr.Zero &&
                   (GetWindowLong(hWnd, GWL_EXSTYLE) & WS_EX_TOOLWINDOW) == 0;
        }

        public static void SaveWindowPositionAndSize(string windowTitle, int x, int y, int width, int height)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\MoveWindow\" + windowTitle))
            {
                if (key != null)
                {
                    key.SetValue("X", x);
                    key.SetValue("Y", y);
                    key.SetValue("Width", width);
                    key.SetValue("Height", height);
                }
            }
        }

        public static (int x, int y, int width, int height)? LoadWindowPositionAndSize(string windowTitle)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\MoveWindow\" + windowTitle))
            {
                if (key != null)
                {
                    object xValue = key.GetValue("X");
                    object yValue = key.GetValue("Y");
                    object widthValue = key.GetValue("Width");
                    object heightValue = key.GetValue("Height");

                    if (xValue != null && yValue != null && widthValue != null && heightValue != null)
                    {
                        int x = (int)xValue;
                        int y = (int)yValue;
                        int width = (int)widthValue;
                        int height = (int)heightValue;
                        return (x, y, width, height);
                    }
                }
            }
            return null;
        }

        public static Dictionary<string, Dictionary<string, string>> LoadAllRegistryValues()
        {
            var registryValues = new Dictionary<string, Dictionary<string, string>>();

            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\MoveWindow", false))
                {
                    if (key != null)
                    {
                        foreach (var subKeyName in key.GetSubKeyNames())
                        {
                            using (var subKey = key.OpenSubKey(subKeyName))
                            {
                                if (subKey != null)
                                {
                                    var subKeyValues = new Dictionary<string, string>();
                                    foreach (var valueName in subKey.GetValueNames())
                                    {
                                        string value = subKey.GetValue(valueName)?.ToString();
                                        if (!string.IsNullOrEmpty(value))
                                        {
                                            subKeyValues[valueName] = value;
                                        }
                                    }
                                    registryValues[subKeyName] = subKeyValues;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error accessing registry: {ex.Message}");
            }

            return registryValues;
        }
    }
}
