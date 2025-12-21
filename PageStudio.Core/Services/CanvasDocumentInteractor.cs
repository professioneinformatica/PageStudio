using PageStudio.Core.Interfaces;
using PageStudio.Core.Features.EventsManagement;
using PageStudio.Core.Models.Abstractions;
using PageStudio.Core.Models.Documents;
using PageStudio.Core.Models.Page;

namespace PageStudio.Core.Services
{
    public enum LayoutMode
    {
        Vertical,
        SideBySide
    }

    public class CanvasDocumentInteractor
    {
        public enum InteractionMode
        {
            Selection,
            Pan
        }

        public float PanOffsetX { get; set; } = 0f;
        public float PanOffsetY { get; set; } = 0f;
        public LayoutMode CurrentLayoutMode { get; set; } = LayoutMode.Vertical;
        public float PageSpacing { get; set; } = 20f;

        public readonly ZoomManager ZoomManager = new();

        public IDocument CurrentDocument
        {
            get;
            init
            {
                field = value;
                SelectedPage = null;
                SelectedElement = null;
            }
        }

        public record DocumentZoomChangedMessage(CanvasDocumentInteractor CanvasDocumentInteractor) : IEvent;

        public CanvasDocumentInteractor(IEventPublisher eventPublisher, IDocument document)
        {
            this.CurrentDocument = document;
            this.ZoomManager.ZoomChanged += () => { eventPublisher.Publish(new DocumentZoomChangedMessage(this)); };
        }

        public IGraphicsContext? GraphicsContext { get; set; }
        
        public void RenderDocument()
        {
            if (this.GraphicsContext is null)
                return;
            
            this.CurrentDocument.Render(this.GraphicsContext);
        }
        // Element selection
        public IPageElement? SelectedElement { get; set; }

        // Pagina attualmente selezionata
        public IPage? SelectedPage
        {
            get => this.SelectedPageIndex.HasValue ? this.CurrentDocument.Pages[this.SelectedPageIndex.Value] : null;

            set => this.SelectedPageIndex = value is null ? null : this.CurrentDocument.Pages.IndexOf(value);
        }

        public int? SelectedPageIndex { get; set; }
        public InteractionMode ActiveTool { get; set; } = InteractionMode.Selection;

        /// <summary>
        /// Gets the page at the specified canvas coordinates.
        /// </summary>
        /// <param name="canvasX">The x-coordinate on the canvas.</param>
        /// <param name="canvasY">The y-coordinate on the canvas.</param>
        /// <returns>
        /// A tuple containing the page located at the specified coordinates, the horizontal offset
        /// of the page relative to the canvas, and the vertical offset of the page relative to the canvas.
        /// If no page is found at the specified coordinates, the first item in the tuple will be null.
        /// </returns>
        public (IPage? page, double pageOffsetX, double pageOffsetY) GetPageAtPosition(double canvasX, double canvasY)
        {

            double yOffset = 0;
            if (CurrentLayoutMode == LayoutMode.Vertical)
            {
                foreach (var page in this.CurrentDocument.Pages)
                {
                    if (canvasY >= yOffset && canvasY < yOffset + page.Height)
                        return (page, 0, yOffset);
                    yOffset += page.Height + PageSpacing;
                }
            }
            else // SideBySide
            {
                var pages = this.CurrentDocument.Pages.ToList();
                yOffset = 0;
                double maxPageHeight = 0;
                for (int i = 0; i < pages.Count; i++)
                {
                    var page = pages[i];
                    double xOffset;
                    if (i % 2 == 0)
                    {
                        xOffset = 0;
                        if (i > 0)
                            yOffset += maxPageHeight + PageSpacing;
                        maxPageHeight = 0;
                    }
                    else
                    {
                        var previousPage = pages[i - 1];
                        xOffset = previousPage.Width + PageSpacing;
                    }

                    if (canvasX >= xOffset && canvasX < xOffset + page.Width && canvasY >= yOffset && canvasY < yOffset + page.Height)
                        return (page, xOffset, yOffset);
                    maxPageHeight = Math.Max(maxPageHeight, page.Height);
                }
            }

            return (null, 0, 0);
        }

        public IPageElement? HitTestElement(IPage page, double x, double y)
        {
            if (page is Page concretePage)
            {
                var elements = concretePage.GetElementsAtPosition(x, y);
                return elements.OrderByDescending(e => e.ZIndex).FirstOrDefault();
            }

            return null;
        }

        public (IPage? page, IPageElement? element) HitTest(IDocument document, double canvasX, double canvasY)
        {
            var (page, pageOffsetX, pageOffsetY) = GetPageAtPosition(canvasX, canvasY);
            if (page != null)
            {
                var element = HitTestElement(page, canvasX - pageOffsetX, canvasY - pageOffsetY);
                return (page, element);
            }

            return (null, null);
        }

        public (double canvasX, double canvasY) ToCanvasCoordinates(double clientX, double clientY)
        {
            return ((clientX - PanOffsetX) / this.ZoomManager.Level, (clientY - PanOffsetY) / this.ZoomManager.Level);
        }
    }
    
    public class ZoomManager
    {
        public event Action? ZoomChanged;
        public float Level { get; private set; } = 1.0f;
        public float Min { get; } = 0.25f;
        public float Max { get; } = 4.0f;
        public float Increment { get; } = 0.1f;

        public void ZoomIn()
        {
            if (Level < Max)
                Level += Increment;
            if (Level > Max)
                Level = Max;
            ZoomChanged?.Invoke();
        }

        public void ZoomOut()
        {
            if (Level > Min)
                Level -= Increment;
            if (Level < Min)
                Level = Min;
            ZoomChanged?.Invoke();
        }

        public void Set(float value)
        {
            Level = Math.Clamp(value, Min, Max);
            ZoomChanged?.Invoke();
        }
    }
}