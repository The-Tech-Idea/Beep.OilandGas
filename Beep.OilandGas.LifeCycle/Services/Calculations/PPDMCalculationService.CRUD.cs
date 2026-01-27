using System;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.LifeCycle.Services.Calculations
{
    public partial class PPDMCalculationService
    {
        // InsertAnalysisResultAsync is implemented in the common helper to perform mapping from DTO to PPDM entity

        private async Task<PPDMGenericRepository> CreateAnalysisResultRepositoryAsync(string tableName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            if (metadata == null)
            {
                throw new InvalidOperationException($"Metadata for table {tableName} not found");
            }

            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(object);

            return new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, _connectionName, tableName, null);
        }

        private async Task<PPDMGenericRepository> GetDCAResultRepositoryAsync()
        {
            return await CreateAnalysisResultRepositoryAsync("DCA_RESULT");
        }

        private async Task<PPDMGenericRepository> GetEconomicResultRepositoryAsync()
        {
            return await CreateAnalysisResultRepositoryAsync("ECONOMIC_ANALYSIS_RESULT");
        }

        private async Task<PPDMGenericRepository> GetNodalResultRepositoryAsync()
        {
            return await CreateAnalysisResultRepositoryAsync("NODAL_ANALYSIS_RESULT");
        }

        private async Task<PPDMGenericRepository> GetWellTestResultRepositoryAsync()
        {
            return await CreateAnalysisResultRepositoryAsync("WELL_TEST_ANALYSIS_RESULT");
        }

        private async Task<PPDMGenericRepository> GetFlashResultRepositoryAsync()
        {
            return await CreateAnalysisResultRepositoryAsync("FLASH_CALCULATION_RESULT");
        }
    }
}
