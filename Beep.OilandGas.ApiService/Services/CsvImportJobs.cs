using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using TheTechIdea.Beep.Editor;
using FileImportResult = Beep.OilandGas.Models.Data.DataManagement.FileImportResult;

namespace Beep.OilandGas.ApiService.Services;


public sealed class CsvImportExecutor(IDMEEditor editor, ICommonColumnHandler columns,
    IPPDM39DefaultsRepository defaults, IPPDMMetadataRepository metadata,
    ILoggerFactory loggers, IProgressTrackingService progress, CompanyModuleConnection company, Beep.OilandGas.PPDM39.Core.Security.IPPDMRowAccessPolicy rows) : ICsvImportExecutor
{
    public async Task<FileImportResult> ExecuteAsync(CsvImportJob job, string path)
    {
        await company.RequireJobAsync(job, default);
        if (!company.Company.Permissions.Contains(Beep.OilandGas.Models.Data.Security.PermissionConstants.DataManagement.ImportData, StringComparer.Ordinal))
            throw Beep.OilandGas.Models.Core.Refusals.RefusalException.Forbidden("DataManagement.ImportData is required for a CSV import.");
        var entityType = typeof(Beep.OilandGas.PPDM.Models.IPPDMEntity).Assembly.GetTypes()
            .First(t => !t.IsAbstract && !t.IsInterface &&
                typeof(Beep.OilandGas.PPDM.Models.IPPDMEntity).IsAssignableFrom(t) &&
                t.Name.Equals(job.TableName, StringComparison.OrdinalIgnoreCase));
        var repository = new PPDMGenericRepository(editor, columns, defaults, metadata,
            entityType, job.ConnectionName, job.TableName, loggers.CreateLogger<PPDMGenericRepository>(), rows);
        return await repository.ImportFromCsvAsync(path, job.UserId, columnMapping: null, skipHeaderRow: true,
            validateForeignKeys: job.ValidateForeignKeys,
            onProgress: (id, percent, message, done, total) => progress.UpdateProgress(id, percent, message, done, total),
            operationId: job.OperationId);
    }
}
