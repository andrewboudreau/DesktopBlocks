using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace DesktopBlocks;

class WireframeRenderer
{
    public static void RenderWireframe(List<MonitorInfo.MonitorInfoEx> monitors, List<WindowInfo.Window> windows, Graphics g, int width, int height, WindowInfo.Window? selectedWindow)
    {
        g.Clear(Color.White);

        // Draw windows with window names
        foreach (var window in windows)
        {
            if (window.IsVisible)
            {
                Pen windowPen = (window.ClassName == selectedWindow?.ClassName) ? new Pen(Color.Red, 3) : Pens.Blue;
                g.DrawRectangle(windowPen, window.Bounds.Left, window.Bounds.Top, window.Bounds.Right - window.Bounds.Left, window.Bounds.Bottom - window.Bounds.Top);

                string windowName = window.Title.Length > 30 ? window.Title[..30] : window.Title;
                g.DrawString(windowName, SystemFonts.DefaultFont, Brushes.Black, window.Bounds.Left, window.Bounds.Top);
            }
        }

        // Draw monitors
        foreach (var monitor in monitors)
        {
            g.DrawRectangle(Pens.Red, monitor.Monitor.Left, monitor.Monitor.Top, monitor.Monitor.Right - monitor.Monitor.Left, monitor.Monitor.Bottom - monitor.Monitor.Top);
        }
    }
}
