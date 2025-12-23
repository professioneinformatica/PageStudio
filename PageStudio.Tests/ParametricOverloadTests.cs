using PageStudio.Core.Features.ParametricProperties;
using Xunit;

namespace PageStudio.Tests;

public class ParametricOverloadTests
{
    [Fact]
    public void CreateProperty_WithTypedValue_WorksCorrectly()
    {
        var engine = new ParametricEngine();
        var ownerId = Guid.NewGuid();
        
        // Use typed double
        var propX = engine.CreateProperty(ownerId, "X", 123.45);
        
        Assert.Equal(123.45, propX.Value);
        Assert.False(propX.IsExplicitFormula);
        Assert.Equal("123.45", propX.FormulaExpression);
    }

    [Fact]
    public void CreateProperty_WithTypedBool_WorksCorrectly()
    {
        var engine = new ParametricEngine();
        var ownerId = Guid.NewGuid();
        
        // Use typed bool
        var propVisible = engine.CreateProperty(ownerId, "IsVisible", true);
        
        Assert.True(propVisible.Value);
        Assert.False(propVisible.IsExplicitFormula);
        Assert.Equal("true", propVisible.FormulaExpression);
    }

    [Fact]
    public void CreateProperty_WithFormulaString_WorksAsBefore()
    {
        var engine = new ParametricEngine();
        var ownerId = Guid.NewGuid();
        
        // Use string formula
        var propX = engine.CreateProperty<double>(ownerId, "X", "100 + 50");
        
        Assert.Equal(150.0, propX.Value);
        Assert.True(propX.IsExplicitFormula);
        Assert.Equal("100 + 50", propX.FormulaExpression);
    }

    [Fact]
    public void CreateProperty_WithStringValue_CanBeDisambiguated()
    {
        var engine = new ParametricEngine();
        var ownerId = Guid.NewGuid();
        
        // To use a string as a constant value instead of a formula when T is string:
        // We can use the named argument 'initialValue'
        var prop = engine.CreateProperty<string>(ownerId, "S", initialValue: "1 + 1");
        
        Assert.Equal("1 + 1", prop.Value);
        Assert.False(prop.IsExplicitFormula);
        Assert.Equal("\"1 + 1\"", prop.FormulaExpression);

        // To use it as a formula:
        var propFormula = engine.CreateProperty<string>(ownerId, "SF", formulaExpression: "'Hello' + ' World'");
        Assert.Equal("Hello World", propFormula.Value);
        Assert.True(propFormula.IsExplicitFormula);
    }
}
