using PageStudio.Core.Interfaces;
using PageStudio.Core.Models.Abstractions;
using SkiaSharp;

namespace PageStudio.Core.Models.ContainerPageElements;

/// <summary>
/// Elemento pagina che visualizza un'immagine caricata
/// </summary>
public class ImageElement : PageElement
{
    /// <summary>
    /// Dati immagine in formato base64 (PNG/JPEG)
    /// </summary>
    public string? ImageBase64 { get; set; }

    /// <summary>
    /// Bitmap decodificata (non serializzata)
    /// </summary>
    [NonSerialized] private SKBitmap? _bitmap;

    /// <summary>
    /// Costruttore
    /// </summary>
    /// <param name="page"></param>
    /// <param name="imageBase64">Dati immagine base64</param>
    /// <param name="mediator"></param>
    public ImageElement(IPage page, string? imageBase64 = null) : base(page, "Image Element")
    {
        ImageBase64 = imageBase64;
        SetDimension(200, 150);

        if (!string.IsNullOrEmpty(imageBase64))
            LoadBitmapFromBase64(imageBase64);
    }

    /// <summary>
    /// Carica la bitmap dai dati base64
    /// </summary>
    /// <param name="base64">Dati immagine base64</param>
    public void LoadBitmapFromBase64(string base64)
    {
        ImageBase64 = base64;
        try
        {
            var bytes = Convert.FromBase64String(base64);
            _bitmap = SKBitmap.Decode(bytes);
            if (_bitmap != null)
            {
                SetDimension(_bitmap.Width, _bitmap.Height);
            }
        }
        catch
        {
            _bitmap = null;
        }

        UpdateModifiedTime();
    }

    /// <summary>
    /// Renderizza l'immagine
    /// </summary>
    /// <param name="graphics">Contesto grafico</param>
    protected override void RenderSelf(IGraphicsContext graphics)
    {
        if (_bitmap == null && !string.IsNullOrEmpty(ImageBase64))
            LoadBitmapFromBase64(ImageBase64);
        if (_bitmap == null)
            return;
        var destRect = new SKRect(0, 0, (float)Width, (float)Height);
        using var image = SKImage.FromBitmap(_bitmap);
        graphics.DrawImage(image, destRect);
    }

    /// <summary>
    /// Clona l'elemento immagine
    /// </summary>
    public override IPageElement Clone()
    {
        var clone = new ImageElement(this.Page, ImageBase64)
        {
            Name = Name,
            X = X,
            Y = Y,
            Rotation = Rotation,
            Opacity = Opacity,
            IsVisible = IsVisible,
            IsLocked = IsLocked,
            ZOrder = ZOrder
        };
        clone.SetDimension(this.Width, this.Height);
        return clone;
    }
}