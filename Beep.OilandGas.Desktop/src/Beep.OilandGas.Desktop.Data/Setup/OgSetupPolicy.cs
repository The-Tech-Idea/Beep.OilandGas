using TheTechIdea.Beep.OilandGas.Desktop.Core;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Setup;

/// <summary>
/// What a person confirmed before an apply run (DAT-02 AC-5). The demo dataset is only loaded when this names
/// the connection being set up, so a confirmation cannot be carried over to another database or arrive by
/// accident with a default.
/// </summary>
/// <param name="ConnectionName">The connection the person named in the confirmation.</param>
/// <param name="IncludeDemoData">True when they asked for the demo dataset.</param>
public sealed record OgSetupConfirmation(string ConnectionName, bool IncludeDemoData)
{
    public static OgSetupConfirmation None { get; } = new(string.Empty, IncludeDemoData: false);
}

/// <summary>
/// The two decisions DAT-02 leaves to policy rather than to the runner's loop: whether the demo dataset may be
/// loaded (AC-5), and what to say when it may not. Pure, so the rule is tested without a database.
/// </summary>
public static class OgSetupPolicy
{
    /// <summary>The module that loads the demo dataset.</summary>
    public const string DemoModuleId = "DEMO_DATA";

    /// <summary>True when the module loads demo data rather than reference data.</summary>
    public static bool IsDemoModule(string? moduleId) =>
        string.Equals(moduleId, DemoModuleId, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// True when this module may run under this confirmation. Everything except the demo module may; the demo
    /// module needs a confirmation that names <paramref name="connectionName"/> exactly (case-insensitively).
    /// </summary>
    public static bool MayRun(string? moduleId, OgSetupConfirmation? confirmation, string connectionName)
    {
        if (!IsDemoModule(moduleId))
        {
            return true;
        }

        return confirmation is { IncludeDemoData: true }
               && !string.IsNullOrWhiteSpace(confirmation.ConnectionName)
               && string.Equals(confirmation.ConnectionName.Trim(), connectionName, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>The sentence recorded against a demo module that was not run, naming the connection it would need.</summary>
    public static string DemoSkipReason(string connectionName) =>
        $"Demo data was not loaded: it needs an explicit confirmation naming the connection '{connectionName}'.";
}
