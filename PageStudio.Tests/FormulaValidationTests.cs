using PageStudio.Core.Features.ParametricProperties;
using Xunit;

namespace PageStudio.Tests;

public class FormulaValidationTests
{
    private ParametricEngine CreateEngine()
    {
        var doc = new MockDocument();
        var engine = new ParametricEngine(doc);
        doc.ParametricEngine = engine;
        return engine;
    }

    [Fact]
    public void InvalidFormula_ThrowsOnAssignment()
    {
        var engine = CreateEngine();
        var itemAId = Guid.NewGuid();
        engine.RegisterElement("ItemA", itemAId);
        var prop = engine.CreateProperty<double>(itemAId, "Width", "100");

        // Syntax error (already handled by JsFormula constructor, but let's verify)
        Assert.Throws<ArgumentException>(() => {
            prop.FormulaExpression = "100 + ";
        });

        // Runtime error in JS (e.g. calling non-existent function or accessing undefined)
        // Now it should throw ArgumentException (wrapping the JS error) on assignment.
        Assert.Throws<ArgumentException>(() => {
            prop.FormulaExpression = "nonExistentFunction()";
        });
    }

    [Fact]
    public void InvalidFormulaInConstructor_Throws()
    {
        var engine = CreateEngine();
        var itemAId = Guid.NewGuid();
        engine.RegisterElement("ItemA", itemAId);

        Assert.Throws<ArgumentException>(() => {
            engine.CreateProperty<double>(itemAId, "Width", "badFunction()");
        });
    }
}
