using System.Data;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.Repository;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>
/// The lifecycle module's connection for background work, or null while the work has to wait: the repository not ready,
/// the module not bound, its datasource not open, or its bound target not confirmed.
/// </summary>
/// <remarks>
/// OILGAS-CATCH-01. Every exception — a module not yet bound as much as an unreachable database — was answered null with a
/// debug line. Now a module not yet bound is asked (<see cref="ModuleConnectionResolver.FindAsync"/>), and only confirming
/// a bound target is caught: the work still waits, and the reason is reported.
/// </remarks>
public sealed class WorkflowBackgroundConnection(IRepositoryReadinessService readiness,
    ModuleConnectionResolver resolver, IDMEEditor editor, IEnumerable<IModuleSetup> modules,
    IFailureReporter failures)
{
    public async Task<string?> ResolveAsync(CancellationToken cancellationToken)
    {
        if (await readiness.CheckAsync(cancellationToken) != RepositoryReadiness.Ready) return null;
        var module = modules.Single(x => x.ModuleId == "LIFECYCLE");
        var name = await resolver.FindAsync(module.ModuleId, cancellationToken);
        if (name is null) return null;
        var source = editor.GetDataSource(name);
        if (source is null) return null;
        try
        {
            MigrationConnectionTarget.Validate(editor, source, name);
            if (source.Openconnection() != ConnectionState.Open) return null;
            ModuleSchemaVerification.Verify(editor, source, module.EntityTypes);
            return name;
        }
        // Whatever stops the bound target being confirmed — a cached datasource that no longer matches it, a schema not
        // installed, the database unreachable — the background work waits for its next run, and the reason is reported.
        // The host stopping is not caught.
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            failures.ReportHandled(exception, "confirming the lifecycle module's database for background work",
                "the lifecycle background work waits and tries again on its next run", FailureSeverity.Degraded);
            return null;
        }
    }
}
