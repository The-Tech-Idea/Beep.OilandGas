using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using TheTechIdea.Beep.Editor;
using FileImportResult = Beep.OilandGas.PPDM39.Core.FileImportResult;

namespace Beep.OilandGas.ApiService.Services;

public sealed record CsvImportJob(string OperationId, string TableName, string ConnectionName,
    string UserId, bool ValidateForeignKeys, byte[] Content)
{
    public const int MaxUploadBytes = 2 * 1024 * 1024;
    public static string QueueKey(string operationId) => $"csv-import:{operationId}";
}

public interface ICsvImportExecutor
{
    Task<FileImportResult> ExecuteAsync(CsvImportJob job, string path);
}

public sealed class CsvImportExecutor(IDMEEditor editor, ICommonColumnHandler columns,
    IPPDM39DefaultsRepository defaults, IPPDMMetadataRepository metadata,
    ILoggerFactory loggers, IProgressTrackingService progress) : ICsvImportExecutor
{
    public Task<FileImportResult> ExecuteAsync(CsvImportJob job, string path)
    {
        var entityType = typeof(Beep.OilandGas.PPDM.Models.IPPDMEntity).Assembly.GetTypes()
            .First(t => !t.IsAbstract && !t.IsInterface &&
                typeof(Beep.OilandGas.PPDM.Models.IPPDMEntity).IsAssignableFrom(t) &&
                t.Name.Equals(job.TableName, StringComparison.OrdinalIgnoreCase));
        var repository = new PPDMGenericRepository(editor, columns, defaults, metadata,
            entityType, job.ConnectionName, job.TableName, loggers.CreateLogger<PPDMGenericRepository>());
        return repository.ImportFromCsvAsync(path, job.UserId, columnMapping: null, skipHeaderRow: true,
            validateForeignKeys: job.ValidateForeignKeys,
            onProgress: (id, percent, message, done, total) => progress.UpdateProgress(id, percent, message, done, total),
            operationId: job.OperationId);
    }
}

public sealed class CsvImportJobRunner(ICsvImportExecutor executor, IProgressTrackingService progress)
{
    public async Task RunAsync(CsvImportJob job, CancellationToken token)
    {
        try
        {
            token.ThrowIfCancellationRequested();
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
