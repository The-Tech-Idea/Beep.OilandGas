using System.Text.Json;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Validation;
using Beep.OilandGas.Models.DTOs.Validation;
using Beep.OilandGas.ProductionAccounting.Models;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ProductionAccounting.Validation
{
    /// <summary>
    /// Service for managing validation operations.
    /// Uses PPDMGenericRepository for database operations.
    /// </summary>
    public class ValidationService : IValidationService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<ValidationService>? _logger;
        private readonly string _connectionName;
        private const string VALIDATION_RESULT_TABLE = "VALIDATION_RESULT";

        public ValidationService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<ValidationService>? logger = null,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
            _connectionName = connectionName ?? "PPDM39";
        }

        /// <summary>
        /// Validates an entity.
        /// </summary>
        public async Task<ValidationResultResponse> ValidateEntityAsync<T>(T entity, string entityType, string userId, string? connectionName = null)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (string.IsNullOrEmpty(entityType))
                throw new ArgumentException("Entity type is required.", nameof(entityType));

            var connName = connectionName ?? _connectionName;
            ValidationResult validationResult;

            // Route to appropriate validator based on entity type
            switch (entityType.ToLower())
            {
                case "lease":
                case "leaseagreement":
                    if (entity is LeaseAgreement lease)
                    {
                        try
                        {
                            LeaseValidator.Validate(lease);
                            validationResult = new ValidationResult();
                        }
                        catch (Exceptions.InvalidLeaseDataException ex)
                        {
                            validationResult = new ValidationResult();
                            validationResult.AddError(ex.FieldName ?? "Lease", ex.Message);
                        }
                    }
                    else
                    {
                        validationResult = new ValidationResult();
                        validationResult.AddError("Entity", "Entity is not a LeaseAgreement.");
                    }
                    break;

                case "crudeoil":
                case "crudeoilproperties":
                    if (entity is CrudeOilProperties properties)
                    {
                        try
                        {
                            CrudeOilValidator.Validate(properties);
                            validationResult = new ValidationResult();
                        }
                        catch (Exceptions.InvalidCrudeOilDataException ex)
                        {
                            validationResult = new ValidationResult();
                            validationResult.AddError(ex.FieldName ?? "Properties", ex.Message);
                        }
                    }
                    else
                    {
                        validationResult = new ValidationResult();
                        validationResult.AddError("Entity", "Entity is not CrudeOilProperties.");
                    }
                    break;

                default:
                    validationResult = new ValidationResult();
                    validationResult.AddWarning("EntityType", $"No specific validator found for entity type: {entityType}");
                    break;
            }

            // Convert to response DTO
            var response = new ValidationResultResponse
            {
                ValidationId = Guid.NewGuid().ToString(),
                EntityType = entityType,
                EntityId = GetEntityId(entity),
                IsValid = validationResult.IsValid,
                ValidationDate = DateTime.UtcNow
            };

            response.Errors = validationResult.Errors.Select(e => new ValidationIssueDto
            {
                Field = e.Field,
                Message = e.Message
            }).ToList();

            response.Warnings = validationResult.Warnings.Select(w => new ValidationIssueDto
            {
                Field = w.Field,
                Message = w.Message
            }).ToList();

            // Save to database
            await SaveValidationResultAsync(response, userId, connName);

            _logger?.LogDebug("Validated {EntityType} entity {EntityId}: {IsValid}",
                entityType, response.EntityId, response.IsValid);

            return response;
        }

        /// <summary>
        /// Validates crude oil properties.
        /// </summary>
        public async Task<ValidationResultResponse> ValidateCrudeOilPropertiesAsync(object properties, string userId, string? connectionName = null)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            if (properties is CrudeOilProperties crudeOilProperties)
            {
                return await ValidateEntityAsync(crudeOilProperties, "CrudeOilProperties", userId, connectionName);
            }

            throw new ArgumentException("Properties must be of type CrudeOilProperties.", nameof(properties));
        }

        /// <summary>
        /// Validates a lease.
        /// </summary>
        public async Task<ValidationResultResponse> ValidateLeaseAsync(object lease, string userId, string? connectionName = null)
        {
            if (lease == null)
                throw new ArgumentNullException(nameof(lease));

            if (lease is LeaseAgreement leaseAgreement)
            {
                return await ValidateEntityAsync(leaseAgreement, "LeaseAgreement", userId, connectionName);
            }

            throw new ArgumentException("Lease must be of type LeaseAgreement.", nameof(lease));
        }

        /// <summary>
        /// Saves validation result.
        /// </summary>
        public async Task<VALIDATION_RESULT> SaveValidationResultAsync(ValidationResultResponse result, string userId, string? connectionName = null)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            var connName = connectionName ?? _connectionName;
            var repo = await GetValidationResultRepositoryAsync(connName);

            // Serialize errors and warnings to JSON
            var errorsJson = JsonSerializer.Serialize(result.Errors);
            var warningsJson = JsonSerializer.Serialize(result.Warnings);

            var entity = new VALIDATION_RESULT
            {
                VALIDATION_RESULT_ID = result.ValidationId,
                ENTITY_TYPE = result.EntityType,
                ENTITY_ID = result.EntityId,
                VALIDATION_DATE = result.ValidationDate,
                IS_VALID = result.IsValid ? "Y" : "N",
                VALIDATION_ERRORS = errorsJson,
                VALIDATION_WARNINGS = warningsJson,
                VALIDATED_BY = userId,
                ACTIVE_IND = "Y"
            };

            if (entity is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForCreateAsync(ppdmEntity, userId, connName);
            }

            await repo.InsertAsync(entity);

            _logger?.LogDebug("Saved validation result {ValidationId} for entity {EntityType} {EntityId}",
                result.ValidationId, result.EntityType, result.EntityId);

            return entity;
        }

        /// <summary>
        /// Gets validation history.
        /// </summary>
        public async Task<List<VALIDATION_RESULT>> GetValidationHistoryAsync(string? entityType, string? entityId, string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var repo = await GetValidationResultRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (!string.IsNullOrEmpty(entityType))
            {
                filters.Add(new AppFilter { FieldName = "ENTITY_TYPE", Operator = "=", FilterValue = entityType });
            }

            if (!string.IsNullOrEmpty(entityId))
            {
                filters.Add(new AppFilter { FieldName = "ENTITY_ID", Operator = "=", FilterValue = entityId });
            }

            var results = await repo.GetAsync(filters);
            return results.Cast<VALIDATION_RESULT>().OrderByDescending(v => v.VALIDATION_DATE).ToList();
        }

        /// <summary>
        /// Creates a validation rule.
        /// </summary>
        public async Task<ValidationRuleResponse> CreateValidationRuleAsync(CreateValidationRuleRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // In a full implementation, would save to VALIDATION_RULE table
            // For now, return a basic response
            var rule = new ValidationRuleResponse
            {
                RuleId = Guid.NewGuid().ToString(),
                EntityType = request.EntityType,
                RuleName = request.RuleName,
                RuleDefinition = request.RuleDefinition,
                RuleType = request.RuleType,
                IsActive = request.IsActive
            };

            _logger?.LogDebug("Created validation rule {RuleId} for entity type {EntityType}",
                rule.RuleId, request.EntityType);

            return rule;
        }

        /// <summary>
        /// Gets validation rules.
        /// </summary>
        public async Task<List<ValidationRuleResponse>> GetValidationRulesAsync(string? entityType, string? connectionName = null)
        {
            // In a full implementation, would query VALIDATION_RULE table
            // For now, return empty list
            return new List<ValidationRuleResponse>();
        }

        /// <summary>
        /// Gets validation summary.
        /// </summary>
        public async Task<ValidationSummary> GetValidationSummaryAsync(string? entityType, DateTime? startDate, DateTime? endDate, string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var history = await GetValidationHistoryAsync(entityType, null, connName);

            var filteredHistory = history.AsEnumerable();

            if (startDate.HasValue)
            {
                filteredHistory = filteredHistory.Where(v => v.VALIDATION_DATE >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                filteredHistory = filteredHistory.Where(v => v.VALIDATION_DATE <= endDate.Value);
            }

            var historyList = filteredHistory.ToList();
            var totalValidations = historyList.Count;
            var validCount = historyList.Count(v => v.IS_VALID == "Y");
            var invalidCount = historyList.Count(v => v.IS_VALID == "N");
            var warningCount = historyList.Count(v => !string.IsNullOrEmpty(v.VALIDATION_WARNINGS));

            var summary = new ValidationSummary
            {
                EntityType = entityType ?? "All",
                StartDate = startDate,
                EndDate = endDate,
                TotalValidations = totalValidations,
                ValidCount = validCount,
                InvalidCount = invalidCount,
                WarningCount = warningCount,
                ValidationSuccessRate = totalValidations > 0 ? (decimal)validCount / totalValidations * 100m : 0m
            };

            _logger?.LogDebug("Retrieved validation summary for {EntityType}: {TotalValidations} validations, {ValidCount} valid, {InvalidCount} invalid",
                entityType, totalValidations, validCount, invalidCount);

            return summary;
        }

        // Helper method to extract entity ID
        private string GetEntityId<T>(T entity)
        {
            if (entity == null)
                return string.Empty;

            // Try to get common ID properties
            var type = typeof(T);
            var idProperty = type.GetProperty("Id") ??
                            type.GetProperty("EntityId") ??
                            type.GetProperty("LeaseId") ??
                            type.GetProperty("PropertyId") ??
                            type.GetProperty("WellId");

            if (idProperty != null)
            {
                var value = idProperty.GetValue(entity);
                return value?.ToString() ?? string.Empty;
            }

            return string.Empty;
        }

        // Repository helper methods
        private async Task<PPDMGenericRepository> GetValidationResultRepositoryAsync(string connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(VALIDATION_RESULT_TABLE);
            var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Validation.{metadata.EntityTypeName}");

            if (entityType == null)
            {
                entityType = typeof(VALIDATION_RESULT);
            }

            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, VALIDATION_RESULT_TABLE,
                null);
        }
    }
}
