using PageStudio.Core.Interfaces;
using PageStudio.Core.Models.Abstractions;
using SkiaSharp;

namespace PageStudio.Core.Models.ContainerPageElements;

/// <summary>
/// Text element that can display text with customizable font and size
/// </summary>
public class TextElement : PageElement
{
    private const float FloatComparisonEpsilon = 0.01f;

    private string _text;

    /// <summary>
    /// The text content to display
    /// </summary>
    public string Text
    {
        get => _text;
        set
        {
            if (_text != value)
            {
                _text = value;
                UpdateSizeFromText();
                UpdateModifiedTime();
            }
        }
    }

    private string _fontFamily;

    /// <summary>
    /// Font family name
    /// </summary>
    public string FontFamily
    {
        get => _fontFamily;
        set
        {
            if (_fontFamily != value)
            {
                _fontFamily = value;
                UpdateSizeFromText();
                UpdateModifiedTime();
            }
        }
    }

    private float _fontSize;

    /// <summary>
    /// Font size in points
    /// </summary>
    public float FontSize
    {
        get => _fontSize;
        set
        {
            if (System.Math.Abs(_fontSize - value) > FloatComparisonEpsilon)
            {
                _fontSize = value;
                UpdateSizeFromText();
                UpdateModifiedTime();
            }
        }
    }

    private SKFontStyle _fontStyle;

    /// <summary>
    /// Font style (Normal, Bold, Italic, etc.)
    /// </summary>
    public SKFontStyle FontStyle
    {
        get => _fontStyle;
        set
        {
            if (_fontStyle != value)
            {
                _fontStyle = value;
                UpdateSizeFromText();
                UpdateModifiedTime();
            }
        }
    }

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
    public TextElement(IPage page, string text = "Sample Text", string fontFamily = "Arial", float fontSize = 12.0f)
        : base(page, "Text Element")
    {
        _text = text;
        _fontFamily = fontFamily;
        _fontSize = fontSize;
        _fontStyle = SKFontStyle.Normal;
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
            IsAntialias = true,
            TextAlign = TextAlign
        };

        // Create font with specified family and style
        using var typeface = SKTypeface.FromFamilyName(FontFamily, FontStyle);
        using var font = new SKFont(typeface, FontSize);

        // Calculate text position based on alignment
        var x = TextAlign switch
        {
            SKTextAlign.Center => (float)Width / 2,
            SKTextAlign.Right => (float)Width,
            _ => 0
        };

        // Draw the text at the calculated position
        // Y position is adjusted to account for font baseline
        var y = FontSize; // Simple baseline calculation

        graphics.DrawText(Text, x, y, textPaint, font, TextAlign);
    }

    /// <summary>
    /// Creates a deep copy of this text element
    /// </summary>
    /// <returns>Cloned text element</returns>
    public override IPageElement Clone()
    {
        var clone = new TextElement(this.Page, Text, FontFamily, FontSize)
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
            Height = FontSize;
            return;
        }

        // Create temporary paint to measure text
        using var paint = new SKPaint();
        using var typeface = SKTypeface.FromFamilyName(FontFamily, FontStyle);
        using var font = new SKFont(typeface, FontSize);

        font.MeasureText(Text, out var textBounds, paint);

        Width = textBounds.Width;
        Height = textBounds.Height;
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
    public void UpdateFont(string fontFamily, float fontSize, SKFontStyle? fontStyle = null)
    {
        FontFamily = fontFamily;
        FontSize = fontSize;
        FontStyle = fontStyle ?? SKFontStyle.Normal;
        UpdateSizeFromText();
        UpdateModifiedTime();
    }
}