using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.OilGas.DataAI;
using TheTechIdea.OilGas.Drilling;
using TheTechIdea.OilGas.Integrity;
using TheTechIdea.OilGas.Mapping;
using TheTechIdea.OilGas.Production;
using TheTechIdea.OilGas.Training;
using TheTechIdea.OilGas.Wellbore.Data;

namespace TheTechIdea.Beep.OilandGas.Desktop.WellTools.Environment;

/// <summary>
/// Registers the OilandGasTools toolboxes with <see cref="ToolboxRegistry"/>.
/// </summary>
/// <remarks>
/// <b>Order matters.</b> The environment's <c>toolboxes</c> section is only validated for toolboxes that
/// are already registered — <c>WellboreEnvironment.Validate()</c> walks <c>ToolboxRegistry.All</c>. A
/// host that loads the environment first gets a section nobody checks and no warning to say so. This
/// class exists so the order lives in one place, and VSB-02's test proves it.
/// </remarks>
public static class OgToolboxBootstrap
{
    private static readonly object Gate = new();

    /// <summary>Toolbox names this bootstrap is responsible for.</summary>
    public static IReadOnlyList<string> Expected { get; } =
    [
        IntegrityToolbox.Name,
        DrillingToolbox.Name,
        ProductionToolbox.Name,
        DataAIToolbox.Name,
        TrainingToolbox.Name,
        MappingToolbox.Name,
    ];

    /// <summary>True once <see cref="RegisterAll"/> has run at least once in this process.</summary>
    public static bool HasRun { get; private set; }

    /// <summary>
    /// Registers every toolbox. Idempotent: the registry replaces a name, so calling it again after a
    /// plug-in unload restores the registration instead of leaving a silent gap. Returns the number of
    /// registered toolboxes afterwards.
    /// </summary>
    public static int RegisterAll()
    {
        lock (Gate)
        {
            IntegrityToolbox.Register();
            DrillingToolbox.Register();
            ProductionToolbox.Register();
            DataAIToolbox.Register();
            TrainingToolbox.Register();
            MappingToolbox.Register();

            HasRun = true;
            return ToolboxRegistry.All.Count;
        }
    }

    /// <summary>The registered toolbox names, sorted.</summary>
    public static IReadOnlyList<string> RegisteredNames =>
        ToolboxRegistry.All.Select(registration => registration.Name).ToList();

    /// <summary>Which of <see cref="Expected"/> are missing from the registry.</summary>
    public static IReadOnlyList<string> Missing() =>
        Expected.Where(name => !ToolboxRegistry.All.Any(registration =>
                string.Equals(registration.Name, name, StringComparison.OrdinalIgnoreCase)))
            .ToList();
}
