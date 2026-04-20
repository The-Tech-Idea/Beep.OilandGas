using System;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;

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
            string projectId)
        {
            if (string.IsNullOrWhiteSpace(projectId))
                throw new ArgumentNullException(nameof(projectId));

            var operation = await GetEnhancedRecoveryOperationAsync(projectId);
            if (operation != null)
                return operation;

            // Project not found — return an empty record with the requested ID
            return new EnhancedRecoveryOperation
            {
                OperationId = projectId,
                Status      = "NOT_FOUND"
            };
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
                existing.InjectionRate = injectionRate;
                return existing;
            }

            // Create a new injection operation record
            var dto = new CreateEnhancedRecoveryOperation
            {
                EORType              = "INJECTION",
                PlannedInjectionRate = injectionRate,
                PlannedStartDate     = DateTime.UtcNow,
            };

            var created = await CreateEnhancedRecoveryOperationAsync(dto);

            return new InjectionOperation
            {
                OperationId   = created.OperationId,
                WellUWI       = injectionWellId,
                InjectionType = "INJECTION",
                InjectionRate = injectionRate,
                OperationDate = DateTime.UtcNow,
                Status        = "Active"
            };
        }
    }
}
