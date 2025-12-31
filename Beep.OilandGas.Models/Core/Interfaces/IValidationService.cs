using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Validation;
using Beep.OilandGas.Models.DTOs.Validation;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for validation operations.
    /// </summary>
    public interface IValidationService
    {
        /// <summary>
        /// Validates an entity.
        /// </summary>
        Task<ValidationResultResponse> ValidateEntityAsync<T>(T entity, string entityType, string userId, string? connectionName = null);
        
        /// <summary>
        /// Validates crude oil properties.
        /// </summary>
        Task<ValidationResultResponse> ValidateCrudeOilPropertiesAsync(object properties, string userId, string? connectionName = null);
        
        /// <summary>
        /// Validates a lease.
        /// </summary>
        Task<ValidationResultResponse> ValidateLeaseAsync(object lease, string userId, string? connectionName = null);
        
        /// <summary>
        /// Saves validation result.
        /// </summary>
        Task<VALIDATION_RESULT> SaveValidationResultAsync(ValidationResultResponse result, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets validation history.
        /// </summary>
        Task<List<VALIDATION_RESULT>> GetValidationHistoryAsync(string? entityType, string? entityId, string? connectionName = null);
        
        /// <summary>
        /// Creates a validation rule.
        /// </summary>
        Task<ValidationRuleResponse> CreateValidationRuleAsync(CreateValidationRuleRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets validation rules.
        /// </summary>
        Task<List<ValidationRuleResponse>> GetValidationRulesAsync(string? entityType, string? connectionName = null);
        
        /// <summary>
        /// Gets validation summary.
        /// </summary>
        Task<ValidationSummary> GetValidationSummaryAsync(string? entityType, DateTime? startDate, DateTime? endDate, string? connectionName = null);
    }
}

