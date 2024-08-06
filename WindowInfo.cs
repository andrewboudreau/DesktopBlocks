using System.Runtime.InteropServices;

namespace DesktopBlocks;

class WindowInfo
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    public class Window
    {
        public IntPtr Handle { get; set; }
        public Rect Bounds { get; set; }
        public required string Title { get; set; }
        public required string ClassName { get; set; }
        public bool IsVisible { get; set; }
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int GetClassName(IntPtr hWnd, System.Text.StringBuilder lpClassName, int nMaxCount);

    public static List<Window> GetOpenWindows()
    {
        List<Window> windows = [];
        EnumWindows((hWnd, lParam) =>
        {
            Rect rect;
            if (GetWindowRect(hWnd, out rect))
            {
                var window = new Window
                {
                    Handle = hWnd,
                    Bounds = rect,
                    Title = GetWindowTitle(hWnd),
                    ClassName = GetWindowClassName(hWnd),
                    IsVisible = IsWindowVisible(hWnd)
                };
                windows.Add(window);
            }
            return true;
        }, IntPtr.Zero);

        return windows;
    }

    private static string GetWindowTitle(IntPtr hWnd)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder(256);
        GetWindowText(hWnd, sb, sb.Capacity);
        return sb.ToString();
    }

    private static string GetWindowClassName(IntPtr hWnd)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder(256);
        GetClassName(hWnd, sb, sb.Capacity);
        return sb.ToString();
    }
}
