using System.Data;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.Repository;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

public sealed class WorkflowBackgroundConnection(IRepositoryReadinessService readiness,
    ModuleConnectionResolver resolver, IDMEEditor editor, IEnumerable<IModuleSetup> modules,
    ILogger<WorkflowBackgroundConnection> logger)
{
    public async Task<string?> ResolveAsync(CancellationToken cancellationToken)
    {
        if (await readiness.CheckAsync(cancellationToken) != RepositoryReadiness.Ready) return null;
        try
        {
            var module = modules.Single(x => x.ModuleId == "LIFECYCLE");
            var name = await resolver.ResolveAsync(module.ModuleId, cancellationToken);
            var source = editor.GetDataSource(name);
            if (source is null) return null;
            MigrationConnectionTarget.Validate(editor, source, name);
            if (source.Openconnection() != ConnectionState.Open) return null;
            ModuleSchemaVerification.Verify(editor, source, module.EntityTypes);
            return name;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception exception)
        {
            logger.LogDebug(exception, "Lifecycle background work is waiting for a valid installed module binding");
            return null;
        }
    }
}
