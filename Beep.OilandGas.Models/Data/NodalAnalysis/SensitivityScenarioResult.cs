using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.NodalAnalysis;

/// <summary>
/// Deterministic scenario slice used by nodal economic sensitivity outputs.
/// </summary>
public class SensitivityScenarioResult : ModelEntityBase
{
    private string ScenarioNameValue = string.Empty;
    public string ScenarioName
    {
        get => ScenarioNameValue;
        set => SetProperty(ref ScenarioNameValue, value);
    }

    private decimal TotalImpactValue;
    public decimal TotalImpact
    {
        get => TotalImpactValue;
        set => SetProperty(ref TotalImpactValue, value);
    }

    private string DominantParameterValue = string.Empty;
    public string DominantParameter
    {
        get => DominantParameterValue;
        set => SetProperty(ref DominantParameterValue, value);
    }
}
