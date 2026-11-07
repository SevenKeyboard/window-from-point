//==============================================================
// windowFromPoint — Gets the window handle at the given screen coordinates
//
// GitHub: https://github.com/SevenKeyboard/window-from-point
// Author: SevenKeyboard Ltd. (2026)
// License: The Unlicense
//
// Documentation / References:
//   https://www.autohotkey.com/boards/viewtopic.php?t=36281
//==============================================================

using System.Runtime.InteropServices;

namespace SevenKeyboard;

public static partial class WindowFromPoint
{
    public static nint Get(int x, int y) => WindowFromPointNative(new Point { X = x, Y = y });

    [StructLayout(LayoutKind.Sequential)]
    private struct Point
    {
        internal int X;
        internal int Y;
    }

    [LibraryImport("user32.dll", EntryPoint = "WindowFromPoint")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static partial nint WindowFromPointNative(Point point);
}
