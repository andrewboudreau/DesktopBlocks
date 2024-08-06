using System.Drawing.Drawing2D;

namespace DesktopBlocks
{
    public partial class Form1 : Form
    {
        private List<MonitorInfo.MonitorInfoEx> monitors;
        private List<WindowInfo.Window> windows;
        private float zoomFactor = 1.0f;
        private PointF zoomCenter = new PointF(0, 0);
        private WindowInfo.Window? selectedWindow;

        public Form1()
        {
            InitializeComponent();
            Load += Draw;
            DoubleClick += Draw;
            pictureBox1.DoubleClick += Draw;
            Resize += Form1_Resize;
            pictureBox1.MouseClick += PictureBox1_MouseClick;
            InitializeWindowTable();
        }

        private void InitializeWindowTable()
        {
            DataGridView windowTable = new DataGridView
            {
                Dock = DockStyle.Bottom,
                Height = 200,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
            };
            windowTable.Columns.Add("Title", "Window Name");
            windowTable.Columns.Add("X", "X");
            windowTable.Columns.Add("Y", "Y");
            windowTable.Columns.Add("ZIndex", "Z-Index");
            windowTable.Columns.Add("Parent", "Parent Window");
            windowTable.SelectionChanged += WindowTable_SelectionChanged;
            Controls.Add(windowTable);
        }

        private void WindowTable_SelectionChanged(object? sender, EventArgs e)
        {
            if (sender is DataGridView dgv && dgv.SelectedRows.Count > 0)
            {
                int selectedIndex = dgv.SelectedRows[0].Index;
                selectedWindow = windows[selectedIndex];
                RenderScaledWireframe();
            }
        }

        private void Draw(object? sender, EventArgs e)
        {
            monitors = MonitorInfo.GetMonitors();
            windows = WindowInfo.GetOpenWindows();
            for (int i = 0; i < windows.Count; i++)
            {
                windows[i].ZIndex = i;
            }
            UpdateWindowTable();
            RenderScaledWireframe();
        }

        private void UpdateWindowTable()
        {
            if (Controls.OfType<DataGridView>().FirstOrDefault() is DataGridView windowTable)
            {
                windowTable.Rows.Clear();
                foreach (var window in windows.Where(w => w.IsVisible))
                {
                    string parentTitle = windows.FirstOrDefault(w => w.Handle == window.ParentHandle)?.Title ?? "None";
                    windowTable.Rows.Add(window.Title, window.Bounds.Left, window.Bounds.Top, window.ZIndex, parentTitle);
                }
            }
        }

        private void Form1_Resize(object? sender, EventArgs e)
        {
            if (monitors != null && windows != null)
            {
                RenderScaledWireframe();
            }
        }

        private void PictureBox1_MouseClick(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                zoomFactor *= 1.2f;
            }
            else if (e.Button == MouseButtons.Right)
            {
                zoomFactor /= 1.2f;
            }

            zoomCenter = new PointF(e.X, e.Y);
            RenderScaledWireframe();
        }

        private void RenderScaledWireframe()
        {
            int totalWidth = monitors.Max(m => m.Monitor.Right);
            int totalHeight = monitors.Max(m => m.Monitor.Bottom);
            float baseScaleFactor = Math.Min((float)pictureBox1.Width / totalWidth, (float)pictureBox1.Height / totalHeight);
            float scaleFactor = baseScaleFactor * zoomFactor;

            Bitmap scaledBitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            using (Graphics g = Graphics.FromImage(scaledBitmap))
            {
                g.Clear(Color.White);
                g.TranslateTransform(zoomCenter.X, zoomCenter.Y);
                g.ScaleTransform(scaleFactor, scaleFactor);
                g.TranslateTransform(-zoomCenter.X / baseScaleFactor, -zoomCenter.Y / baseScaleFactor);

                WireframeRenderer.RenderWireframe(monitors, windows, g, totalWidth, totalHeight, selectedWindow);
            }

            pictureBox1.Image = scaledBitmap;
        }
    }
}
