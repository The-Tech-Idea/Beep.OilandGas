using System;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.ConfigUtil;

namespace Beep.OilandGas.EnhancedRecovery.Services
{
    public partial class EnhancedRecoveryService
    {
        // Explicit implementations of Models.Core.Interfaces.IEnhancedRecoveryService

        async Task<EnhancedRecoveryOperation> Beep.OilandGas.Models.Core.Interfaces.IEnhancedRecoveryService.AnalyzeEORPotentialAsync(
            string fieldId, string eorMethod)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentNullException(nameof(fieldId));

            // Query existing EOR operations for this field and method
            var operations = await GetEnhancedRecoveryOperationsAsync(fieldId);
            var existing = operations.FirstOrDefault(o =>
                string.Equals(o.EORType, eorMethod, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
                return existing;

            // No existing record — create a new analysis entry
            var dto = new CreateEnhancedRecoveryOperation
            {
                FieldId           = fieldId,
                EORType           = eorMethod,
                PlannedStartDate  = DateTime.UtcNow,
            };

            return await CreateEnhancedRecoveryOperationAsync(dto);
        }

        async Task<EnhancedRecoveryOperation> Beep.OilandGas.Models.Core.Interfaces.IEnhancedRecoveryService.CalculateRecoveryFactorAsync(
            string operationId)
        {
            if (string.IsNullOrWhiteSpace(operationId))
                throw new ArgumentNullException(nameof(operationId));

            var operation = await GetEnhancedRecoveryOperationAsync(operationId);
            if (operation != null)
                return operation;

            throw new InvalidOperationException($"Enhanced recovery operation {operationId} was not found.");
        }

        async Task<System.Collections.Generic.List<InjectionOperation>> Beep.OilandGas.Models.Core.Interfaces.IEnhancedRecoveryService.GetInjectionOperationsAsync(
            string? wellUWI)
        {
            return await GetInjectionOperationsAsync(wellUWI);
        }

        async Task<EOREconomicAnalysis> Beep.OilandGas.Models.Core.Interfaces.IEnhancedRecoveryService.AnalyzeEOReconomicsAsync(
            string fieldId,
            double estimatedIncrementalOil,
            double oilPrice,
            double capitalCost,
            double operatingCostPerBarrel,
            int projectLifeYears,
            double discountRate)
        {
            return await AnalyzeEOReconomicsAsync(
                fieldId,
                estimatedIncrementalOil,
                oilPrice,
                capitalCost,
                operatingCostPerBarrel,
                projectLifeYears,
                discountRate);
        }

        async Task<InjectionOperation> Beep.OilandGas.Models.Core.Interfaces.IEnhancedRecoveryService.ManageInjectionAsync(
            string injectionWellId, decimal injectionRate)
        {
            if (string.IsNullOrWhiteSpace(injectionWellId))
                throw new ArgumentNullException(nameof(injectionWellId));

            // Look for an existing active injection operation on this well
            var operations = await GetInjectionOperationsAsync(injectionWellId);
            var existing = operations.FirstOrDefault(o =>
                string.Equals(o.Status, "Active", StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                var pdenUow = GetPDENUnitOfWork();
                var pden = pdenUow.Read(existing.OperationId) as PDEN;
                if (pden == null)
                    throw new InvalidOperationException($"Injection operation {existing.OperationId} was not found.");

                var now = DateTime.UtcNow;
                pden.ACTIVE_IND = "Y";
                pden.PDEN_STATUS = "ACTIVE";
                pden.CURRENT_STATUS_DATE = now;
                pden.LAST_INJECTION_DATE = now;
                pden.ROW_CHANGED_BY = "SYSTEM";
                pden.ROW_CHANGED_DATE = now;

                var updateResult = await pdenUow.UpdateDoc(pden);
                if (updateResult.Flag != Errors.Ok)
                    throw new InvalidOperationException($"Failed to update injection operation {existing.OperationId}: {updateResult.Message}");

                await pdenUow.Commit();
                await UpsertFlowMeasurementAsync(pden, injectionRate, existing.InjectionRateUnit ?? "BBL/D");

                return await MapInjectionOperationAsync(pden);
            }

            var created = await CreateInjectionOperationAsync(injectionWellId, injectionRate);
            return await MapInjectionOperationAsync(created);
        }
    }
}
