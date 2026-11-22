namespace TheTechIdea.Beep.OilandGas.Desktop.Core;

/// <summary>
/// The commercial module keys, derived from the 29 <c>ModuleId</c> values the owning projects
/// register. A page is gated by its module key and by the signed-in roles; both must pass.
/// </summary>
public static class OgModuleKeys
{
    public const string Core = "core";
    public const string Data = "data";
    public const string Wells = "wells";
    public const string Exploration = "exploration";
    public const string Lease = "lease";
    public const string Development = "development";
    public const string Drilling = "drilling";
    public const string Production = "production";
    public const string Reservoir = "reservoir";
    public const string Properties = "properties";
    public const string Forecasting = "forecasting";
    public const string Lift = "lift";
    public const string WellTest = "welltest";
    public const string Pipeline = "pipeline";
    public const string Economics = "economics";
    public const string Accounting = "accounting";
    public const string Hse = "hse";
    public const string Permits = "permits";
    public const string Decommissioning = "decommissioning";
    public const string Work = "work";
    public const string Workflow = "workflow";
    public const string Interop = "interop";
    public const string Training = "training";
    public const string Reporting = "reporting";
    public const string Administration = "administration";

    public static IReadOnlyList<string> All { get; } =
    [
        Core, Data, Wells, Exploration, Lease, Development, Drilling, Production, Reservoir,
        Properties, Forecasting, Lift, WellTest, Pipeline, Economics, Accounting, Hse, Permits,
        Decommissioning, Work, Workflow, Interop, Training, Reporting, Administration
    ];
}
