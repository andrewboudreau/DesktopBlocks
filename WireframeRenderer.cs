﻿using System.Threading;

namespace DesktopBlocks;

class WireframeRenderer
{
    public static void RenderWireframe(List<MonitorInfo.MonitorInfoEx> monitors, List<WindowInfo.Window> windows, PictureBox pictureBox)
    {
        int width = 0;
        int height = 0;

        foreach (var monitor in monitors)
        {
            width = Math.Max(width, monitor.Monitor.Right);
            height = Math.Max(height, monitor.Monitor.Bottom);
        }
        Bitmap bitmap = new(width, height);
        using (Graphics g = Graphics.FromImage(bitmap))
        {
            g.Clear(Color.White);
            g.DrawRectangle(Pens.Blue, 10, 10, 50, 50);

            // Draw monitors
            foreach (var monitor in monitors)
            {
                g.DrawRectangle(Pens.Blue, monitor.Monitor.Left, monitor.Monitor.Top, monitor.Monitor.Right - monitor.Monitor.Left, monitor.Monitor.Bottom - monitor.Monitor.Top);
            }

            // Draw windows with z-order
            int zIndex = 0;
            foreach (var window in windows)
            {
                g.DrawRectangle(Pens.Red, window.Bounds.Left, window.Bounds.Top, window.Bounds.Right - window.Bounds.Left, window.Bounds.Bottom - window.Bounds.Top);
                g.DrawString(zIndex.ToString(), SystemFonts.DefaultFont, Brushes.Black, window.Bounds.Left, window.Bounds.Top);
                zIndex++;
            }
        }

        pictureBox.Image = bitmap;
    }
}
