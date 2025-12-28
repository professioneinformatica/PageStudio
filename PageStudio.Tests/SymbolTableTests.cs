using PageStudio.Core.Features.ParametricProperties;
using Xunit;

namespace PageStudio.Tests;

public class SymbolTableTests
{
    private ParametricEngine CreateEngine()
    {
        var doc = new MockDocument();
        var engine = new ParametricEngine(doc);
        doc.ParametricEngine = engine;
        return engine;
    }

    [Fact]
    public void IsSymbolNameAvailable_ReturnsTrueForNewName()
    {
        var engine = CreateEngine();
        var table = new SymbolTable(engine);
        Assert.True(table.IsSymbolNameAvailable("NewName", Guid.CreateVersion7()));
    }

    [Fact]
    public void IsSymbolNameAvailable_ReturnsFalseForExistingName()
    {
        var engine = CreateEngine();
        var table = new SymbolTable(engine);
        var id = Guid.CreateVersion7();
        table.RegisterElement("Existing", id);
        
        Assert.False(table.IsSymbolNameAvailable("Existing", Guid.CreateVersion7()));
    }

    [Fact]
    public void IsSymbolNameAvailable_ReturnsTrueForCurrentNameOfSameElement()
    {
        var engine = CreateEngine();
        var table = new SymbolTable(engine);
        var id = Guid.NewGuid();
        table.RegisterElement("Existing", id);
        
        Assert.True(table.IsSymbolNameAvailable("Existing", id));
    }

    [Fact]
    public void RegisterElement_UpdatesMappingCorrectly()
    {
        var engine = CreateEngine();
        var table = new SymbolTable(engine);
        var id = Guid.NewGuid();
        
        table.RegisterElement("Name1", id);
        Assert.Equal("Name1", table.GetSymbolName(id));
        Assert.False(table.IsSymbolNameAvailable("Name1", Guid.NewGuid()));

        table.RegisterElement("Name2", id);
        Assert.Equal("Name2", table.GetSymbolName(id));
        Assert.True(table.IsSymbolNameAvailable("Name1", id));
        Assert.False(table.IsSymbolNameAvailable("Name2", Guid.NewGuid()));
    }
}
