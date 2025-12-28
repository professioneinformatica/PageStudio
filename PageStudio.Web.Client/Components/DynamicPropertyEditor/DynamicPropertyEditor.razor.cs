using Microsoft.AspNetCore.Components;
using PageStudio.Core.Features.ParametricProperties;
using PageStudio.Core.Models.Abstractions;

namespace PageStudio.Web.Client.Components.DynamicPropertyEditor;

public partial class DynamicPropertyEditor<T> : ComponentBase
{
    [Parameter] public DynamicProperty<T> Property { get; set; } = null!;
    
    [Parameter] public EventCallback<DynamicProperty<T>> PropertyChanged { get; set; }

    private string DisplayFormula => Property.Symbols.Engine.Translator.Denormalize(Property.FormulaExpression);

    private bool _isFormulaMode;
    private bool _hasError;
    private string? _errorMessage;

    private DynamicProperty<T>? _previousProperty;

    protected override void OnParametersSet()
    {
        if (Property != _previousProperty)
        {
            _isFormulaMode = Property.IsExplicitFormula || !Property.IsConstant;
            _previousProperty = Property;
        }
    }

    private async Task OnValueChanged(ChangeEventArgs e)
    {
        if (typeof(T) == typeof(bool))
        {
            if (bool.TryParse(e.Value?.ToString(), out var result))
            {
                Property.Value = (T)(object)result;
            }
        }
        else if (typeof(T) == typeof(double))
        {
            if (double.TryParse(e.Value?.ToString(), out var result))
            {
                Property.Value = (T)(object)result;
            }
        }
        else if (typeof(T) == typeof(int))
        {
            if (int.TryParse(e.Value?.ToString(), out var result))
            {
                Property.Value = (T)(object)result;
            }
        }
        else if (typeof(T) == typeof(string))
        {
            Property.Value = (T)(object)e.Value?.ToString()!;
        }
        
        await PropertyChanged.InvokeAsync(Property);
    }

    private async Task OnFormulaChanged(ChangeEventArgs e)
    {
        var expression = e.Value?.ToString() ?? string.Empty;
        try
        {
            var normalizedExpression = Property.Symbols.Engine.Translator.Normalize(expression);
            Property.FormulaExpression = normalizedExpression;
            _hasError = false;
            _errorMessage = null;
            await PropertyChanged.InvokeAsync(Property);
        }
        catch (Exception ex)
        {
            _hasError = true;
            _errorMessage = ex.Message;
        }
    }

    private void ToggleMode()
    {
        _isFormulaMode = !_isFormulaMode;
        
        // Se passiamo da formula a costante, cerchiamo di mantenere il valore attuale come costante
        if (!_isFormulaMode)
        {
            var currentValue = Property.GetValue();
            Property.Value = (T?)currentValue;
        }
    }
}
