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
        public IntPtr ParentHandle { get; set; }
        public Rect Bounds { get; set; }
        public required string Title { get; set; }
        public required string ClassName { get; set; }
        public bool IsVisible { get; set; }
        public int ZIndex { get; set; }
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

    [DllImport("user32.dll", ExactSpelling = true)]
    public static extern IntPtr GetAncestor(IntPtr hwnd, GetAncestorFlags flags);

    public enum GetAncestorFlags
    {
        GetParent = 1,
        GetRoot = 2,
        GetRootOwner = 3
    }

    public static List<Window> GetOpenWindows()
    {
        List<Window> windows = [];
        EnumWindows((hWnd, lParam) =>
        {
            Rect rect;
            if (GetWindowRect(hWnd, out rect))
            {
                string title = GetWindowTitle(hWnd);
                bool isVisible = IsWindowVisible(hWnd) &&
                                 rect.Right - rect.Left > 0 &&
                                 rect.Bottom - rect.Top > 0 &&
                                 !string.IsNullOrWhiteSpace(title);

                var window = new Window
                {
                    Handle = hWnd,
                    ParentHandle = GetAncestor(hWnd, GetAncestorFlags.GetParent),
                    Bounds = rect,
                    Title = title,
                    ClassName = GetWindowClassName(hWnd),
                    IsVisible = isVisible
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
