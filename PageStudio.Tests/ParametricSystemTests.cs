using PageStudio.Core.Features.ParametricProperties;
using PageStudio.Core.Models.Documents;
using PageStudio.Core.Interfaces;
using PageStudio.Core.Models.Page;
using PageStudio.Core.Models.Abstractions;
using PageStudio.Core.Models.ContainerPageElements;
using Xunit;

namespace PageStudio.Tests;

public class MockDocument : IDocument
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; set; } = "Mock";
    public IList<IPage> Pages { get; } = new List<IPage>();
    public ParametricEngine ParametricEngine { get; set; } = null!;
    // Implementazione minima degli altri membri se necessario, ma per i test dell'engine basta questo
    public Task AddPage(IPage page, int? insertIndex) => Task.CompletedTask;
    public Task<bool> RemovePage(Guid pageId) => Task.FromResult(true);
    public IPage GetPage(Guid pageId) => null!;
    public IPage? GetPageByIndex(int index) => null;
    public void InsertPage(int index, IPage page) {}
    public Task<bool> MovePage(int fromIndex, int toIndex) => Task.FromResult(true);
    public void ClearPages() {}
    public void SetMetadata(string key, object value) {}
    public object? GetMetadata(string key) => null;
    public void Render(IGraphicsContext graphics) {}
    public void UpdateModifiedTime() {}
    public Task<List<IPage>> AddPages(PageStudio.Core.Models.Page.PageFormat pageFormat, int numberOfPagesToAdd, int? startPage = null) => Task.FromResult(new List<IPage>());
    public Task SetActivePage(Guid pageId) => Task.CompletedTask;
    public SkiaSharp.SKSurface Surface { get; set; } = null!;
    public int Dpi { get; set; }
    public UnitOfMeasure UnitOfMeasure { get; set; }
    public PageStudio.Core.Services.CanvasDocumentInteractor CanvasInteractor { get; set; } = null!;
    public DateTime CreatedAt { get; }
    public DateTime ModifiedAt { get; set; }
    public Dictionary<string, object> Metadata { get; } = new();
    public PageStudio.Core.Models.Page.PageFormat DefaultPageFormat { get; set; } = null!;
}

public class ParametricSystemTests
{
    private ParametricEngine CreateEngine()
    {
        var doc = new MockDocument();
        var engine = new ParametricEngine(doc);
        doc.ParametricEngine = engine;
        return engine;
    }

    [Fact]
    public void ConstantValues_WorkCorrectly()
    {
        var engine = CreateEngine();
        var ownerId = Guid.NewGuid();
        
        var propX = engine.CreateProperty<double>(ownerId, "X", "100");
        
        Assert.Equal(100.0, propX.Value);
    }

    [Fact]
    public void CrossReference_UpdatesAutomatically()
    {
        var engine = CreateEngine();
        
        var itemAId = Guid.NewGuid();
        engine.RegisterElement("ItemA", itemAId);
        var itemAWidth = engine.CreateProperty<double>(itemAId, "Width", "100");
        
        var itemBId = Guid.NewGuid();
        engine.RegisterElement("ItemB", itemBId);
        var itemBX = engine.CreateProperty<double>(itemBId, "X", "ItemA.Width + 20");

        // Initial check
        Assert.Equal(120.0, itemBX.Value);

        // Update ItemA.Width
        itemAWidth.Value = 200.0;
        
        // ItemB.X should be dirty and update when accessed
        Assert.True(itemBX.IsDirty);
        Assert.Equal(220.0, itemBX.Value);
    }

    [Fact]
    public void CircularDependency_IsPreventedOnAssignment()
    {
        var engine = CreateEngine();
        
        var itemAId = Guid.NewGuid();
        engine.RegisterElement("A", itemAId);
        var aWidth = engine.CreateProperty<double>(itemAId, "Width", "10");
        
        var itemBId = Guid.NewGuid();
        engine.RegisterElement("B", itemBId);
        var bHeight = engine.CreateProperty<double>(itemBId, "Height", "A.Width");

        // Try to close the loop: A.Width = B.Height
        Assert.Throws<InvalidOperationException>(() => {
            aWidth.Formula = new JsFormula("B.Height");
        });
    }

    [Fact]
    public void MultipleDependencies_WorkCorrectly()
    {
        var engine = CreateEngine();
        
        var pageId = Guid.NewGuid();
        engine.RegisterElement("Page", pageId);
        var pageWidth = engine.CreateProperty<double>(pageId, "Width", "1000");

        var headerId = Guid.NewGuid();
        engine.RegisterElement("Header", headerId);
        var headerWidth = engine.CreateProperty<double>(headerId, "Width", "Page.Width * 0.5");

        Assert.Equal(500.0, headerWidth.Value);

        pageWidth.Value = 800.0;
        Assert.Equal(400.0, headerWidth.Value);
    }

    [Fact]
    public void ComplexFormula_WorksCorrectly()
    {
        var engine = CreateEngine();
        
        var itemAId = Guid.NewGuid();
        engine.RegisterElement("ItemA", itemAId);
        engine.CreateProperty<double>(itemAId, "Width", "100");
        
        var itemBId = Guid.NewGuid();
        engine.RegisterElement("ItemB", itemBId);
        engine.CreateProperty<double>(itemBId, "Height", "50");

        var resultProp = engine.CreateProperty<double>(Guid.NewGuid(), "Result", "Math.max(ItemA.Width, ItemB.Height * 3)");

        Assert.Equal(150.0, resultProp.Value);
    }

    [Fact]
    public void BooleanProperties_WorkCorrectly()
    {
        var engine = CreateEngine();
        
        var itemAId = Guid.NewGuid();
        engine.RegisterElement("ItemA", itemAId);
        var opacity = engine.CreateProperty<double>(itemAId, "Opacity", "0.3");
        
        var isVisible = engine.CreateProperty<bool>(itemAId, "IsVisible", "ItemA.Opacity > 0.5");

        Assert.False(isVisible.Value);

        opacity.Value = 0.8;
        Assert.True(isVisible.Value);
    }

    [Fact]
    public void HierarchicalResolution_WorksCorrectly()
    {
        var doc = new MockDocument();
        var engine = new ParametricEngine(doc);
        doc.ParametricEngine = engine;
        var eventPublisher = new PageStudio.Core.Features.EventsManagement.EventPublisher();

        // Setup gerarchia: Page1 -> Layer1 -> Element1
        var page1 = new PageStudio.Core.Models.Page.Page(eventPublisher, doc, "Page1");
        var layer1 = new Layer(eventPublisher, page1, "Layer1");
        page1.Layers.Add(layer1);
        ((List<IPage>)doc.Pages).Add(page1);

        var element1Id = Guid.NewGuid();
        // Usiamo una classe concreta di PageElement per il test
        var element1 = new PageStudio.Core.Models.ContainerPageElements.TextElement(eventPublisher, page1, "Sample Text")
        {
            Name = "Element1"
        };
        
        // Invece di creare un nuovo GUID, prendiamo quello generato
        element1Id = element1.Id;
        
        layer1.AddChild(element1);
        
        // Registriamo il simbolo "Element1" associato all'ID
        engine.Symbols.RegisterElement("Element1", element1Id);
        engine.CreateProperty<double>(element1Id, "X", "100");

        // Test normalizzazione con percorso completo
        var formula = "Page1.Layer1.Element1.X + 50";
        var normalized = engine.Translator.Normalize(formula);
        
        Assert.Contains(element1Id.ToString(), normalized);
        Assert.Contains(":X", normalized);

        // Test denormalizzazione
        var denormalized = engine.Translator.Denormalize(normalized);
        Assert.Equal("Page1.Layer1.Element1.X + 50", denormalized);
    }
}
