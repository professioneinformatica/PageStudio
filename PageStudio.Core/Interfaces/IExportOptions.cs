namespace PageStudio.Core.Interfaces;

/// <summary>
/// Base interface for export options
/// </summary>
public interface IExportOptions
{
    /// <summary>
    /// Output quality (0.0 to 1.0)
    /// </summary>
    double Quality { get; set; }
    
    /// <summary>
    /// Export resolution in DPI
    /// </summary>
    int Resolution { get; set; }
    
    /// <summary>
    /// Whether to include metadata in the export
    /// </summary>
    bool IncludeMetadata { get; set; }
}

/// <summary>
/// Export options specific to PDF format
/// </summary>
public interface IPdfExportOptions : IExportOptions
{
    /// <summary>
    /// PDF version to use
    /// </summary>
    string PdfVersion { get; set; }
    
    /// <summary>
    /// Whether to embed fonts
    /// </summary>
    bool EmbedFonts { get; set; }
    
    /// <summary>
    /// Whether to compress images
    /// </summary>
    bool CompressImages { get; set; }
    
    /// <summary>
    /// Image compression quality (0.0 to 1.0)
    /// </summary>
    double ImageCompressionQuality { get; set; }
    
    /// <summary>
    /// Whether to create PDF/A compliant document
    /// </summary>
    bool CreatePdfA { get; set; }
    
    /// <summary>
    /// Document title for PDF metadata
    /// </summary>
    string? Title { get; set; }
    
    /// <summary>
    /// Document author for PDF metadata
    /// </summary>
    string? Author { get; set; }
    
    /// <summary>
    /// Document subject for PDF metadata
    /// </summary>
    string? Subject { get; set; }
    
    /// <summary>
    /// Document keywords for PDF metadata
    /// </summary>
    string? Keywords { get; set; }
}

/// <summary>
/// Export options specific to image formats (PNG, JPG)
/// </summary>
public interface IImageExportOptions : IExportOptions
{
    /// <summary>
    /// Image format (PNG, JPG, etc.)
    /// </summary>
    string Format { get; set; }
    
    /// <summary>
    /// Image width in pixels (if different from document size)
    /// </summary>
    int? Width { get; set; }
    
    /// <summary>
    /// Image height in pixels (if different from document size)
    /// </summary>
    int? Height { get; set; }
    
    /// <summary>
    /// Background color for transparent areas
    /// </summary>
    string? BackgroundColor { get; set; }
    
    /// <summary>
    /// Whether to maintain aspect ratio when resizing
    /// </summary>
    bool MaintainAspectRatio { get; set; }
    
    /// <summary>
    /// Anti-aliasing settings
    /// </summary>
    bool AntiAliasing { get; set; }
    
    /// <summary>
    /// Color profile to use
    /// </summary>
    string? ColorProfile { get; set; }
}