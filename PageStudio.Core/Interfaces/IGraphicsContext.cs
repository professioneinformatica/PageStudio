using SkiaSharp;

namespace PageStudio.Core.Interfaces;

/// <summary>
/// Graphics context interface for rendering operations
/// </summary>
public interface IGraphicsContext : IDisposable
{
    /// <summary>
    /// The underlying SkiaSharp canvas
    /// </summary>
    SKCanvas Canvas { get; }
    
    /// <summary>
    /// Current width of the drawing area
    /// </summary>
    int Width { get; }
    
    /// <summary>
    /// Current height of the drawing area
    /// </summary>
    int Height { get; }
    
    /// <summary>
    /// Saves the current graphics state
    /// </summary>
    void Save();
    
    /// <summary>
    /// Restores the previously saved graphics state
    /// </summary>
    void Restore();
    
    /// <summary>
    /// Translates the coordinate system
    /// </summary>
    /// <param name="dx">X translation</param>
    /// <param name="dy">Y translation</param>
    void Translate(float dx, float dy);
    
    /// <summary>
    /// Rotates the coordinate system
    /// </summary>
    /// <param name="degrees">Rotation angle in degrees</param>
    void Rotate(float degrees);
    
    /// <summary>
    /// Scales the coordinate system
    /// </summary>
    /// <param name="sx">X scale factor</param>
    /// <param name="sy">Y scale factor</param>
    void Scale(float sx, float sy);
    
    /// <summary>
    /// Sets the clipping rectangle
    /// </summary>
    /// <param name="rect">Clipping rectangle</param>
    void ClipRect(SKRect rect);
    
    /// <summary>
    /// Clears the drawing area with the specified color
    /// </summary>
    /// <param name="color">Clear color</param>
    void Clear(SKColor color);
    
    /// <summary>
    /// Draws a rectangle
    /// </summary>
    /// <param name="rect">Rectangle bounds</param>
    /// <param name="paint">Paint style</param>
    void DrawRect(SKRect rect, SKPaint paint);
    
    /// <summary>
    /// Draws a circle
    /// </summary>
    /// <param name="cx">Center X coordinate</param>
    /// <param name="cy">Center Y coordinate</param>
    /// <param name="radius">Circle radius</param>
    /// <param name="paint">Paint style</param>
    void DrawCircle(float cx, float cy, float radius, SKPaint paint);

    /// <summary>
    /// Draws text
    /// </summary>
    /// <param name="text">Text to draw</param>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="paint">Paint style</param>
    /// <param name="align">Alignement</param>
    /// <param name="font">Font</param>
    void DrawText(string text, float x, float y, SKPaint paint , SKFont? font, SKTextAlign align = SKTextAlign.Left);
    
    /// <summary>
    /// Draws an image
    /// </summary>
    /// <param name="image">Image to draw</param>
    /// <param name="dest">Destination rectangle</param>
    /// <param name="paint">Optional paint for effects</param>
    void DrawImage(SKImage image, SKRect dest, SKPaint? paint = null);
    
    /// <summary>
    /// Draws a path
    /// </summary>
    /// <param name="path">Path to draw</param>
    /// <param name="paint">Paint style</param>
    void DrawPath(SKPath path, SKPaint paint);
}