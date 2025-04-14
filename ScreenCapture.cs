using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Runtime.InteropServices;
using Raylib_cs;
using Image = Raylib_cs.Image;
using PixelFormat = Raylib_cs.PixelFormat;
using Rectangle = Raylib_cs.Rectangle;

namespace DesktopBlocks;

public static class ScreenCapture
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

    [DllImport("gdi32.dll")]
    private static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int width, int height, IntPtr hdcSource, int xSrc, int ySrc, int rop);

    private const int SRCCOPY = 0xCC0020;
    private const int CAPTUREBLT = 0x40000000;

    public static Image CaptureScreenToImage(MonitorInfo.Rect monitorBounds)
    {
        int width = monitorBounds.Right - monitorBounds.Left;
        int height = monitorBounds.Bottom - monitorBounds.Top;

        // Create a bitmap to store the screen capture
        using Bitmap bitmap = new(width, height);
        using (var graphics = Graphics.FromImage(bitmap))
        {
            // Get a device context for the entire screen
            IntPtr hdcScreen = GetDC(IntPtr.Zero);
            IntPtr hdcMemory = graphics.GetHdc();

            // Copy the screen to our bitmap
            BitBlt(hdcMemory, 0, 0, width, height, hdcScreen, monitorBounds.Left, monitorBounds.Top, SRCCOPY | CAPTUREBLT);

            // Release the device context
            graphics.ReleaseHdc(hdcMemory);
            ReleaseDC(IntPtr.Zero, hdcScreen);
        }

        // Convert Bitmap to Raylib Image
        return BitmapToRaylibImage(bitmap);
    }

    private static unsafe Image BitmapToRaylibImage(Bitmap bitmap)
    {
        // Lock the bitmap's bits
        BitmapData bmpData = bitmap.LockBits(
            new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), 
            ImageLockMode.ReadOnly,
            bitmap.PixelFormat);
        
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
        Buffer.MemoryCopy((void*)bmpData.Scan0, (void*)image.Data, dataSize, dataSize);

        // Swap red and blue channels: converting BGRA -> RGBA
        byte* pixelPtr = (byte*)image.Data;
        for (int i = 0; i < dataSize; i += 4)
        {
            byte temp = pixelPtr[i];              // Blue
            pixelPtr[i] = pixelPtr[i + 2];        // Red becomes Blue
            pixelPtr[i + 2] = temp;               // Blue becomes Red
        }
        // Unlock the bitmap
        bitmap.UnlockBits(bmpData);
        
        return image;
    }

    public static void AdjustWindowSize(int monitorHeight, int monitorWidth, int maxHeight, out int windowHeight, out int windowWidth, out float scaleRatio)
    {
        float aspectRatio = (float)monitorWidth / monitorHeight;
        windowHeight = Math.Min(monitorHeight, maxHeight);
        windowWidth = (int)(windowHeight * aspectRatio);
        scaleRatio = (float)windowHeight / monitorHeight;
    }

    public static void DrawScreenshotTexture(Texture2D screenshotTexture, int windowWidth, int windowHeight)
    {
        Raylib.DrawTexturePro(
            screenshotTexture,
            new Rectangle(0, 0, screenshotTexture.Width, screenshotTexture.Height), // Source rect (full texture)
            new Rectangle(0, 0, windowWidth, windowHeight), // Destination rect (full window)
            Vector2.Zero, // Origin
            0f, // Rotation
            Raylib_cs.Color.White); // Tint
    }
}