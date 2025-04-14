using Raylib_cs;

using System.Numerics;
using System.Runtime.InteropServices; // Required for Vector2

namespace DesktopBlocks;

internal static class Program
{
    static List<WindowInfo.Window> windows = [];
    static Texture2D screenshotTexture;
    static MonitorInfo.MonitorInfoEx primaryMonitor;
    static int windowWidth;
    static int windowHeight;
    static float scaleRatio; // Ratio to scale screen coordinates to window coordinates

    // Add physics simulation elements here (e.g., list of sand particles)

    static void Main()
    {
        // 1. Get Primary Monitor Info
        var monitors = MonitorInfo.GetMonitorsWithScale();
        // Assume primary is the one with the largest area
        (primaryMonitor, _) = monitors.FirstOrDefault(m => m.Monitor.IsPrimaryMonitor);

        if (primaryMonitor.Size == 0)
        {
            Console.WriteLine("Error: Could not find primary monitor info.");
            return; // Cannot proceed without monitor info
        }

        int monitorWidth = primaryMonitor.Monitor.Right - primaryMonitor.Monitor.Left;
        int monitorHeight = primaryMonitor.Monitor.Bottom - primaryMonitor.Monitor.Top;
        float aspectRatio = (float)monitorWidth / monitorHeight;

        // 2. Calculate Window Size (Maintain aspect ratio, max height 1440)
        const int maxHeight = 1080;
        windowHeight = Math.Min(monitorHeight, maxHeight);
        windowWidth = (int)(windowHeight * aspectRatio);
        scaleRatio = (float)windowHeight / monitorHeight; // How much we scaled down (or up)

        // 3. Take Screenshot using our custom method
        var screenImage = ScreenCapture.CaptureScreenToImage(primaryMonitor.Monitor);

        // Add debug info about captured image dimensions
        Console.WriteLine($"Captured image: {screenImage.Width}x{screenImage.Height}");
        Console.WriteLine($"Monitor dimensions: {monitorWidth}x{monitorHeight}");

        // 5. Initialize Raylib Window
        Raylib.InitWindow(windowWidth, windowHeight, "Desktop Blocks - Background");
        Raylib.SetTargetFPS(60);

        // 6. Create Texture from Image
        screenshotTexture = Raylib.LoadTextureFromImage(screenImage);

        // 7. Unload Image (no longer needed)
        Raylib.UnloadImage(screenImage);

        // Load initial window layout
        windows = WindowInfo.GetOpenWindows();

        while (!Raylib.WindowShouldClose())
        {
            // --- Draw ---
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black); // Clear with black in case texture doesn't draw

            // 8. Draw Background Screenshot (Scaled)
            Raylib.DrawTexturePro(
                screenshotTexture,
                new Rectangle(0, 0, screenshotTexture.Width, screenshotTexture.Height), // Use monitor dimensions as source
                new Rectangle(0, 0, windowWidth, windowHeight),   // Destination is window size
                Vector2.Zero,
                0f,
                Color.White
            );

            // 9. Draw Window Outlines (Scaled and Offset)
            foreach (var window in windows)
            {
                // Only draw visible windows that are at least partially on the primary monitor
                if (window.IsVisible &&
                    window.Bounds.Left < primaryMonitor.Monitor.Right &&
                    window.Bounds.Right > primaryMonitor.Monitor.Left &&
                    window.Bounds.Top < primaryMonitor.Monitor.Bottom &&
                    window.Bounds.Bottom > primaryMonitor.Monitor.Top)
                {
                    // Calculate window bounds relative to the primary monitor's top-left
                    float relativeX = window.Bounds.Left - primaryMonitor.Monitor.Left;
                    float relativeY = window.Bounds.Top - primaryMonitor.Monitor.Top;
                    float relativeWidth = window.Bounds.Right - window.Bounds.Left;
                    float relativeHeight = window.Bounds.Bottom - window.Bounds.Top;

                    // Scale these relative coordinates to fit the Raylib window
                    Rectangle windowDrawRect = new(
                        relativeX * scaleRatio,
                        relativeY * scaleRatio,
                        relativeWidth * scaleRatio,
                        relativeHeight * scaleRatio
                    );

                    // Clamp drawing to window bounds to avoid drawing outside
                    windowDrawRect.X = Math.Max(0, windowDrawRect.X);
                    windowDrawRect.Y = Math.Max(0, windowDrawRect.Y);
                    windowDrawRect.Width = Math.Min(windowWidth - windowDrawRect.X, windowDrawRect.Width);
                    windowDrawRect.Height = Math.Min(windowHeight - windowDrawRect.Y, windowDrawRect.Height);


                    if (windowDrawRect.Width > 0 && windowDrawRect.Height > 0)
                    {
                        Raylib.DrawRectangleLinesEx(windowDrawRect, 2, Color.Blue);

                        // Calculate text dimensions for background
                        int fontSize = (int)(10 * scaleRatio);
                        int textX = (int)windowDrawRect.X;
                        int textY = (int)windowDrawRect.Y - (int)(15 * scaleRatio);
                        int textWidth = Raylib.MeasureText(window.Title, fontSize);

                        // Draw white background for text
                        var padding = 2;
                        Raylib.DrawRectangle(
                            textX - padding,
                            textY - padding,
                            textWidth + padding + padding,
                            fontSize + padding + padding,
                            Color.White);

                        // Draw title text (same position as before)
                        Raylib.DrawText(window.Title, textX, textY, fontSize, Color.Black);
                    }
                }
            }

            // TODO: Draw sand particles (dynamic bodies)

            Raylib.DrawFPS(10, 10);

            // Debug info for scaling
            string debugInfo = $"Monitor: {monitorWidth}x{monitorHeight}, Window: {windowWidth}x{windowHeight}, Scale: {scaleRatio:F2}";
            Raylib.DrawText(debugInfo, 10, 30, 10, Color.Lime);

            //// Draw a grid to visualize scaling
            //for (int x = 0; x < windowWidth; x += 100)
            //{
            //    Raylib.DrawLine(x, 0, x, windowHeight, Color.Red);
            //}

            //for (int y = 0; y < windowHeight; y += 100)
            //{
            //    Raylib.DrawLine(0, y, windowWidth, y, Color.Red);
            //}

            Raylib.EndDrawing();
        }

        // 10. Cleanup
        Raylib.UnloadTexture(screenshotTexture);
        Raylib.CloseWindow();
    }
    // Add this method to save the screenshotTexture to an image file on disk
    private static void SaveTextureToImage(Texture2D texture, string filePath)
    {
        // Get the image from the texture
        Image image = Raylib.LoadImageFromTexture(texture);

        // Export the image to the specified file path
        Raylib.ExportImage(image, filePath);

        // Unload the image to free memory
        Raylib.UnloadImage(image);
    }
    private static unsafe Image BitmapToRaylibImage(System.Drawing.Bitmap bitmap)
    {
        // Lock the bitmap's bits
        System.Drawing.Imaging.BitmapData bmpData = bitmap.LockBits(
            new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
            System.Drawing.Imaging.ImageLockMode.ReadOnly,
            System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        // Create a Raylib image with the same dimensions
        Image image = new()
        {
            Width = bitmap.Width,
            Height = bitmap.Height,
            Mipmaps = 1,
            Format = PixelFormat.UncompressedR8G8B8A8
        };

        // Allocate memory for the image data
        int dataSize = bitmap.Width * bitmap.Height * 4; // 4 bytes per pixel (RGBA)
        image.Data = (void*)Marshal.AllocHGlobal(dataSize);

        // Copy the bitmap data to the image
        Buffer.MemoryCopy((void*)bmpData.Scan0, image.Data, dataSize, dataSize);

        // Swap the red and blue channels (BGRA -> RGBA)
        byte* pixelPtr = (byte*)image.Data;
        for (int i = 0; i < dataSize; i += 4)
        {
            byte temp = pixelPtr[i];             // Blue
            pixelPtr[i] = pixelPtr[i + 2];       // Red becomes Blue
            pixelPtr[i + 2] = temp;              // Blue becomes Red
        }

        // Unlock the bitmap
        bitmap.UnlockBits(bmpData);

        return image;
    }
}