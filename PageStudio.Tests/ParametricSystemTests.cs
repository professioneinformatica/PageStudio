using PageStudio.Core.Features.ParametricProperties;
using Xunit;

namespace PageStudio.Tests;

public class ParametricSystemTests
{
    [Fact]
    public void ConstantValues_WorkCorrectly()
    {
        var engine = new ParametricEngine();
        var ownerId = Guid.NewGuid();
        
        var propX = engine.CreateProperty<double>(ownerId, "X", "100");
        
        Assert.Equal(100.0, propX.Value);
    }

    [Fact]
    public void CrossReference_UpdatesAutomatically()
    {
        var engine = new ParametricEngine();
        
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
        var engine = new ParametricEngine();
        
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
        var engine = new ParametricEngine();
        
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
        var engine = new ParametricEngine();
        
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
        var engine = new ParametricEngine();
        
        var itemAId = Guid.NewGuid();
        engine.RegisterElement("ItemA", itemAId);
        var opacity = engine.CreateProperty<double>(itemAId, "Opacity", "0.3");
        
        var isVisible = engine.CreateProperty<bool>(itemAId, "IsVisible", "ItemA.Opacity > 0.5");

        Assert.False(isVisible.Value);

        opacity.Value = 0.8;
        Assert.True(isVisible.Value);
    }
}
