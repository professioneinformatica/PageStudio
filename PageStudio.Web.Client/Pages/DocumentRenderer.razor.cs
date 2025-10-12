using Mediator;
using PageStudio.Core.Graphics;
using PageStudio.Core.Interfaces;
using PageStudio.Core.Models;
using SkiaSharp;
using SkiaSharp.Views.Blazor;
using Microsoft.AspNetCore.Components.Web;
using PageStudio.Core.Models.ContainerPageElements;
using Microsoft.FluentUI.AspNetCore.Components;
using PageStudio.Core.Models.Documents;
using PageStudio.Core.Services;

namespace PageStudio.Web.Client.Pages;

public partial class DocumentRenderer
{
    private readonly IMediator _mediator;
    private SKCanvasView? _canvasView;
    private string _documentName = "My Document";
    private bool _isRendered;

    private readonly CanvasDocumentInteractor _canvasInteractor = new();

    public DocumentRenderer(IMediator mediator)
    {
        _mediator = mediator;
        _canvasInteractor.ZoomManager.ZoomChanged += OnZoomChanged;
    }

    private void OnZoomChanged()
    {
        RenderCanvas();
    }

    // Document properties modal
    private bool _showDocumentProperties;

    // Add pages modal
    private bool _showAddPagesModal;
    private readonly AddPageModel _addPageModel = new();

    // Add text modal
    private bool _showAddTextModal;
    private string _newTextContent = "Sample Text";
    private string _newTextFontFamily = "Arial";
    private float _newTextFontSize = 12.0f;

    // Pan e scroll
    private bool _isPanning;
    private float _panStartX;
    private float _panStartY;

    // Stato di prontezza del canvas SkiaSharp
    private bool _canvasReady;

    // Stato drag and drop elemento
    private bool _isDraggingElement;
    private float _dragStartX;
    private float _dragStartY;
    private double _elementStartX;
    private double _elementStartY;

    // Stato mouse
    private double _mouseX;
    private double _mouseY;

    // Variabili per posizione mouse rispetto alla pagina
    private double? _mousePageX;
    private double? _mousePageY;
    private IPage? _hoveredPage;

    private readonly List<ITreeViewItem> _documentTree = [];
    private ITreeViewItem? _selectedTreeNode;

    private string CanvasStyleAttribute
    {
        get
        {
            var cursorType = "default";
            switch (_canvasInteractor.ActiveTool)
            {
                case CanvasDocumentInteractor.InteractionMode.Selection:
                    break;
                case CanvasDocumentInteractor.InteractionMode.Pan:
                    cursorType = _isPanning ? "grabbing" : "grab";
                    break;
            }

            return $"width:100%;height:100%;cursor:{cursorType}";
        }
    }

    private void CreateNewDocument()
    {
        Console.WriteLine("[DEBUG_LOG] CreateNewDocument method called");
        _canvasInteractor.CurrentDocument = new Document(_mediator, _documentName);
        _isRendered = false;
        StateHasChanged();
        Console.WriteLine($"[DEBUG_LOG] Document created: {_canvasInteractor.CurrentDocument?.Name}");
    }

    private async Task AddSamplePage(AddPageModel model)
    {
        if (_canvasInteractor.CurrentDocument == null) return;
        CloseAddPagesModal();

        var addedPages = await _canvasInteractor.CurrentDocument.AddPages(model.SelectedPageFormat, model.NumberOfPagesToAdd, _canvasInteractor.SelectedPageIndex);

        foreach (var page in addedPages)
        {
            var title = page.AddElement(new TextElement()
            {
                Text = $"Page Name: {page.Name}",
                FontFamily = "Arial",
                FontSize = 24,
                TextColor = SKColors.Gray,
                X = 10,
                Y = 10
            });

            page.AddElement(new TextElement()
            {
                Text = $"Page Size: {page.Width} x {page.Height}",
                FontFamily = "Arial",
                FontSize = 16,
                TextColor = SKColors.LightGray,
                X = title.X,
                Y = title.Height + 10,
            });

        }

        BuildDocumentTree(); // Aggiorna l'albero dopo aver aggiunto pagine
        this.RenderDocument();
    }

    private void RenderDocument()
    {
        if (_canvasInteractor.CurrentDocument == null) return;
        _isRendered = true;
        if (_canvasReady)
        {
            StateHasChanged();
            // Force canvas repaint
            RenderCanvas();
        }
    }

    private void SetVerticalLayout()
    {
        _canvasInteractor.CurrentLayoutMode = LayoutMode.Vertical;
        RenderCanvas();
    }

    private void SetSideBySideLayout()
    {
        _canvasInteractor.CurrentLayoutMode = LayoutMode.SideBySide;
        RenderCanvas();
    }

    private void SetSelectionMode()
    {
        StateHasChanged();
    }

    private void SetPanMode()
    {
        StateHasChanged();
    }

    private void ShowDocumentProperties()
    {
        // Chiudi l'ElementPropertiesPanel se visibile
        _canvasInteractor.SelectedElement = null;
        RenderCanvas();

        if (_canvasInteractor.CurrentDocument != null)
        {
            _showDocumentProperties = true;
            StateHasChanged();
        }
    }

    private void CloseDocumentProperties()
    {
        _showDocumentProperties = false;
        StateHasChanged();
    }

    private void ShowAddPagesModal()
    {
        if (_canvasInteractor.CurrentDocument != null)
        {
            _showAddPagesModal = true;
            _addPageModel.Reset(); // Reset to default
            StateHasChanged();
        }
    }

    private void CloseAddPagesModal()
    {
        _showAddPagesModal = false;
        StateHasChanged();
    }

    private void ShowAddTextModal()
    {
        if (_canvasInteractor.CurrentDocument != null)
        {
            _showAddTextModal = true;
            // Reset to default values
            _newTextContent = "Sample Text";
            _newTextFontFamily = "Arial";
            _newTextFontSize = 12.0f;
            StateHasChanged();
        }
    }

    private void CloseAddTextModal()
    {
        _showAddTextModal = false;
        BuildDocumentTree();

        StateHasChanged();
    }

    private void OnAddTextRequested(AddTextModal.AddTextRequest request)
    {
        if (_canvasInteractor.CurrentDocument == null || string.IsNullOrWhiteSpace(request.TextContent))
            return;
        var textElement = new TextElement(request.TextContent, request.FontFamily, request.FontSize)
        {
            X = 50,
            Y = 50,
            ZOrder = 1
        };
        // Aggiungi alla pagina selezionata
        var page = _canvasInteractor.SelectedPage ?? _canvasInteractor.CurrentDocument.Pages.FirstOrDefault();
        if (page is Page concretePage)
        {
            concretePage.AddElement(textElement);
        }

        BuildDocumentTree(); // Aggiorna l'albero dopo aver aggiunto un elemento
        CloseAddTextModal();
        if (_isRendered)
        {
            RenderCanvas();
        }
        else
        {
            RenderDocument();
        }
    }

    private void OnPointerDown(PointerEventArgs e)
    {
        if (_canvasInteractor.ActiveTool == CanvasDocumentInteractor.InteractionMode.Pan)
        {
            _isPanning = true;
            _panStartX = (float)e.ClientX;
            _panStartY = (float)e.ClientY;
        }
        else if (_canvasInteractor.ActiveTool == CanvasDocumentInteractor.InteractionMode.Selection)
        {
            var (canvasX, canvasY) = _canvasInteractor.ToCanvasCoordinates(e.OffsetX, e.OffsetY);
            if (_canvasInteractor.SelectedElement != null)
            {
                double localX = canvasX - _canvasInteractor.SelectedElement.X;
                double localY = canvasY - _canvasInteractor.SelectedElement.Y;
                var handleIdx = _canvasInteractor.SelectedElement is Core.Models.Abstractions.PageElement pe ? pe.HitTestHandle(localX, localY) : null;
                if (handleIdx.HasValue)
                {
                    _isResizingElement = true;
                    _resizingHandleIndex = handleIdx;
                    _resizeStartX = (float)e.ClientX;
                    _resizeStartY = (float)e.ClientY;
                    _elementStartWidth = _canvasInteractor.SelectedElement.Width;
                    _elementStartHeight = _canvasInteractor.SelectedElement.Height;
                    _elementStartXResize = _canvasInteractor.SelectedElement.X;
                    _elementStartYResize = _canvasInteractor.SelectedElement.Y;
                    return;
                }
            }

            if (_canvasInteractor.SelectedElement != null && _canvasInteractor.SelectedPage is Page concretePage)
            {
                var elementsAtPosition = concretePage.GetElementsAtPosition(canvasX, canvasY);
                if (elementsAtPosition.Contains(_canvasInteractor.SelectedElement))
                {
                    _isDraggingElement = true;
                    _dragStartX = (float)e.ClientX;
                    _dragStartY = (float)e.ClientY;
                    _elementStartX = _canvasInteractor.SelectedElement.X;
                    _elementStartY = _canvasInteractor.SelectedElement.Y;
                    return;
                }
            }

            // Usa la nuova logica per selezione
            OnCanvasClick(e);
        }
    }

    private void OnCanvasClick(PointerEventArgs e)
    {
        if (_canvasInteractor.CurrentDocument == null || !_isRendered)
            return;

        var (canvasX, canvasY) = _canvasInteractor.ToCanvasCoordinates(e.OffsetX, e.OffsetY);
        var (clickedPage, clickedElement) = _canvasInteractor.HitTest(_canvasInteractor.CurrentDocument, canvasX, canvasY);
        _canvasInteractor.SelectedPage = clickedPage;
        _canvasInteractor.SelectedElement = clickedElement;
        if (_canvasInteractor.SelectedPage is Page concretePage)
        {
            foreach (var el in concretePage.GetAllElementsByRenderOrder())
                el.IsSelected = false;
            if (clickedElement != null)
                clickedElement.IsSelected = true;
        }

        RenderCanvas();
        Console.WriteLine(
            $"[DEBUG_LOG] Clicked at ({canvasX}, {canvasY}), selected: {_canvasInteractor.SelectedElement?.Name ?? "None"}, page: {_canvasInteractor.SelectedPage?.Name}");
    }

    private void OnPointerMove(PointerEventArgs e)
    {
        _mouseX = e.OffsetX;
        _mouseY = e.OffsetY;
        var (canvasX, canvasY) = _canvasInteractor.ToCanvasCoordinates(e.OffsetX, e.OffsetY);
        var (page, pageOffsetX, pageOffsetY) = _canvasInteractor.GetPageAtPosition(canvasX, canvasY);
        _hoveredPage = page;
        if (page != null)
        {
            _mousePageX = canvasX - pageOffsetX;
            _mousePageY = canvasY - pageOffsetY;
        }
        else
        {
            _mousePageX = null;
            _mousePageY = null;
        }

        if (_canvasInteractor.ActiveTool == CanvasDocumentInteractor.InteractionMode.Pan && _isPanning)
        {
            float deltaX = (float)e.ClientX - _panStartX;
            float deltaY = (float)e.ClientY - _panStartY;
            _canvasInteractor.PanOffsetX += deltaX;
            _canvasInteractor.PanOffsetY += deltaY;
            _panStartX = (float)e.ClientX;
            _panStartY = (float)e.ClientY;
            ClampPanOffsets(_canvasPixelWidth, _canvasPixelHeight);
            RenderCanvas();
        }
        else if (_canvasInteractor.ActiveTool == CanvasDocumentInteractor.InteractionMode.Selection && _isResizingElement && _canvasInteractor.SelectedElement != null &&
                 _resizingHandleIndex.HasValue)
        {
            float deltaX = ((float)e.ClientX - _resizeStartX) / _canvasInteractor.ZoomManager.Level;
            float deltaY = ((float)e.ClientY - _resizeStartY) / _canvasInteractor.ZoomManager.Level;
            double newX = _elementStartXResize;
            double newY = _elementStartYResize;
            double newW = _elementStartWidth;
            double newH = _elementStartHeight;
            bool lockAspect = _canvasInteractor.SelectedElement.LockAspectRatio;
            double aspect = _elementStartWidth / (_elementStartHeight == 0 ? 1 : _elementStartHeight);
            switch (_resizingHandleIndex.Value)
            {
                case 0: // top-left
                    newX = _elementStartXResize + deltaX;
                    newY = _elementStartYResize + deltaY;
                    newW = _elementStartWidth - deltaX;
                    newH = _elementStartHeight - deltaY;
                    break;
                case 1: // top
                    newY = _elementStartYResize + deltaY;
                    newH = _elementStartHeight - deltaY;
                    break;
                case 2: // top-right
                    newY = _elementStartYResize + deltaY;
                    newW = _elementStartWidth + deltaX;
                    newH = _elementStartHeight - deltaY;
                    break;
                case 3: // right
                    newW = _elementStartWidth + deltaX;
                    break;
                case 4: // bottom-right
                    newW = _elementStartWidth + deltaX;
                    newH = _elementStartHeight + deltaY;
                    break;
                case 5: // bottom
                    newH = _elementStartHeight + deltaY;
                    break;
                case 6: // bottom-left
                    newX = _elementStartXResize + deltaX;
                    newW = _elementStartWidth - deltaX;
                    newH = _elementStartHeight + deltaY;
                    break;
                case 7: // left
                    newX = _elementStartXResize + deltaX;
                    newW = _elementStartWidth - deltaX;
                    break;
            }

            // Blocca rapporto di aspetto se richiesto
            if (lockAspect)
            {
                switch (_resizingHandleIndex.Value)
                {
                    case 0: // top-left
                    case 2: // top-right
                    case 4: // bottom-right
                    case 6: // bottom-left
                        if (Math.Abs(deltaX) > Math.Abs(deltaY))
                            newH = newW / aspect;
                        else
                            newW = newH * aspect;
                        if (_resizingHandleIndex.Value == 0 || _resizingHandleIndex.Value == 6)
                            newX = _elementStartXResize + (_elementStartWidth - newW);
                        if (_resizingHandleIndex.Value == 0 || _resizingHandleIndex.Value == 2)
                            newY = _elementStartYResize + (_elementStartHeight - newH);
                        break;
                    case 1: // top
                    case 5: // bottom
                        newW = newH * aspect;
                        newX = _elementStartXResize + (_elementStartWidth - newW) / 2;
                        break;
                    case 3: // right
                    case 7: // left
                        newH = newW / aspect;
                        newY = _elementStartYResize + (_elementStartHeight - newH) / 2;
                        break;
                }
            }

            // Limiti minimi
            newW = Math.Max(10, newW);
            newH = Math.Max(10, newH);
            _canvasInteractor.SelectedElement.X = newX;
            _canvasInteractor.SelectedElement.Y = newY;
            _canvasInteractor.SelectedElement.Width = newW;
            _canvasInteractor.SelectedElement.Height = newH;
            RenderCanvas();
        }
        else if (_canvasInteractor.ActiveTool == CanvasDocumentInteractor.InteractionMode.Selection && _isDraggingElement && _canvasInteractor.SelectedElement != null)
        {
            float deltaX = (float)e.ClientX - _dragStartX;
            float deltaY = (float)e.ClientY - _dragStartY;
            // Aggiorna la posizione dell'elemento selezionato
            _canvasInteractor.SelectedElement.X = _elementStartX + deltaX / _canvasInteractor.ZoomManager.Level;
            _canvasInteractor.SelectedElement.Y = _elementStartY + deltaY / _canvasInteractor.ZoomManager.Level;
            RenderCanvas();
        }

        // Aggiorna la UI per la status bar
        StateHasChanged();
    }

    private void OnPropertyChanged()
    {
        if (_canvasInteractor.SelectedElement != null && _canvasReady)
        {
            this.RenderCanvas();
        }
    }

    private void OnPointerUp(PointerEventArgs e)
    {
        _isPanning = false;
        if (_isDraggingElement)
        {
            _isDraggingElement = false;
            RenderCanvas();
        }

        if (_isResizingElement)
        {
            _isResizingElement = false;
            _resizingHandleIndex = null;
            RenderCanvas();
        }
    }

    private void OnPointerLeave(PointerEventArgs e)
    {
        _isPanning = false;
        if (_isDraggingElement)
        {
            _isDraggingElement = false;
            RenderCanvas();
        }
    }

    private void OnWheel(WheelEventArgs e)
    {
        // Gestione scrolling orizzontale e verticale
        if (Math.Abs(e.DeltaX) > 0.01)
        {
            _canvasInteractor.PanOffsetX -= (float)e.DeltaX;
        }

        if (Math.Abs(e.DeltaY) > 0.01)
        {
            if (e.ShiftKey)
                _canvasInteractor.PanOffsetX -= (float)e.DeltaY;
            else
                _canvasInteractor.PanOffsetY -= (float)e.DeltaY;
        }

        ClampPanOffsets(_canvasPixelWidth, _canvasPixelHeight);
        RenderCanvas();
    }

    // Calcola i limiti di pan e li applica
    private void ClampPanOffsets(float canvasWidth, float canvasHeight)
    {
        if (_canvasInteractor.CurrentDocument == null) return;
        float docWidth;
        float docHeight;
        float spacing = 20f;
        if (_canvasInteractor.CurrentLayoutMode == LayoutMode.Vertical)
        {
            // Larghezza massima tra tutte le pagine
            docWidth = _canvasInteractor.CurrentDocument.Pages.Max(p => (float)p.Width);
            // Altezza totale (somma delle altezze + spazi)
            docHeight = _canvasInteractor.CurrentDocument.Pages.Sum(p => (float)p.Height) + (_canvasInteractor.CurrentDocument.Pages.Count - 1) * spacing;
        }
        else // SideBySide
        {
            var pages = _canvasInteractor.CurrentDocument.Pages.ToList();
            float maxRowWidth = 0f;
            float totalHeight = 0f;
            for (int i = 0; i < pages.Count; i += 2)
            {
                float rowWidth = (float)pages[i].Width;
                float rowHeight = (float)pages[i].Height;
                if (i + 1 < pages.Count)
                {
                    rowWidth += spacing + (float)pages[i + 1].Width;
                    rowHeight = Math.Max(rowHeight, (float)pages[i + 1].Height);
                }

                maxRowWidth = Math.Max(maxRowWidth, rowWidth);
                totalHeight += rowHeight;
                if (i + 2 < pages.Count)
                    totalHeight += spacing;
            }

            docWidth = maxRowWidth;
            docHeight = totalHeight;
        }

        float scaledDocWidth = docWidth * _canvasInteractor.ZoomManager.Level;
        float scaledDocHeight = docHeight * _canvasInteractor.ZoomManager.Level;
        // Limiti: non andare oltre il bordo sinistro/superiore (max 0),
        // né oltre il bordo destro/inferiore (min canvas - doc)
        float minPanX = Math.Min(0, canvasWidth - scaledDocWidth);
        float maxPanX = 0;
        float minPanY = Math.Min(0, canvasHeight - scaledDocHeight);
        float maxPanY = 0;
        _canvasInteractor.PanOffsetX = Math.Min(maxPanX, Math.Max(minPanX, _canvasInteractor.PanOffsetX));
        _canvasInteractor.PanOffsetY = Math.Min(maxPanY, Math.Max(minPanY, _canvasInteractor.PanOffsetY));
    }

    private int _canvasPixelWidth;
    private int _canvasPixelHeight;

    private void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        // Salva la dimensione effettiva del canvas
        _canvasPixelWidth = e.Info.Width;
        _canvasPixelHeight = e.Info.Height;
        var surface = e.Surface;
        var canvas = surface.Canvas;

        canvas.Clear(SKColors.White);
        canvas.Save();
        canvas.Translate(_canvasInteractor.PanOffsetX, _canvasInteractor.PanOffsetY);
        canvas.Scale(_canvasInteractor.ZoomManager.Level);

        // Create graphics context
        using var graphicsContext = new GraphicsContext(canvas, 800, 600);
        System.Diagnostics.Debug.WriteLine(canvas.DeviceClipBounds);

        // Render pages based on layout mode
        if (_canvasInteractor.CurrentLayoutMode == LayoutMode.Vertical)
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
        foreach (var page in _canvasInteractor.CurrentDocument!.Pages)
        {
            RenderPage(context, page, 0, yOffset);
            yOffset += (float)page.Height + 20; // Add some spacing between pages
        }
    }

    private void RenderPagesSideBySide(IGraphicsContext context)
    {
        var pages = _canvasInteractor.CurrentDocument!.Pages.ToList();
        var maxPageHeight = 0f;

        for (var i = 0; i < pages.Count; i++)
        {
            var page = pages[i];

            // If this is an even index (0, 2, 4...), start a new row
            float xOffset;
            float yOffset = 0;
            if (i % 2 == 0)
            {
                xOffset = 0f;
                if (i > 0)
                {
                    yOffset += maxPageHeight + 20; // Move to the next row with spacing
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
            using var backgroundPaint = new SKPaint();
            backgroundPaint.Color = SKColors.White;
            backgroundPaint.Style = SKPaintStyle.Fill;

            context.DrawRect(new SKRect(0, 0, (float)page.Width, (float)page.Height), backgroundPaint);

            // Draw page border
            using var borderPaint = new SKPaint();
            borderPaint.Color = SKColors.LightGray;
            borderPaint.Style = SKPaintStyle.Stroke;
            borderPaint.StrokeWidth = CalculateStrokeWidth(1.0f);

            context.DrawRect(new SKRect(0, 0, (float)page.Width, (float)page.Height), borderPaint);

            // Render page elements if they exist
            var elements = page.GetAllElementsByRenderOrder();
            foreach (var element in elements)
            {
                RenderPageElement(context, element);
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
        return Math.Max(baseWidth / _canvasInteractor.ZoomManager.Level, 0.5f);
    }

    private int? _resizingHandleIndex;
    private bool _isResizingElement;
    private float _resizeStartX, _resizeStartY;
    private double _elementStartWidth, _elementStartHeight, _elementStartXResize, _elementStartYResize;

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

    private async Task OnFluentImageSelected(FluentInputFileEventArgs file)
    {
        if (file.Stream == null) return;
        using var ms = new MemoryStream();
        await file.Stream.CopyToAsync(ms);
        var base64 = Convert.ToBase64String(ms.ToArray());
        var imageElement = new ImageElement(base64)
        {
            X = 50,
            Y = 50
        };
        var page = _canvasInteractor.SelectedPage ?? _canvasInteractor.CurrentDocument?.Pages.FirstOrDefault();
        if (page != null)
            page.AddElement(imageElement);
        BuildDocumentTree(); // Aggiorna l'albero dopo aver aggiunto un'immagine
        RenderCanvas();
    }


    private void BuildDocumentTree()
    {
        _documentTree.Clear();
        if (_canvasInteractor.CurrentDocument == null) return;
        foreach (var page in _canvasInteractor.CurrentDocument.Pages)
        {
            var treeItem = new TreeViewItem
            {
                Text = $"Page {page.Name}",
                Id = page.Id.ToString(),
                Items = new List<ITreeViewItem>()
            };
            BuildElementTree(page, treeItem);
            _documentTree.Add(treeItem);
        }
    }

    private void BuildElementTree(IPage page, ITreeViewItem tree)
    {
        foreach (var layer in page.Layers)
        {
            var treeItem = new TreeViewItem
            {
                Text = $"{layer.Name}",
                Id = layer.Id.ToString(),
                Items = new List<ITreeViewItem>()
            };
            (tree.Items as List<ITreeViewItem>)?.Add(treeItem);
            foreach (var el in layer.Childrens)
            {
                BuildElementNodeRecursive(el, treeItem);
            }
        }
    }

    private void BuildElementNodeRecursive(IPageElement el, ITreeViewItem tree)
    {
        var treeItem = new TreeViewItem
        {
            Text = $"{el.Name}",
            Id = el.Id.ToString(),
            Items = el.CanContainChildren ? new List<ITreeViewItem>() : null
        };
        (tree.Items as List<ITreeViewItem>)?.Add(treeItem);

        if (el.CanContainChildren)
        {
            foreach (var child in el.Childrens)
            {
                BuildElementNodeRecursive(child, treeItem);
            }
        }
    }

    private void OnTreeNodeSelected(ITreeViewItem? node)
    {
        _selectedTreeNode = node;
        if (node == null) return;
        // if (node.IsPage && node.Model is IPage page)
        // {
        //     selectedPage = page;
        //     selectedElement = null;
        // }
        // else if (node.IsElement && node.Model is IPageElement el)
        // {
        //     selectedElement = el;
        //     // Trova la pagina padre
        //     foreach (var p in currentDocument!.Pages)
        //     {
        //         if (p.GetAllElementsByRenderOrder().Contains(el))
        //         {
        //             selectedPage = p;
        //             break;
        //         }
        //     }
        // }

        // StateHasChanged();
    }

// OnAfterRenderAsync: segna il canvas come pronto dopo il primo render
    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            _canvasReady = true;
            StateHasChanged();
        }
    }

    protected override void OnParametersSet()
    {
    }

    /// <summary>
    /// Restituisce la pagina e l'offset della pagina dati canvasX/canvasY
    /// </summary>
    private (IPage? page, double pageOffsetX, double pageOffsetY) GetPageAtPosition(double canvasX, double canvasY)
    {
        if (_canvasInteractor.CurrentDocument == null) return (null, 0, 0);
        if (_canvasInteractor.CurrentLayoutMode == LayoutMode.Vertical)
        {
            double yOffset = 0;
            foreach (var page in _canvasInteractor.CurrentDocument.Pages)
            {
                if (canvasY >= yOffset && canvasY < yOffset + page.Height)
                {
                    return (page, 0, yOffset);
                }

                yOffset += page.Height + 20;
            }
        }
        else // SideBySide
        {
            var pages = _canvasInteractor.CurrentDocument.Pages.ToList();
            double yOffset = 0;
            double maxPageHeight = 0;
            for (int i = 0; i < pages.Count; i++)
            {
                var page = pages[i];
                double xOffset;
                if (i % 2 == 0)
                {
                    xOffset = 0;
                    if (i > 0)
                        yOffset += maxPageHeight + 20;
                    maxPageHeight = 0;
                }
                else
                {
                    var previousPage = pages[i - 1];
                    xOffset = previousPage.Width + 20;
                }

                if (canvasX >= xOffset && canvasX < xOffset + page.Width && canvasY >= yOffset && canvasY < yOffset + page.Height)
                {
                    return (page, xOffset, yOffset);
                }

                maxPageHeight = Math.Max(maxPageHeight, page.Height);
            }
        }

        return (null, 0, 0);
    }

    // Funzione centralizzata per check OS e invalidate canvas
    private void RenderCanvas()
    {
        if (OperatingSystem.IsBrowser())
        {
            _canvasView?.Invalidate();
        }
    }
}