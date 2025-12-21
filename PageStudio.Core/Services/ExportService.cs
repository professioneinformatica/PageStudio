using System.Collections.Generic;
using SkiaSharp;
using PageStudio.Core.Interfaces;
using PageStudio.Core.Graphics;
using PageStudio.Core.Models.Documents;
using PageStudio.Core.Models.Page;

namespace PageStudio.Core.Services;

/// <summary>
/// Implementation of IExportService interface for exporting documents to various formats
/// </summary>
public class ExportService : IExportService
{
    private readonly string[] _supportedFormats = { "PDF", "PNG", "JPG", "JPEG", "BMP", "WEBP" };

    /// <summary>
    /// Exports document to PDF format
    /// </summary>
    /// <param name="document">Document to export</param>
    /// <param name="filePath">Output file path</param>
    /// <param name="options">Export options</param>
    /// <returns>Task representing the export operation</returns>
    public async Task<bool> ExportToPdfAsync(IDocument document, string filePath, IPdfExportOptions? options = null)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        try
        {
            return await Task.Run(() => ExportToPdf(document, filePath, options));
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Exports document to image format (PNG, JPG)
    /// </summary>
    /// <param name="document">Document to export</param>
    /// <param name="filePath">Output file path</param>
    /// <param name="options">Export options</param>
    /// <returns>Task representing the export operation</returns>
    public async Task<bool> ExportToImageAsync(IDocument document, string filePath, IImageExportOptions? options = null)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        try
        {
            return await Task.Run(() => ExportToImage(document, filePath, options));
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Exports a specific page to image format
    /// </summary>
    /// <param name="page">Page to export</param>
    /// <param name="filePath">Output file path</param>
    /// <param name="options">Export options</param>
    /// <returns>Task representing the export operation</returns>
    public async Task<bool> ExportPageToImageAsync(IPage page, string filePath, IImageExportOptions? options = null)
    {
        if (page == null)
            throw new ArgumentNullException(nameof(page));
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        try
        {
            return await Task.Run(() => ExportPageToImage(page, filePath, options));
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Gets supported export formats
    /// </summary>
    /// <returns>List of supported formats</returns>
    public IEnumerable<string> GetSupportedFormats()
    {
        return _supportedFormats;
    }

    /// <summary>
    /// Synchronous PDF export implementation
    /// </summary>
    private bool ExportToPdf(IDocument document, string filePath, IPdfExportOptions? options)
    {
        if (document.Pages.Count == 0)
            return false;

        var pdfOptions = options ?? new Export.PdfExportOptions();
        
        using var stream = new SKFileWStream(filePath);
        using var pdfDocument = SKDocument.CreatePdf(stream);

        foreach (var page in document.Pages)
        {
            var canvas = pdfDocument.BeginPage((float)page.Width, (float)page.Height);
            using var graphics = new GraphicsContext(canvas, (int)page.Width, (int)page.Height);
            
            // Clear background
            if (page.Background != null)
            {
                // Handle background color/pattern
                var backgroundColor = ParseBackgroundColor(page.Background);
                if (backgroundColor.HasValue)
                    graphics.Clear(backgroundColor.Value);
                else
                    graphics.Clear(SKColors.White);
            }
            else
            {
                graphics.Clear(SKColors.White);
            }

            // Render all elements sorted by layer Z-index then element Z-order
            var sortedElements = page.GetAllElementsByRenderOrder();
            foreach (var element in sortedElements)
            {
                if (element.IsVisible.Value)
                {
                    element.Render(graphics);
                }
            }

            pdfDocument.EndPage();
        }

        pdfDocument.Close();
        return true;
    }

    /// <summary>
    /// Synchronous image export implementation for document
    /// </summary>
    private bool ExportToImage(IDocument document, string filePath, IImageExportOptions? options)
    {
        if (document.Pages.Count == 0)
            return false;

        // Export first page if multiple pages exist
        var page = document.Pages[0];
        return ExportPageToImage(page, filePath, options);
    }

    /// <summary>
    /// Synchronous image export implementation for page
    /// </summary>
    private bool ExportPageToImage(IPage page, string filePath, IImageExportOptions? options)
    {
        var imageOptions = options ?? new Export.ImageExportOptions();
        
        // Determine image dimensions
        var width = imageOptions.Width ?? (int)(page.Width * imageOptions.Resolution / 72.0);
        var height = imageOptions.Height ?? (int)(page.Height * imageOptions.Resolution / 72.0);

        if (imageOptions.MaintainAspectRatio && imageOptions.Width.HasValue && imageOptions.Height.HasValue)
        {
            var aspectRatio = page.Width / page.Height;
            if (width / aspectRatio > height)
            {
                width = (int)(height * aspectRatio);
            }
            else
            {
                height = (int)(width / aspectRatio);
            }
        }

        // Create bitmap and canvas
        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        using var graphics = new GraphicsContext(canvas, width, height);

        // Scale to fit the page
        var scaleX = (float)width / (float)page.Width;
        var scaleY = (float)height / (float)page.Height;
        graphics.Scale(scaleX, scaleY);

        // Clear background
        var backgroundColor = ParseBackgroundColor(page.Background) ?? ParseBackgroundColor(imageOptions.BackgroundColor) ?? SKColors.White;
        graphics.Clear(backgroundColor);

        // Render all elements sorted by layer Z-index then element Z-order
        var sortedElements = page.GetAllElementsByRenderOrder();
        foreach (var element in sortedElements)
        {
            if (element.IsVisible.Value)
            {
                element.Render(graphics);
            }
        }

        // Save to file
        var format = GetSkiaImageFormat(imageOptions.Format);
        var quality = (int)(imageOptions.Quality * 100);

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(format, quality);
        using var stream = File.OpenWrite(filePath);
        data.SaveTo(stream);

        return true;
    }

    /// <summary>
    /// Parses background color from various input formats
    /// </summary>
    private SKColor? ParseBackgroundColor(object? background)
    {
        if (background == null)
            return null;

        if (background is string colorString)
        {
            if (SKColor.TryParse(colorString, out var color))
                return color;
        }
        else if (background is SKColor skColor)
        {
            return skColor;
        }

        return null;
    }

    /// <summary>
    /// Gets SkiaSharp image format from string
    /// </summary>
    private SKEncodedImageFormat GetSkiaImageFormat(string format)
    {
        return format.ToUpperInvariant() switch
        {
            "PNG" => SKEncodedImageFormat.Png,
            "JPG" or "JPEG" => SKEncodedImageFormat.Jpeg,
            "BMP" => SKEncodedImageFormat.Bmp,
            "WEBP" => SKEncodedImageFormat.Webp,
            _ => SKEncodedImageFormat.Png
        };
    }
}