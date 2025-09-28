namespace PageStudio.Core.Interfaces;

/// <summary>
/// Service for exporting documents to various formats
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Exports document to PDF format
    /// </summary>
    /// <param name="document">Document to export</param>
    /// <param name="filePath">Output file path</param>
    /// <param name="options">Export options</param>
    /// <returns>Task representing the export operation</returns>
    Task<bool> ExportToPdfAsync(IDocument document, string filePath, IPdfExportOptions? options = null);
    
    /// <summary>
    /// Exports document to image format (PNG, JPG)
    /// </summary>
    /// <param name="document">Document to export</param>
    /// <param name="filePath">Output file path</param>
    /// <param name="options">Export options</param>
    /// <returns>Task representing the export operation</returns>
    Task<bool> ExportToImageAsync(IDocument document, string filePath, IImageExportOptions? options = null);
    
    /// <summary>
    /// Exports a specific page to image format
    /// </summary>
    /// <param name="page">Page to export</param>
    /// <param name="filePath">Output file path</param>
    /// <param name="options">Export options</param>
    /// <returns>Task representing the export operation</returns>
    Task<bool> ExportPageToImageAsync(IPage page, string filePath, IImageExportOptions? options = null);
    
    /// <summary>
    /// Gets supported export formats
    /// </summary>
    /// <returns>List of supported formats</returns>
    IEnumerable<string> GetSupportedFormats();
}