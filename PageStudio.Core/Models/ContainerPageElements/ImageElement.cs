using PageStudio.Core.Features.EventsManagement;
using PageStudio.Core.Interfaces;
using PageStudio.Core.Models.Abstractions;
using PageStudio.Core.Models.Page;
using SkiaSharp;

namespace PageStudio.Core.Models.ContainerPageElements;

/// <summary>
/// Elemento pagina che visualizza un'immagine caricata
/// </summary>
public class ImageElement : PageElement
{
    private readonly IEventPublisher _eventPublisher;

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
    public ImageElement(IEventPublisher eventPublisher, IPage page, string? imageBase64 = null) : base(eventPublisher, page, "Image Element")
    {
        _eventPublisher = eventPublisher;
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
        var destRect = new SKRect(0, 0, (float)Width.Value, (float)Height.Value);
        using var image = SKImage.FromBitmap(_bitmap);
        graphics.DrawImage(image, destRect);
    }

    /// <summary>
    /// Clona l'elemento immagine
    /// </summary>
    public override IPageElement Clone()
    {
        var clone = new ImageElement(_eventPublisher, this.Page, ImageBase64)
        {
            Name = Name,
            ZIndex = ZIndex,
            LockAspectRatio = LockAspectRatio
        };
        
        clone.X.Formula = X.Formula;
        clone.Y.Formula = Y.Formula;
        clone.Width.Formula = Width.Formula;
        clone.Height.Formula = Height.Formula;
        clone.Rotation.Formula = Rotation.Formula;
        clone.Opacity.Formula = Opacity.Formula;
        clone.IsVisible.Formula = IsVisible.Formula;
        clone.IsLocked.Formula = IsLocked.Formula;
        
        return clone;
    }
}