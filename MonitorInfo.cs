using System.Runtime.InteropServices;

namespace DesktopBlocks;

class MonitorInfo
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
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
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

    public delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData);

    [DllImport("user32.dll")]
    public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfoEx lpmi);

    public static List<MonitorInfoEx> GetMonitors()
    {
        List<MonitorInfoEx> monitors = [];
        EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData) =>
        {
            MonitorInfoEx mi = new()
            {
                Size = Marshal.SizeOf(typeof(MonitorInfoEx)),
                DeviceName = new char[32]
            };

            if (GetMonitorInfo(hMonitor, ref mi))
            {
                monitors.Add(mi);
            }
            return true;
        }, IntPtr.Zero);

        return monitors;
    }
}

