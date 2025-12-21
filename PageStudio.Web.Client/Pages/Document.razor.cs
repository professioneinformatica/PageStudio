using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.FluentUI.AspNetCore.Components;
using PageStudio.Core.Features.EventsManagement;
using PageStudio.Core.Graphics;
using PageStudio.Core.Interfaces;
using PageStudio.Core.Models;
using PageStudio.Core.Models.Abstractions;
using PageStudio.Core.Models.ContainerPageElements;
using PageStudio.Core.Models.Page;
using PageStudio.Core.Services;
using SkiaSharp;
using SkiaSharp.Views.Blazor;

namespace PageStudio.Web.Client.Pages;

public partial class Document(IEventPublisher eventPublisher, ILogger<Document> logger, IDocumentsRepository documentsRepository)
{
    private SKCanvasView? _canvasView;
    private string _documentName = "My Document";

    // _canvasInteractor.ZoomManager.ZoomChanged += OnZoomChanged;

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
            switch (documentsRepository.CurrentDocument?.CanvasInteractor.ActiveTool)
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
        logger.LogDebug("CreateNewDocument method called");

        var doc = documentsRepository.Create(_documentName);

        StateHasChanged();
        logger.LogDebug("Document created: {Document}", doc.Name);
    }

    private async Task AddPage(AddPageModel model)
    {
        if (documentsRepository.CurrentDocument is null) return;
        CloseAddPagesModal();

        var addedPages = await documentsRepository.CurrentDocument.AddPages(model.SelectedPageFormat, model.NumberOfPagesToAdd,
            documentsRepository.CurrentDocument.CanvasInteractor.SelectedPageIndex);

        foreach (var page in addedPages)
        {
            var title = page.AddElement(new TextElement(eventPublisher, page)
            {
                Text = $"Page Name: {page.Name}",
                FontFamily = "Arial",
                FontSize = 24,
                TextColor = SKColors.Gray,
                X = { Value = 10 },
                Y = { Value = 10 },
                HideFromDocumentStructure = true
            });

            page.AddElement(new TextElement(eventPublisher, page)
            {
                Text = $"Page Size: {page.Width} x {page.Height}",
                FontFamily = "Arial",
                FontSize = 16,
                TextColor = SKColors.LightGray,
                X = { Value = title.X.Value },
                Y = { Value = title.Height.Value + 10 },
                HideFromDocumentStructure = true
            });
        }

        BuildDocumentTree(); // Aggiorna l'albero dopo aver aggiunto pagine
        this.RenderCanvas();
    }

    private void SetVerticalLayout()
    {
        Guard.Against.Null(documentsRepository.CurrentDocument);
        documentsRepository.CurrentDocument.CanvasInteractor.CurrentLayoutMode = LayoutMode.Vertical;
        RenderCanvas();
    }

    private void SetSideBySideLayout()
    {
        Guard.Against.Null(documentsRepository.CurrentDocument);
        documentsRepository.CurrentDocument.CanvasInteractor.CurrentLayoutMode = LayoutMode.SideBySide;
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
        Guard.Against.Null(documentsRepository.CurrentDocument);

        documentsRepository.CurrentDocument.CanvasInteractor.SelectedElement = null;
        RenderCanvas();

        _showDocumentProperties = true;
        StateHasChanged();
    }

    private void CloseDocumentProperties()
    {
        _showDocumentProperties = false;
        StateHasChanged();
    }

    private void ShowAddPagesModal()
    {
        Guard.Against.Null(documentsRepository.CurrentDocument);

        _showAddPagesModal = true;
        _addPageModel.Reset(); // Reset to default
        StateHasChanged();
    }

    private void CloseAddPagesModal()
    {
        _showAddPagesModal = false;
        StateHasChanged();
    }

    private void ShowAddTextModal()
    {
        Guard.Against.Null(documentsRepository.CurrentDocument);

        _showAddTextModal = true;
        // Reset to default values
        _newTextContent = "Sample Text";
        _newTextFontFamily = "Arial";
        _newTextFontSize = 12.0f;
        StateHasChanged();
    }

    private void CloseAddTextModal()
    {
        _showAddTextModal = false;
        BuildDocumentTree();

        StateHasChanged();
    }

    private void OnAddTextRequested(AddTextModal.AddTextRequest request)
    {
        Guard.Against.Null(documentsRepository.CurrentDocument);

        if (string.IsNullOrWhiteSpace(request.TextContent)
            || documentsRepository.CurrentDocument.CanvasInteractor.SelectedPage is null)
            return;


        var textElement = new TextElement(eventPublisher, documentsRepository.CurrentDocument.CanvasInteractor.SelectedPage, request.TextContent, request.FontFamily,
            request.FontSize)
        {
            X = { Value = 50 },
            Y = { Value = 50 },
            ZIndex = 1
        };
        // Aggiungi alla pagina selezionata
        documentsRepository.CurrentDocument.CanvasInteractor.SelectedPage.AddElement(textElement);

        BuildDocumentTree(); // Aggiorna l'albero dopo aver aggiunto un elemento
        CloseAddTextModal();

        RenderCanvas();
    }

    private void OnPointerDown(PointerEventArgs e)
    {
        Guard.Against.Null(documentsRepository.CurrentDocument);

        if (documentsRepository.CurrentDocument.CanvasInteractor.ActiveTool == CanvasDocumentInteractor.InteractionMode.Pan)
        {
            _isPanning = true;
            _panStartX = (float)e.ClientX;
            _panStartY = (float)e.ClientY;
        }
        else if (documentsRepository.CurrentDocument.CanvasInteractor.ActiveTool == CanvasDocumentInteractor.InteractionMode.Selection)
        {
            var (canvasX, canvasY) = documentsRepository.CurrentDocument.CanvasInteractor.ToCanvasCoordinates(e.OffsetX, e.OffsetY);
            if (documentsRepository.CurrentDocument.CanvasInteractor.SelectedElement != null)
            {
                double localX = canvasX - documentsRepository.CurrentDocument.CanvasInteractor.SelectedElement.X.Value;
                double localY = canvasY - documentsRepository.CurrentDocument.CanvasInteractor.SelectedElement.Y.Value;
                var handleIdx = documentsRepository.CurrentDocument.CanvasInteractor.SelectedElement is Core.Models.Abstractions.PageElement pe
                    ? pe.HitTestHandle(localX, localY)
                    : null;
                if (handleIdx.HasValue)
                {
                    _isResizingElement = true;
                    _resizingHandleIndex = handleIdx;
                    _resizeStartX = (float)e.ClientX;
                    _resizeStartY = (float)e.ClientY;
                    _elementStartWidth = documentsRepository.CurrentDocument.CanvasInteractor.SelectedElement.Width.Value;
                    _elementStartHeight = documentsRepository.CurrentDocument.CanvasInteractor.SelectedElement.Height.Value;
                    _elementStartXResize = documentsRepository.CurrentDocument.CanvasInteractor.SelectedElement.X.Value;
                    _elementStartYResize = documentsRepository.CurrentDocument.CanvasInteractor.SelectedElement.Y.Value;
                    return;
                }
            }

            if (documentsRepository.CurrentDocument.CanvasInteractor.SelectedElement != null &&
                documentsRepository.CurrentDocument.CanvasInteractor.SelectedPage is Page concretePage)
            {
                var elementsAtPosition = concretePage.GetElementsAtPosition(canvasX, canvasY);
                if (elementsAtPosition.Contains(documentsRepository.CurrentDocument.CanvasInteractor.SelectedElement))
                {
                    _isDraggingElement = true;
                    _dragStartX = (float)e.ClientX;
                    _dragStartY = (float)e.ClientY;
                    _elementStartX = documentsRepository.CurrentDocument.CanvasInteractor.SelectedElement.X.Value;
                    _elementStartY = documentsRepository.CurrentDocument.CanvasInteractor.SelectedElement.Y.Value;
                    return;
                }
            }

            // Usa la nuova logica per selezione
            OnCanvasClick(e);
        }
    }

    private void OnCanvasClick(PointerEventArgs e)
    {
        if (documentsRepository.CurrentDocument is null)
            return;

        var (canvasX, canvasY) = documentsRepository.CurrentDocument.CanvasInteractor.ToCanvasCoordinates(e.OffsetX, e.OffsetY);
        var (clickedPage, clickedElement) = documentsRepository.CurrentDocument.CanvasInteractor.HitTest(documentsRepository.CurrentDocument, canvasX, canvasY);
        documentsRepository.CurrentDocument.CanvasInteractor.SelectedPage = clickedPage;
        documentsRepository.CurrentDocument.CanvasInteractor.SelectedElement = clickedElement;
        if (documentsRepository.CurrentDocument.CanvasInteractor.SelectedPage is Page concretePage)
        {
            foreach (var el in concretePage.GetAllElementsByRenderOrder())
                el.IsSelected = false;
            if (clickedElement != null)
                clickedElement.IsSelected = true;
        }

        RenderCanvas();
        Console.WriteLine(
            $"[DEBUG_LOG] Clicked at ({canvasX}, {canvasY}), selected: {documentsRepository.CurrentDocument.CanvasInteractor.SelectedElement?.Name ?? "None"}, page: {documentsRepository.CurrentDocument.CanvasInteractor.SelectedPage?.Name}");
    }

    private void OnPointerMove(PointerEventArgs e)
    {
        Guard.Against.Null(documentsRepository.CurrentDocument);

        _mouseX = e.OffsetX;
        _mouseY = e.OffsetY;
        var (canvasX, canvasY) = documentsRepository.CurrentDocument.CanvasInteractor.ToCanvasCoordinates(e.OffsetX, e.OffsetY);
        var (page, pageOffsetX, pageOffsetY) = documentsRepository.CurrentDocument.CanvasInteractor.GetPageAtPosition(canvasX, canvasY);
        _hoveredPage = page;
        if (page != null)
        {
            //_mousePageX = canvasX - pageOffsetX;
            _mousePageX = e.OffsetX;
            _mousePageY = canvasY - pageOffsetY;
        }
        else
        {
            _mousePageX = null;
            _mousePageY = null;
        }

        if (documentsRepository.CurrentDocument.CanvasInteractor.ActiveTool == CanvasDocumentInteractor.InteractionMode.Pan && _isPanning)
        {
            float deltaX = (float)e.ClientX - _panStartX;
            float deltaY = (float)e.ClientY - _panStartY;
            documentsRepository.CurrentDocument.CanvasInteractor.PanOffsetX += deltaX;
            documentsRepository.CurrentDocument.CanvasInteractor.PanOffsetY += deltaY;
            _panStartX = (float)e.ClientX;
            _panStartY = (float)e.ClientY;
            ClampPanOffsets(_canvasPixelWidth, _canvasPixelHeight);
            RenderCanvas();
        }
        else if (documentsRepository.CurrentDocument.CanvasInteractor.ActiveTool == CanvasDocumentInteractor.InteractionMode.Selection && _isResizingElement &&
                 documentsRepository.CurrentDocument.CanvasInteractor.SelectedElement != null &&
                 _resizingHandleIndex.HasValue)
        {
            float deltaX = ((float)e.ClientX - _resizeStartX) / documentsRepository.CurrentDocument.CanvasInteractor.ZoomManager.Level;
            float deltaY = ((float)e.ClientY - _resizeStartY) / documentsRepository.CurrentDocument.CanvasInteractor.ZoomManager.Level;
            double newX = _elementStartXResize;
            double newY = _elementStartYResize;
            double newW = _elementStartWidth;
            double newH = _elementStartHeight;
            bool lockAspect = documentsRepository.CurrentDocument.CanvasInteractor.SelectedElement.LockAspectRatio;
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
            documentsRepository.CurrentDocument.CanvasInteractor.SelectedElement.X.Value = newX;
            documentsRepository.CurrentDocument.CanvasInteractor.SelectedElement.Y.Value = newY;
            documentsRepository.CurrentDocument.CanvasInteractor.SelectedElement.Width.Value = newW;
            documentsRepository.CurrentDocument.CanvasInteractor.SelectedElement.Height.Value = newH;
            RenderCanvas();
        }
        else if (documentsRepository.CurrentDocument.CanvasInteractor.ActiveTool == CanvasDocumentInteractor.InteractionMode.Selection && _isDraggingElement &&
                 documentsRepository.CurrentDocument.CanvasInteractor.SelectedElement != null)
        {
            float deltaX = (float)e.ClientX - _dragStartX;
            float deltaY = (float)e.ClientY - _dragStartY;
            // Aggiorna la posizione dell'elemento selezionato
            documentsRepository.CurrentDocument.CanvasInteractor.SelectedElement.X.Value =
                _elementStartX + deltaX / documentsRepository.CurrentDocument.CanvasInteractor.ZoomManager.Level;
            documentsRepository.CurrentDocument.CanvasInteractor.SelectedElement.Y.Value =
                _elementStartY + deltaY / documentsRepository.CurrentDocument.CanvasInteractor.ZoomManager.Level;
            RenderCanvas();
        }

        // Aggiorna la UI per la status bar
        StateHasChanged();
    }

    private void OnPropertyChanged()
    {
        if (documentsRepository.CurrentDocument?.CanvasInteractor.SelectedElement != null && _canvasReady)
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
        Guard.Against.Null(documentsRepository.CurrentDocument);

        // Gestione scrolling orizzontale e verticale
        if (Math.Abs(e.DeltaX) > 0.01)
        {
            documentsRepository.CurrentDocument.CanvasInteractor.PanOffsetX -= (float)e.DeltaX;
        }

        if (Math.Abs(e.DeltaY) > 0.01)
        {
            if (e.ShiftKey)
                documentsRepository.CurrentDocument.CanvasInteractor.PanOffsetX -= (float)e.DeltaY;
            else
                documentsRepository.CurrentDocument.CanvasInteractor.PanOffsetY -= (float)e.DeltaY;
        }

        ClampPanOffsets(_canvasPixelWidth, _canvasPixelHeight);
        RenderCanvas();
    }

    // Calcola i limiti di pan e li applica
    private void ClampPanOffsets(float canvasWidth, float canvasHeight)
    {
        if (documentsRepository.CurrentDocument is null) return;
        float docWidth;
        float docHeight;
        float spacing = 20f;
        if (documentsRepository.CurrentDocument.CanvasInteractor.CurrentLayoutMode == LayoutMode.Vertical)
        {
            // Larghezza massima tra tutte le pagine
            docWidth = documentsRepository.CurrentDocument.Pages.Max(p => (float)p.Width);
            // Altezza totale (somma delle altezze + spazi)
            docHeight = documentsRepository.CurrentDocument.Pages.Sum(p => (float)p.Height) + (documentsRepository.CurrentDocument.Pages.Count - 1) * spacing;
        }
        else // SideBySide
        {
            var pages = documentsRepository.CurrentDocument.Pages.ToList();
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

        float scaledDocWidth = docWidth * documentsRepository.CurrentDocument.CanvasInteractor.ZoomManager.Level;
        float scaledDocHeight = docHeight * documentsRepository.CurrentDocument.CanvasInteractor.ZoomManager.Level;
        // Limiti: non andare oltre il bordo sinistro/superiore (max 0),
        // né oltre il bordo destro/inferiore (min canvas - doc)
        float minPanX = Math.Min(0, canvasWidth - scaledDocWidth);
        float maxPanX = 0;
        float minPanY = Math.Min(0, canvasHeight - scaledDocHeight);
        float maxPanY = 0;
        documentsRepository.CurrentDocument.CanvasInteractor.PanOffsetX = Math.Min(maxPanX, Math.Max(minPanX, documentsRepository.CurrentDocument.CanvasInteractor.PanOffsetX));
        documentsRepository.CurrentDocument.CanvasInteractor.PanOffsetY = Math.Min(maxPanY, Math.Max(minPanY, documentsRepository.CurrentDocument.CanvasInteractor.PanOffsetY));
    }

    private int _canvasPixelWidth;
    private int _canvasPixelHeight;

    private async Task OnZoomOutClickedAsync()
    {
        Guard.Against.Null(documentsRepository.CurrentDocument);
        documentsRepository.CurrentDocument.CanvasInteractor.ZoomManager.ZoomOut();
        this.RenderCanvas();
        await Task.CompletedTask;
    }

    private async Task OnZoomInClickedAsync()
    {
        Guard.Against.Null(documentsRepository.CurrentDocument);
        documentsRepository.CurrentDocument.CanvasInteractor.ZoomManager.ZoomIn();
        this.RenderCanvas();
        await Task.CompletedTask;
    }

    private void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        Guard.Against.Null(documentsRepository.CurrentDocument);
        var graphicsContext = GraphicsContext.FromSurface(e.Surface);
        documentsRepository.CurrentDocument.CanvasInteractor.GraphicsContext = graphicsContext;
        documentsRepository.CurrentDocument.Surface = e.Surface;

        documentsRepository.CurrentDocument.Render(graphicsContext);
        // Salva la dimensione effettiva del canvas
        _canvasPixelWidth = e.Info.Width;
        _canvasPixelHeight = e.Info.Height;
    }


    private int? _resizingHandleIndex;
    private bool _isResizingElement;
    private float _resizeStartX, _resizeStartY;
    private double _elementStartWidth, _elementStartHeight, _elementStartXResize, _elementStartYResize;

    private async Task OnFluentImageSelected(FluentInputFileEventArgs file)
    {
        Guard.Against.Null(documentsRepository.CurrentDocument);
        if (documentsRepository.CurrentDocument.CanvasInteractor.SelectedPage is null)
        {
            return;
        }

        if (file.Stream == null) return;
        using var ms = new MemoryStream();
        await file.Stream.CopyToAsync(ms);
        var base64 = Convert.ToBase64String(ms.ToArray());
        var imageElement = new ImageElement(eventPublisher, documentsRepository.CurrentDocument.CanvasInteractor.SelectedPage, base64)
        {
            X = { Value = 50 },
            Y = { Value = 50 }
        };
        documentsRepository.CurrentDocument.CanvasInteractor.SelectedPage.AddElement(imageElement);
        BuildDocumentTree(); // Aggiorna l'albero dopo aver aggiunto un'immagine
        RenderCanvas();
    }


    private void BuildDocumentTree()
    {
        _documentTree.Clear();
        if (documentsRepository.CurrentDocument is null) return;
        foreach (var page in documentsRepository.CurrentDocument.Pages)
        {
            var treeItem = new TreeViewItem
            {
                Text = $"{page.Name}",
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
            foreach (var el in layer.Children)
            {
                BuildElementNodeRecursive(el, treeItem);
            }
        }
    }

    private void BuildElementNodeRecursive(IPageElement el, ITreeViewItem tree)
    {
        if (el.HideFromDocumentStructure)
        {
            return;
        }

        var treeItem = new TreeViewItem
        {
            Text = $"{el.Name}",
            Id = el.Id.ToString(),
            Items = el.CanContainChildren ? new List<ITreeViewItem>() : null
        };
        (tree.Items as List<ITreeViewItem>)?.Add(treeItem);

        if (el.CanContainChildren)
        {
            foreach (var child in el.Children.Where(x => !x.HideFromDocumentStructure))
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

    // Funzione centralizzata per check OS e invalidate canvas
    private void RenderCanvas()
    {
        if (OperatingSystem.IsBrowser())
        {
            _canvasView?.Invalidate();
        }
    }
}