using PageStudio.Core.Graphics;
using PageStudio.Core.Interfaces;
using PageStudio.Core.Models;
using SkiaSharp;
using SkiaSharp.Views.Blazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using PageStudio.Core.Models.ContainerPageElements;

namespace PageStudio.Web.Client.Pages;

public partial class DocumentRenderer
{
    private SKCanvasView? canvasView;
    private Document? currentDocument;
    private string documentName = "My Document";
    private bool isRendered = false;

    // Zoom functionality
    private float zoomLevel = 1.0f;
    private const float ZoomIncrement = 0.1f;
    private const float MinZoom = 0.25f;
    private const float MaxZoom = 4.0f;

    // Layout mode functionality
    public enum PageLayoutMode
    {
        Vertical,
        SideBySide
    }

    private PageLayoutMode layoutMode = PageLayoutMode.Vertical;

    // Document properties modal
    private bool showDocumentProperties = false;

    // Add pages modal
    private bool showAddPagesModal = false;
    private AddPageModel addPageModel = new AddPageModel();

    // Add text modal
    private bool showAddTextModal = false;
    private string newTextContent = "Sample Text";
    private string newTextFontFamily = "Arial";
    private float newTextFontSize = 12.0f;

    // Element selection
    private IPageElement? selectedElement = null;

    private void CreateNewDocument()
    {
        Console.WriteLine("[DEBUG_LOG] CreateNewDocument method called");
        currentDocument = new Document(documentName);
        isRendered = false;
        StateHasChanged();
        Console.WriteLine($"[DEBUG_LOG] Document created: {currentDocument?.Name}");
    }

    private void AddSamplePage(AddPageModel model)
    {
        if (currentDocument == null || model == null) return;

        var pageFormat = model.SelectedPageFormat;

        for (int i = 0; i < model.NumberOfPagesToAdd; i++)
        {
            var page = new Page
            {
                Width = pageFormat.ActualWidth,
                Height = pageFormat.ActualHeight,
                Name = $"Page {currentDocument.Pages.Count() + 1}"
            };

            // Add some sample elements to demonstrate rendering
            // This is a simplified example - in a real implementation you would have proper page elements

            currentDocument.AddPage(page);
        }

        CloseAddPagesModal();
        this.RenderDocument();
    }

    private void RenderDocument()
    {
        if (currentDocument == null) return;

        isRendered = true;
        StateHasChanged();

        // Force canvas repaint
        canvasView?.Invalidate();
    }

    private void ZoomIn()
    {
        if (zoomLevel < MaxZoom)
        {
            zoomLevel += ZoomIncrement;
            canvasView?.Invalidate();
        }
    }

    private void ZoomOut()
    {
        if (zoomLevel > MinZoom)
        {
            zoomLevel -= ZoomIncrement;
            canvasView?.Invalidate();
        }
    }

    private void SetVerticalLayout()
    {
        layoutMode = PageLayoutMode.Vertical;
        canvasView?.Invalidate();
    }

    private void SetSideBySideLayout()
    {
        layoutMode = PageLayoutMode.SideBySide;
        canvasView?.Invalidate();
    }


    private void ShowDocumentProperties()
    {
        if (currentDocument != null)
        {
            showDocumentProperties = true;
            StateHasChanged();
        }
    }

    private void CloseDocumentProperties()
    {
        showDocumentProperties = false;
        StateHasChanged();
    }

    private void ShowAddPagesModal()
    {
        if (currentDocument != null)
        {
            showAddPagesModal = true;
            addPageModel.Reset(); // Reset to default
            StateHasChanged();
        }
    }

    private void CloseAddPagesModal()
    {
        showAddPagesModal = false;
        StateHasChanged();
    }

    private void ShowAddTextModal()
    {
        if (currentDocument != null)
        {
            showAddTextModal = true;
            // Reset to default values
            newTextContent = "Sample Text";
            newTextFontFamily = "Arial";
            newTextFontSize = 12.0f;
            StateHasChanged();
        }
    }

    private void CloseAddTextModal()
    {
        showAddTextModal = false;
        StateHasChanged();
    }

    private void OnAddTextRequested(AddTextModal.AddTextRequest request)
    {
        if (currentDocument == null || string.IsNullOrWhiteSpace(request.TextContent))
            return;

        // Create new text element
        var textElement = new TextElement(request.TextContent, request.FontFamily, request.FontSize)
        {
            X = 50, // Default position
            Y = 50,
            ZOrder = 1
        };

        // Add to the first page (or create one if none exists)
        var firstPage = currentDocument.Pages.FirstOrDefault();
        if (firstPage == null)
        {
            // Create a default page if none exists
            firstPage = new Page
            {
                Width = 595, // A4 width in points
                Height = 842, // A4 height in points  
                Name = "Page 1"
            };
            currentDocument.AddPage(firstPage);
        }

        firstPage.AddElement(textElement);
        CloseAddTextModal();

        // Refresh the canvas
        if (isRendered)
        {
            canvasView?.Invalidate();
        }
        else
        {
            RenderDocument();
        }
    }

    private void AddTextElement()
    {
        // Legacy method - kept for backward compatibility if needed
        var request = new AddTextModal.AddTextRequest
        {
            TextContent = newTextContent,
            FontFamily = newTextFontFamily,
            FontSize = newTextFontSize
        };
        OnAddTextRequested(request);
    }

    private void OnCanvasClick(PointerEventArgs e)
    {
        if (currentDocument == null || !isRendered)
            return;

        // Convert screen coordinates to canvas coordinates accounting for zoom
        double canvasX = e.OffsetX / zoomLevel;
        double canvasY = e.OffsetY / zoomLevel;

        // Find clicked element through hit testing
        IPageElement clickedElement = null;
        foreach (var page in currentDocument.Pages)
        {
            if (page is Page concretePage)
            {
                var elementsAtPosition = concretePage.GetElementsAtPosition(canvasX, canvasY);
                clickedElement = elementsAtPosition.OrderByDescending(el => el.ZOrder).FirstOrDefault();
                if (clickedElement != null)
                    break;
            }
        }

        // Update selection
        selectedElement = clickedElement;
        canvasView?.Invalidate();

        Console.WriteLine($"[DEBUG_LOG] Clicked at ({canvasX}, {canvasY}), selected: {selectedElement?.Name ?? "None"}");
    }

    private void OnPropertyChanged()
    {
        if (selectedElement != null)
        {
            canvasView?.Invalidate();
            StateHasChanged();
            Console.WriteLine($"[DEBUG_LOG] Property changed for element: {selectedElement.Name}");
        }
    }

    private void OnDocumentPropertiesChanged(Document updatedDocument)
    {
        // Document has been updated, refresh UI elements that depend on document properties
        documentName = updatedDocument.Name;
        StateHasChanged();

        // Force canvas repaint if document is rendered
        if (isRendered)
        {
            canvasView?.Invalidate();
        }
    }

    private void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var surface = e.Surface;
        var canvas = surface.Canvas;

        canvas.Clear(SKColors.White);

        if (currentDocument == null || !isRendered)
            return;

        // Apply zoom transformation
        canvas.Save();
        canvas.Scale(zoomLevel);

        // Create graphics context
        using var graphicsContext = new GraphicsContext(canvas, 800, 600);
        System.Diagnostics.Debug.WriteLine(canvas.DeviceClipBounds);

        // Render pages based on layout mode
        if (layoutMode == PageLayoutMode.Vertical)
        {
            RenderPagesVertically(graphicsContext);
        }
        else
        {
            RenderPagesSideBySide(graphicsContext);
        }

        // Restore canvas state after zoom transformation
        canvas.Restore();
    }

    private void RenderPagesVertically(IGraphicsContext context)
    {
        var yOffset = 0f;
        foreach (var page in currentDocument!.Pages)
        {
            RenderPage(context, page, 0, yOffset);
            yOffset += (float)page.Height + 20; // Add some spacing between pages
        }
    }

    private void RenderPagesSideBySide(IGraphicsContext context)
    {
        var pages = currentDocument!.Pages.ToList();
        var xOffset = 0f;
        var yOffset = 0f;
        var maxPageHeight = 0f;

        for (int i = 0; i < pages.Count; i++)
        {
            var page = pages[i];

            // If this is an even index (0, 2, 4...), start a new row
            if (i % 2 == 0)
            {
                xOffset = 0f;
                if (i > 0)
                {
                    yOffset += maxPageHeight + 20; // Move to next row with spacing
                }

                maxPageHeight = 0f;
            }
            else
            {
                // Odd index (1, 3, 5...), place to the right of the previous page
                var previousPage = pages[i - 1];
                xOffset = (float)previousPage.Width + 20; // Add spacing between side-by-side pages
            }

            RenderPage(context, page, xOffset, yOffset);

            // Track the maximum height in this row
            maxPageHeight = Math.Max(maxPageHeight, (float)page.Height);
        }
    }

    private void RenderPage(IGraphicsContext context, IPage page, float xOffset, float yOffset)
    {
        // Save the current state
        context.Save();

        try
        {
            // Translate to page position
            context.Translate(xOffset, yOffset);

            // Draw page background
            using var backgroundPaint = new SKPaint
            {
                Color = SKColors.White,
                Style = SKPaintStyle.Fill
            };

            context.DrawRect(new SKRect(0, 0, (float)page.Width, (float)page.Height), backgroundPaint);

            // Draw page border
            using var borderPaint = new SKPaint
            {
                Color = SKColors.LightGray,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = CalculateStrokeWidth(1.0f)
            };

            context.DrawRect(new SKRect(0, 0, (float)page.Width, (float)page.Height), borderPaint);

            // Draw page title
            using var textPaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 16,
                IsAntialias = true
            };

            using var font = new SKFont
            {
                Size = 16
            };

            context.DrawText($"Page: {page.Name}", 10, 25, textPaint, font, SKTextAlign.Left);

            // Draw page dimensions info
            textPaint.TextSize = 12;
            font.Size = 12;
            context.DrawText($"Size: {page.Width} x {page.Height}", 10, 45, textPaint, font, SKTextAlign.Left);

            // Render page elements if they exist
            var elements = page.GetAllElementsByRenderOrder();
            if (elements != null)
            {
                foreach (var element in elements)
                {
                    RenderPageElement(context, element);
                }
            }

            // Draw selection highlight if this element is selected
            if (selectedElement is not null)
            {
                // Translate to element position
                context.Translate((float)selectedElement.X, (float)selectedElement.Y);

                using var selectionPaint = new SKPaint
                {
                    Color = SKColors.Blue,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = CalculateStrokeWidth(1.0f),
                    PathEffect = SKPathEffect.CreateDash(new float[] { 3, 3 }, 0)
                };
                if (Math.Abs(selectedElement.Rotation) > 0.001)
                {
                    context.Rotate((float)selectedElement.Rotation);
                }

                // Draw selection rectangle around the element
                var selectionRect = new SKRect(0, 0, (float)selectedElement.Width, (float)selectedElement.Height);
                context.DrawRect(selectionRect, selectionPaint);
            }
        }
        finally
        {
            // Restore the previous state
            context.Restore();
        }
    }

    private float CalculateStrokeWidth(float baseWidth)
    {
        // Mantiene lo spessore visibile indipendentemente dal zoom
        // Più il zoom è piccolo, più lo spessore aumenta per rimanere visibile
        return Math.Max(baseWidth / zoomLevel, 0.5f);
    }

    private void RenderPageElement(IGraphicsContext context, IPageElement element)
    {
        // Save the current state
        context.Save();

        try
        {
            // Use the element's built-in rendering method
            // This allows each element type to render itself appropriately
            element.Render(context);
        }
        finally
        {
            // Restore the previous state
            context.Restore();
        }
    }
}