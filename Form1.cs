using System.Drawing.Drawing2D;

namespace DesktopBlocks
{
    public partial class Form1 : Form
    {
        private List<MonitorInfo.MonitorInfoEx> monitors;
        private List<WindowInfo.Window> windows;

        public Form1()
        {
            InitializeComponent();
            Load += Draw;
            DoubleClick += Draw;
            pictureBox1.DoubleClick += Draw;
            Resize += Form1_Resize;
        }

        private void Draw(object? sender, EventArgs e)
        {
            monitors = MonitorInfo.GetMonitors();
            windows = WindowInfo.GetOpenWindows();
            RenderScaledWireframe();
        }

        private void Form1_Resize(object? sender, EventArgs e)
        {
            if (monitors != null && windows != null)
            {
                RenderScaledWireframe();
            }
        }

        private void RenderScaledWireframe()
        {
            int totalWidth = monitors.Max(m => m.Monitor.Right);
            int totalHeight = monitors.Max(m => m.Monitor.Bottom);
            float scaleFactor = Math.Min((float)pictureBox1.Width / totalWidth, (float)pictureBox1.Height / totalHeight);

            Bitmap scaledBitmap = new Bitmap((int)(totalWidth * scaleFactor), (int)(totalHeight * scaleFactor));
            using (Graphics g = Graphics.FromImage(scaledBitmap))
            {
                g.Clear(Color.White);
                g.ScaleTransform(scaleFactor, scaleFactor);

                WireframeRenderer.RenderWireframe(monitors, windows, g, totalWidth, totalHeight);
            }

            pictureBox1.Image = scaledBitmap;
        }
    }
}
