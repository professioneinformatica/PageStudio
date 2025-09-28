using PageStudio.Core.Interfaces;

namespace PageStudio.Core.Export;

/// <summary>
/// Base implementation of IExportOptions interface
/// </summary>
public class ExportOptions : IExportOptions
{
    /// <summary>
    /// Output quality (0.0 to 1.0)
    /// </summary>
    public double Quality { get; set; } = 1.0;
    
    /// <summary>
    /// Export resolution in DPI
    /// </summary>
    public int Resolution { get; set; } = 300;
    
    /// <summary>
    /// Whether to include metadata in the export
    /// </summary>
    public bool IncludeMetadata { get; set; } = true;
}

/// <summary>
/// Implementation of IPdfExportOptions interface for PDF export settings
/// </summary>
public class PdfExportOptions : ExportOptions, IPdfExportOptions
{
    /// <summary>
    /// PDF version to use
    /// </summary>
    public string PdfVersion { get; set; } = "1.4";
    
    /// <summary>
    /// Whether to embed fonts
    /// </summary>
    public bool EmbedFonts { get; set; } = true;
    
    /// <summary>
    /// Whether to compress images
    /// </summary>
    public bool CompressImages { get; set; } = true;
    
    /// <summary>
    /// Image compression quality (0.0 to 1.0)
    /// </summary>
    public double ImageCompressionQuality { get; set; } = 0.85;
    
    /// <summary>
    /// Whether to create PDF/A compliant document
    /// </summary>
    public bool CreatePdfA { get; set; } = false;
    
    /// <summary>
    /// Document title for PDF metadata
    /// </summary>
    public string? Title { get; set; }
    
    /// <summary>
    /// Document author for PDF metadata
    /// </summary>
    public string? Author { get; set; }
    
    /// <summary>
    /// Document subject for PDF metadata
    /// </summary>
    public string? Subject { get; set; }
    
    /// <summary>
    /// Document keywords for PDF metadata
    /// </summary>
    public string? Keywords { get; set; }
}

/// <summary>
/// Implementation of IImageExportOptions interface for image export settings
/// </summary>
public class ImageExportOptions : ExportOptions, IImageExportOptions
{
    /// <summary>
    /// Image format (PNG, JPG, etc.)
    /// </summary>
    public string Format { get; set; } = "PNG";
    
    /// <summary>
    /// Image width in pixels (if different from document size)
    /// </summary>
    public int? Width { get; set; }
    
    /// <summary>
    /// Image height in pixels (if different from document size)
    /// </summary>
    public int? Height { get; set; }
    
    /// <summary>
    /// Background color for transparent areas
    /// </summary>
    public string? BackgroundColor { get; set; } = "#FFFFFF";
    
    /// <summary>
    /// Whether to maintain aspect ratio when resizing
    /// </summary>
    public bool MaintainAspectRatio { get; set; } = true;
    
    /// <summary>
    /// Anti-aliasing settings
    /// </summary>
    public bool AntiAliasing { get; set; } = true;
    
    /// <summary>
    /// Color profile to use
    /// </summary>
    public string? ColorProfile { get; set; } = "sRGB";
}