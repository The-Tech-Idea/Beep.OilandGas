using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.Models.Core.Interfaces;
using FileImportResult = Beep.OilandGas.Models.Data.DataManagement.FileImportResult;

namespace Beep.OilandGas.ApiService.Services;

public interface ICsvImportExecutor
{
    Task<FileImportResult> ExecuteAsync(CsvImportJob job, string path);
}

public sealed class CsvImportJobRunner(ICsvImportExecutor executor, IProgressTrackingService progress, CompanyModuleConnection company)
{
    public async Task RunAsync(CsvImportJob job, CancellationToken token)
    {
        try
        {
            token.ThrowIfCancellationRequested();
            await company.RequireJobAsync(job, token);
            var path = Path.Combine(Path.GetTempPath(), $"beep-import-{Guid.NewGuid():N}.csv");
            FileImportResult result;
            try
            {
                await File.WriteAllBytesAsync(path, job.Content, token);
                token.ThrowIfCancellationRequested();
                // The repository has no cancellation token. Await it fully before deleting its input or disposing its scope.
                result = await executor.ExecuteAsync(job, path);
            }
            finally { File.Delete(path); }
            if (result.ErrorCount > 0 || result.Errors.Count > 0)
                throw new InvalidOperationException("CSV import reported row errors; some rows may have been written.");
            progress.CompleteOperation(job.OperationId, true, $"Import completed: {result.SuccessCount} rows imported.");
        }
        catch
        {
            progress.CompleteOperation(job.OperationId, false, errorMessage: "Import failed or stopped. Review server logs and imported rows before retrying.");
            throw;
        }
    }
}
