using System.Data;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Runs existing module setup on an explicitly selected, already configured connection.</summary>
public sealed class SetupWizardExecutor(IDMEEditor editor, IEnumerable<IModuleSetup> modules,
    ILogger<PpdmModuleSeedingService> logger) : ISetupWizardExecutor
{
    public async Task ExecuteAsync(SetupWizardJob job, IProgress<WizardProgress> progress, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (editor.OpenDataSource(job.ConnectionName) != ConnectionState.Open)
            throw new InvalidOperationException("The selected setup connection could not be opened.");
        var seeding = new PpdmModuleSeedingService(modules, editor, job.ConnectionName, logger);
        var report = await seeding.SeedAllAsync(job.UserId,
            new InlineProgress<(int done, int total, string message)>(p => progress.Report(new(p.done, p.total, p.message))), token);
        token.ThrowIfCancellationRequested();
        if (!report.Completed || report.Succeeded != report.TotalModules || report.Results.Any(r => r.Errors.Count > 0))
            throw new InvalidOperationException("One or more setup modules failed.");
    }
}
