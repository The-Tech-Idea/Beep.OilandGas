using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;

namespace TheTechIdea.Beep.OilandGas.Desktop.Services;

/// <summary>The result of asking whether the installation needs repairing.</summary>
/// <param name="Needed">Always false: the installation is repaired by setup, not by a repair run.</param>
/// <param name="DatabasePath">The database the installation uses, for display.</param>
/// <param name="Reason">Why a repair run is not what to do.</param>
public sealed record OgRepairAssessment(bool Needed, string DatabasePath, string Reason);

/// <summary>
/// The old repair run, reduced to what is still true.
/// </summary>
/// <remarks>
/// <para>
/// Repair existed to recreate a <b>local database file</b> when it went missing while the company record and the
/// administrator lived beside it in <c>setup.json</c>. That file is gone: the company, its settings and its accounts
/// now live in the database itself, so a missing database is not a missing file to recreate — it is an installation
/// whose data is gone, and the startup gate sends that case to setup, which creates the database and the company
/// again.
/// </para>
/// <para>
/// The type is kept because the shell and the diagnostics page ask the question; the honest answer is now "setup,
/// not repair". <see cref="RepairAsync"/> refuses, so no caller can believe it restored anything.
/// </para>
/// </remarks>
public sealed class OgRepairService
{
    public OgRepairService(string? databasePath = null)
    {
        DatabasePath = databasePath ?? OgPaths.LocalDatabasePath;
    }

    public string DatabasePath { get; }

    public OgRepairAssessment Assess() =>
        new(
            Needed: false,
            DatabasePath,
            "The company, its settings and its accounts live in the installation's database. If it is missing or " +
            "unreachable, start the application again and run setup against it; there is no separate repair.");

    public Task<OgResult> RepairAsync(
        IOgConnectionCatalog connections,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connections);

        return Task.FromResult(OgResult.Failed(
            "There is no local database to repair. Start the application again and run setup against the database.",
            "repair-not-applicable"));
    }
}
