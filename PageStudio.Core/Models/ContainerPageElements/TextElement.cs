using PageStudio.Core.Interfaces;
using PageStudio.Core.Models.Abstractions;
using SkiaSharp;

namespace PageStudio.Core.Models.ContainerPageElements;

/// <summary>
/// Text element that can display text with customizable font and size
/// </summary>
public class TextElement : PageElement
{
    /// <summary>
    /// The text content to display
    /// </summary>
    public string Text { get; set; }
    
    /// <summary>
    /// Font family name
    /// </summary>
    public string FontFamily { get; set; }
    
    /// <summary>
    /// Font size in points
    /// </summary>
    public float FontSize { get; set; }
    
    /// <summary>
    /// Font style (Normal, Bold, Italic, etc.)
    /// </summary>
    public SKFontStyle FontStyle { get; set; }
    
    /// <summary>
    /// Text color
    /// </summary>
    public SKColor TextColor { get; set; }
    
    /// <summary>
    /// Text alignment
    /// </summary>
    public SKTextAlign TextAlign { get; set; }

    /// <summary>
    /// Initializes a new instance of TextElement
    /// </summary>
    /// <param name="text">Initial text content</param>
    /// <param name="fontFamily">Font family name</param>
    /// <param name="fontSize">Font size in points</param>
    public TextElement(string text = "Sample Text", string fontFamily = "Arial", float fontSize = 12.0f) 
        : base("Text Element")
    {
        Text = text;
        FontFamily = fontFamily;
        FontSize = fontSize;
        FontStyle = SKFontStyle.Normal;
        TextColor = SKColors.Black;
        TextAlign = SKTextAlign.Left;
        
        // Set default size based on text
        UpdateSizeFromText();
    }

    /// <summary>
    /// Renders the text element
    /// </summary>
    /// <param name="graphics">Graphics context for rendering</param>
    protected override void RenderSelf(IGraphicsContext graphics)
    {
        if (string.IsNullOrEmpty(Text))
            return;

        // Create paint for the text
        using var textPaint = new SKPaint
        {
            Color = TextColor,
            TextSize = FontSize,
            IsAntialias = true,
            TextAlign = TextAlign
        };

        // Create font with specified family and style
        using var typeface = SKTypeface.FromFamilyName(FontFamily, FontStyle);
        using var font = new SKFont(typeface, FontSize);

        // Calculate text position based on alignment
        float x = TextAlign switch
        {
            SKTextAlign.Center => (float)Width / 2,
            SKTextAlign.Right => (float)Width,
            _ => 0
        };

        // Draw the text at the calculated position
        // Y position is adjusted to account for font baseline
        float y = FontSize; // Simple baseline calculation
        
        graphics.DrawText(Text, x, y, textPaint, font, TextAlign);
    }

    /// <summary>
    /// Creates a deep copy of this text element
    /// </summary>
    /// <returns>Cloned text element</returns>
    public override IPageElement Clone()
    {
        var clone = new TextElement(Text, FontFamily, FontSize)
        {
            Name = Name,
            X = X,
            Y = Y,
            Width = Width,
            Height = Height,
            Rotation = Rotation,
            Opacity = Opacity,
            IsVisible = IsVisible,
            IsLocked = IsLocked,
            ZOrder = ZOrder,
            FontStyle = FontStyle,
            TextColor = TextColor,
            TextAlign = TextAlign
        };

        return clone;
    }

    /// <summary>
    /// Updates the element size based on text content
    /// </summary>
    private void UpdateSizeFromText()
    {
        if (string.IsNullOrEmpty(Text))
        {
            Width = 100;
            Height = FontSize * 1.2; // Default height with some padding
            return;
        }

        // Create temporary paint to measure text
        using var paint = new SKPaint
        {
            TextSize = FontSize
        };

        using var typeface = SKTypeface.FromFamilyName(FontFamily, FontStyle);
        using var font = new SKFont(typeface, FontSize);

        var textBounds = new SKRect();
        paint.MeasureText(Text, ref textBounds);

        Width = Math.Max(textBounds.Width + 10, 50); // Add some padding
        Height = Math.Max(FontSize * 1.2, 20); // Height based on font size with padding
    }

    /// <summary>
    /// Updates text content and recalculates size
    /// </summary>
    /// <param name="newText">New text content</param>
    public void UpdateText(string newText)
    {
        Text = newText;
        UpdateSizeFromText();
        UpdateModifiedTime();
    }

    /// <summary>
    /// Updates font properties and recalculates size
    /// </summary>
    /// <param name="fontFamily">Font family name</param>
    /// <param name="fontSize">Font size in points</param>
    /// <param name="fontStyle">Font style</param>
    public void UpdateFont(string fontFamily, float fontSize, SKFontStyle fontStyle = default)
    {
        FontFamily = fontFamily;
        FontSize = fontSize;
        FontStyle = fontStyle == default ? SKFontStyle.Normal : fontStyle;
        UpdateSizeFromText();
        UpdateModifiedTime();
    }
}