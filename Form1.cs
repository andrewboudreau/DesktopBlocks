namespace DesktopBlocks
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Load += Draw;
            DoubleClick += Draw;
            pictureBox1.DoubleClick += Draw;
        }

        private void Draw(object? sender, EventArgs e)
        {
            var monitors = MonitorInfo.GetMonitors();
            var windows = WindowInfo.GetOpenWindows();

            WireframeRenderer.RenderWireframe(monitors, windows, pictureBox1);
        }
    }
}
