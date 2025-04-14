using System.Runtime.InteropServices;

namespace DesktopBlocks;

public class MonitorInfo
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
        public readonly int Width => Right - Left;
        public readonly int Height => Bottom - Top;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MonitorInfoEx
    {
        public int Size;
        public Rect Monitor;
        public Rect WorkArea;
        public uint Flags;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public char[] DeviceName;
        public readonly bool IsPrimaryMonitor => Flags >= 0;
    }

    public enum DpiType
    {
        Effective = 0,
        Angular = 1,
        Raw = 2
    }

    // New: Process DPI awareness enums
    public enum Process_Dpi_Awareness
    {
        Process_DPI_Unaware = 0,
        Process_System_DPI_Aware = 1,
        Process_Per_Monitor_DPI_Aware = 2
    }

    // New: Import to set process DPI awareness
    [DllImport("Shcore.dll", SetLastError = true)]
    private static extern int SetProcessDpiAwareness(Process_Dpi_Awareness awareness);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

    public delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData);

    [DllImport("user32.dll")]
    public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfoEx lpmi);

    [DllImport("Shcore.dll")]
    private static extern int GetDpiForMonitor(IntPtr hMonitor, DpiType dpiType, out uint dpiX, out uint dpiY);

    // Static constructor to set process DPI awareness
    static MonitorInfo()
    {
        // Set the process to be per-monitor DPI aware.
        // This needs to be done before any DPI operations.
        SetProcessDpiAwareness(Process_Dpi_Awareness.Process_Per_Monitor_DPI_Aware);
    }

    public static List<(MonitorInfoEx Monitor, float ScaleFactor)> GetMonitorsWithScale()
    {
        List<(MonitorInfoEx Monitor, float ScaleFactor)> monitors = [];
        EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData) =>
        {
            MonitorInfoEx mi = new()
            {
                Size = Marshal.SizeOf(typeof(MonitorInfoEx)),
                DeviceName = new char[32]
            };

            if (GetMonitorInfo(hMonitor, ref mi))
            {
                // Get DPI for the monitor
                int result = GetDpiForMonitor(hMonitor, DpiType.Effective, out uint dpiX, out uint dpiY);
                if (result == 0) // S_OK
                {
                    // Calculate scale factor (DPI / 96, where 96 is the default DPI)
                    float scaleFactor = dpiX / 96.0f;
                    monitors.Add((mi, scaleFactor));
                }
                else
                {
                    // Default to 1.0 scale if DPI retrieval fails
                    monitors.Add((mi, 1.0f));
                }
            }
            return true;
        }, IntPtr.Zero);

        return monitors;
    }
}

