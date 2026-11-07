using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using SevenKeyboard;

[assembly: DefaultDllImportSearchPaths(DllImportSearchPath.System32)]

internal static unsafe partial class Program
{
    private static int checks;

    private static int Main()
    {
        nint library = NativeLibrary.Load("user32.dll", typeof(Program).Assembly, DllImportSearchPath.System32);
        nint fixture = 0;
        try
        {
            // A packed Int64 is the independent ABI used by the original AHK implementation.
            var query = (delegate* unmanaged[Stdcall]<long, nint>)NativeLibrary.GetExport(library, "WindowFromPoint");
            int[] coordinates = [int.MinValue, -65537, -32769, -1, 0, 1, 32768, 65536, int.MaxValue];
            foreach (int x in coordinates)
                foreach (int y in coordinates)
                    Compare(query, x, y);

            if (GetCursorPos(out Point cursor) == 0)
                throw new Win32Exception(Marshal.GetLastPInvokeError());
            Compare(query, cursor.X, cursor.Y);

            // Owned STATIC fixture: hidden, disabled, negative coordinates, never shown or activated.
            fixture = CreateWindowExW(0x08000000, "STATIC", "Port smoke", 0x08000000,
                -120, -100, 80, 40, 0, 0, 0, 0);
            if (fixture == 0)
                throw new Win32Exception(Marshal.GetLastPInvokeError());
            Check(IsWindowVisible(fixture) == 0, "Fixture is hidden.");
            Check(IsWindowEnabled(fixture) == 0, "Fixture is disabled.");
            Check(GetActiveWindow() != fixture && GetFocus() != fixture, "Fixture is inactive and unfocused.");
            Compare(query, -110, -90);
            Check(WindowFromPoint.Get(-110, -90) != fixture, "Hidden/disabled fixture is excluded.");
            Check(WindowFromPoint.Get(int.MinValue, int.MaxValue) == 0, "Outside desktop returns zero.");
            Console.WriteLine($"PASS: {checks} checks, {RuntimeInformation.ProcessArchitecture}.");
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error);
            return 1;
        }
        finally
        {
            if (fixture != 0 && DestroyWindow(fixture) == 0)
                throw new Win32Exception(Marshal.GetLastPInvokeError());
            NativeLibrary.Free(library);
        }
    }

    private static void Compare(delegate* unmanaged[Stdcall]<long, nint> query, int x, int y)
    {
        long packed = unchecked((long)((ulong)(uint)x | ((ulong)(uint)y << 32)));
        for (int attempt = 0; attempt < 5; attempt++)
        {
            nint before = query(packed);
            nint actual = WindowFromPoint.Get(x, y);
            nint after = query(packed);
            if (before != after)
                continue;
            Check(actual == before, $"Native HWND at ({x}, {y}).");
            return;
        }
        throw new InvalidOperationException($"No stable native observation at ({x}, {y}).");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
        checks++;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Point
    {
        internal int X;
        internal int Y;
    }

    [LibraryImport("user32.dll", EntryPoint = "GetCursorPos", SetLastError = true)]
    private static partial int GetCursorPos(out Point point);

    [LibraryImport("user32.dll", EntryPoint = "CreateWindowExW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    private static partial nint CreateWindowExW(uint extendedStyle, string className, string title, uint style,
        int x, int y, int width, int height, nint parent, nint menu, nint instance, nint parameter);

    [LibraryImport("user32.dll", EntryPoint = "DestroyWindow", SetLastError = true)]
    private static partial int DestroyWindow(nint window);

    [LibraryImport("user32.dll", EntryPoint = "IsWindowVisible")]
    private static partial int IsWindowVisible(nint window);

    [LibraryImport("user32.dll", EntryPoint = "IsWindowEnabled")]
    private static partial int IsWindowEnabled(nint window);

    [LibraryImport("user32.dll", EntryPoint = "GetActiveWindow")]
    private static partial nint GetActiveWindow();

    [LibraryImport("user32.dll", EntryPoint = "GetFocus")]
    private static partial nint GetFocus();
}
