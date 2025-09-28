using SkiaSharp;
using PageStudio.Core.Interfaces;

namespace PageStudio.Core.Graphics;

/// <summary>
/// Implementation of IGraphicsContext interface providing graphics rendering operations using SkiaSharp
/// </summary>
public class GraphicsContext : IGraphicsContext
{
    private bool _disposed = false;

    /// <summary>
    /// The underlying SkiaSharp canvas
    /// </summary>
    public SKCanvas Canvas { get; }
    
    /// <summary>
    /// Current width of the drawing area
    /// </summary>
    public int Width { get; }
    
    /// <summary>
    /// Current height of the drawing area
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Initializes a new instance of GraphicsContext
    /// </summary>
    /// <param name="canvas">SkiaSharp canvas to wrap</param>
    /// <param name="width">Width of the drawing area</param>
    /// <param name="height">Height of the drawing area</param>
    public GraphicsContext(SKCanvas canvas, int width, int height)
    {
        Canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Saves the current graphics state
    /// </summary>
    public void Save()
    {
        ThrowIfDisposed();
        Canvas.Save();
    }

    /// <summary>
    /// Restores the previously saved graphics state
    /// </summary>
    public void Restore()
    {
        ThrowIfDisposed();
        Canvas.Restore();
    }

    /// <summary>
    /// Translates the coordinate system
    /// </summary>
    /// <param name="dx">X translation</param>
    /// <param name="dy">Y translation</param>
    public void Translate(float dx, float dy)
    {
        ThrowIfDisposed();
        Canvas.Translate(dx, dy);
    }

    /// <summary>
    /// Rotates the coordinate system
    /// </summary>
    /// <param name="degrees">Rotation angle in degrees</param>
    public void Rotate(float degrees)
    {
        ThrowIfDisposed();
        Canvas.RotateDegrees(degrees);
    }

    /// <summary>
    /// Scales the coordinate system
    /// </summary>
    /// <param name="sx">X scale factor</param>
    /// <param name="sy">Y scale factor</param>
    public void Scale(float sx, float sy)
    {
        ThrowIfDisposed();
        Canvas.Scale(sx, sy);
    }

    /// <summary>
    /// Sets the clipping rectangle
    /// </summary>
    /// <param name="rect">Clipping rectangle</param>
    public void ClipRect(SKRect rect)
    {
        ThrowIfDisposed();
        Canvas.ClipRect(rect);
    }

    /// <summary>
    /// Clears the drawing area with the specified color
    /// </summary>
    /// <param name="color">Clear color</param>
    public void Clear(SKColor color)
    {
        ThrowIfDisposed();
        Canvas.Clear(color);
    }

    /// <summary>
    /// Draws a rectangle
    /// </summary>
    /// <param name="rect">Rectangle bounds</param>
    /// <param name="paint">Paint style</param>
    public void DrawRect(SKRect rect, SKPaint paint)
    {
        ThrowIfDisposed();
        if (paint == null)
            throw new ArgumentNullException(nameof(paint));
        
        Canvas.DrawRect(rect, paint);
    }

    /// <summary>
    /// Draws a circle
    /// </summary>
    /// <param name="cx">Center X coordinate</param>
    /// <param name="cy">Center Y coordinate</param>
    /// <param name="radius">Circle radius</param>
    /// <param name="paint">Paint style</param>
    public void DrawCircle(float cx, float cy, float radius, SKPaint paint)
    {
        ThrowIfDisposed();
        if (paint == null)
            throw new ArgumentNullException(nameof(paint));
        
        Canvas.DrawCircle(cx, cy, radius, paint);
    }

    /// <summary>
    /// Draws text
    /// </summary>
    /// <param name="text">Text to draw</param>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="paint">Paint style</param>
    public void DrawText(string text, float x, float y, SKPaint paint)
    {
        ThrowIfDisposed();
        if (string.IsNullOrEmpty(text))
            return;
        if (paint == null)
            throw new ArgumentNullException(nameof(paint));
        
        Canvas.DrawText(text, x, y, paint);
    }

    /// <summary>
    /// Draws an image
    /// </summary>
    /// <param name="image">Image to draw</param>
    /// <param name="dest">Destination rectangle</param>
    /// <param name="paint">Optional paint for effects</param>
    public void DrawImage(SKImage image, SKRect dest, SKPaint? paint = null)
    {
        ThrowIfDisposed();
        if (image == null)
            throw new ArgumentNullException(nameof(image));
        
        Canvas.DrawImage(image, dest, paint);
    }

    /// <summary>
    /// Draws a path
    /// </summary>
    /// <param name="path">Path to draw</param>
    /// <param name="paint">Paint style</param>
    public void DrawPath(SKPath path, SKPaint paint)
    {
        ThrowIfDisposed();
        if (path == null)
            throw new ArgumentNullException(nameof(path));
        if (paint == null)
            throw new ArgumentNullException(nameof(paint));
        
        Canvas.DrawPath(path, paint);
    }

    /// <summary>
    /// Draws a rounded rectangle
    /// </summary>
    /// <param name="rect">Rectangle bounds</param>
    /// <param name="rx">X radius for rounded corners</param>
    /// <param name="ry">Y radius for rounded corners</param>
    /// <param name="paint">Paint style</param>
    public void DrawRoundRect(SKRect rect, float rx, float ry, SKPaint paint)
    {
        ThrowIfDisposed();
        if (paint == null)
            throw new ArgumentNullException(nameof(paint));
        
        Canvas.DrawRoundRect(rect, rx, ry, paint);
    }

    /// <summary>
    /// Draws an oval
    /// </summary>
    /// <param name="rect">Bounding rectangle for the oval</param>
    /// <param name="paint">Paint style</param>
    public void DrawOval(SKRect rect, SKPaint paint)
    {
        ThrowIfDisposed();
        if (paint == null)
            throw new ArgumentNullException(nameof(paint));
        
        Canvas.DrawOval(rect, paint);
    }

    /// <summary>
    /// Draws a line
    /// </summary>
    /// <param name="x0">Start X coordinate</param>
    /// <param name="y0">Start Y coordinate</param>
    /// <param name="x1">End X coordinate</param>
    /// <param name="y1">End Y coordinate</param>
    /// <param name="paint">Paint style</param>
    public void DrawLine(float x0, float y0, float x1, float y1, SKPaint paint)
    {
        ThrowIfDisposed();
        if (paint == null)
            throw new ArgumentNullException(nameof(paint));
        
        Canvas.DrawLine(x0, y0, x1, y1, paint);
    }

    /// <summary>
    /// Creates a graphics context from an SKBitmap
    /// </summary>
    /// <param name="bitmap">Bitmap to create context from</param>
    /// <returns>New graphics context</returns>
    public static GraphicsContext FromBitmap(SKBitmap bitmap)
    {
        if (bitmap == null)
            throw new ArgumentNullException(nameof(bitmap));

        var canvas = new SKCanvas(bitmap);
        return new GraphicsContext(canvas, bitmap.Width, bitmap.Height);
    }

    /// <summary>
    /// Creates a graphics context from an SKSurface
    /// </summary>
    /// <param name="surface">Surface to create context from</param>
    /// <returns>New graphics context</returns>
    public static GraphicsContext FromSurface(SKSurface surface)
    {
        if (surface == null)
            throw new ArgumentNullException(nameof(surface));

        return new GraphicsContext(surface.Canvas, (int)Math.Round(surface.Canvas.LocalClipBounds.Width), (int)Math.Round(surface.Canvas.LocalClipBounds.Height));
    }

    /// <summary>
    /// Throws ObjectDisposedException if the object has been disposed
    /// </summary>
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(GraphicsContext));
    }

    /// <summary>
    /// Disposes the graphics context
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            // Note: We don't dispose the Canvas here because it might be owned by someone else
            // The caller is responsible for disposing the SKCanvas if needed
            _disposed = true;
        }
    }
}