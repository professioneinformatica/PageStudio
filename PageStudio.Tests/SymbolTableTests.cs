using PageStudio.Core.Features.ParametricProperties;
using Xunit;

namespace PageStudio.Tests;

public class SymbolTableTests
{
    [Fact]
    public void IsSymbolNameAvailable_ReturnsTrueForNewName()
    {
        var table = new SymbolTable();
        Assert.True(table.IsSymbolNameAvailable("NewName", Guid.CreateVersion7()));
    }

    [Fact]
    public void IsSymbolNameAvailable_ReturnsFalseForExistingName()
    {
        var table = new SymbolTable();
        var id = Guid.CreateVersion7();
        table.RegisterElement("Existing", id);
        
        Assert.False(table.IsSymbolNameAvailable("Existing", Guid.CreateVersion7()));
    }

    [Fact]
    public void IsSymbolNameAvailable_ReturnsTrueForCurrentNameOfSameElement()
    {
        var table = new SymbolTable();
        var id = Guid.NewGuid();
        table.RegisterElement("Existing", id);
        
        Assert.True(table.IsSymbolNameAvailable("Existing", id));
    }

    [Fact]
    public void RegisterElement_UpdatesMappingCorrectly()
    {
        var table = new SymbolTable();
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
