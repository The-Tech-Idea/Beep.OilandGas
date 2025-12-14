using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.DTOs;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.PPDM39.DataManagement.Services
{
    /// <summary>
    /// Service for validating PPDM entities against business rules
    /// Provides simple and efficient validation for oil and gas data
    /// </summary>
    public class PPDMDataValidationService : IPPDMDataValidationService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;

        public PPDMDataValidationService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName;
        }

        /// <summary>
        /// Validates an entity against business rules
        /// </summary>
        public async Task<ValidationResult> ValidateAsync(object entity, string tableName)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or empty", nameof(tableName));

            var result = new ValidationResult
            {
                Entity = entity,
                TableName = tableName,
                IsValid = true
            };

            var rules = await GetValidationRulesAsync(tableName);
            var entityType = entity.GetType();

            foreach (var rule in rules.Where(r => r.IsActive))
            {
                var property = entityType.GetProperty(rule.FieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (property == null)
                    continue;

                var value = property.GetValue(entity);
                var validationError = ValidateRule(rule, value, entity, tableName);

                if (validationError != null)
                {
                    if (validationError.Severity == ValidationSeverity.Error)
                    {
                        result.IsValid = false;
                        result.Errors.Add(validationError);
                    }
                    else
                    {
                        result.Warnings.Add(new ValidationWarning
                        {
                            FieldName = rule.FieldName,
                            WarningMessage = validationError.ErrorMessage,
                            RuleName = rule.RuleName
                        });
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Validates multiple entities in batch
        /// </summary>
        public async Task<List<ValidationResult>> ValidateBatchAsync(IEnumerable<object> entities, string tableName)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            var results = new List<ValidationResult>();
            var rules = await GetValidationRulesAsync(tableName);

            foreach (var entity in entities)
            {
                var result = await ValidateAsync(entity, tableName);
                results.Add(result);
            }

            return results;
        }

        /// <summary>
        /// Gets validation rules for a table
        /// Uses metadata and defaults to build common validation rules
        /// </summary>
        public async Task<List<ValidationRule>> GetValidationRulesAsync(string tableName)
        {
            var rules = new List<ValidationRule>();
            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            if (metadata == null)
                return rules;

            // Get entity type
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");
            if (entityType == null)
                return rules;

            // Build rules from metadata
            // Note: PPDMTableMetadata may not have Columns property, so we'll use reflection
            // For now, we'll create rules based on common PPDM patterns
            var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            // Check if property is primary key
            var pkColumns = metadata.PrimaryKeyColumn.Split(',').Select(c => c.Trim()).ToList();
            
            foreach (var property in properties)
            {
                var columnName = property.Name;
                var isPrimaryKey = pkColumns.Contains(columnName, StringComparer.OrdinalIgnoreCase);
                
                // Required field rule for primary keys
                if (isPrimaryKey)
                {
                    rules.Add(new ValidationRule
                    {
                        RuleName = $"{columnName}_Required",
                        FieldName = columnName,
                        RuleType = ValidationRuleType.Required,
                        ErrorMessage = $"{columnName} is required (primary key)",
                        Severity = ValidationSeverity.Error,
                        IsActive = true
                    });
                }

                // UWI format validation (common in oil and gas)
                if (columnName.Equals("UWI", StringComparison.OrdinalIgnoreCase))
                {
                    rules.Add(new ValidationRule
                    {
                        RuleName = "UWI_Format",
                        FieldName = "UWI",
                        RuleType = ValidationRuleType.Pattern,
                        RuleValue = @"^[A-Z0-9\-]+$",
                        ErrorMessage = "UWI must contain only alphanumeric characters and hyphens",
                        Severity = ValidationSeverity.Error,
                        IsActive = true
                    });
                }

                // ACTIVE_IND validation
                if (columnName.Equals("ACTIVE_IND", StringComparison.OrdinalIgnoreCase))
                {
                    rules.Add(new ValidationRule
                    {
                        RuleName = "ACTIVE_IND_Values",
                        FieldName = "ACTIVE_IND",
                        RuleType = ValidationRuleType.Custom,
                        RuleValue = $"{_defaults.GetActiveIndicatorYes()},{_defaults.GetActiveIndicatorNo()}",
                        ErrorMessage = $"ACTIVE_IND must be '{_defaults.GetActiveIndicatorYes()}' or '{_defaults.GetActiveIndicatorNo()}'",
                        Severity = ValidationSeverity.Error,
                        IsActive = true
                    });
                }
            }

            return rules;
        }

        /// <summary>
        /// Validates a single rule
        /// </summary>
        private ValidationError ValidateRule(ValidationRule rule, object value, object entity, string tableName)
        {
            switch (rule.RuleType)
            {
                case ValidationRuleType.Required:
                    if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
                    {
                        return new ValidationError
                        {
                            FieldName = rule.FieldName,
                            ErrorMessage = rule.ErrorMessage,
                            RuleName = rule.RuleName,
                            Severity = rule.Severity
                        };
                    }
                    break;

                case ValidationRuleType.MaxLength:
                    if (value is string stringValue && int.TryParse(rule.RuleValue, out int maxLength))
                    {
                        if (stringValue.Length > maxLength)
                        {
                            return new ValidationError
                            {
                                FieldName = rule.FieldName,
                                ErrorMessage = rule.ErrorMessage,
                                RuleName = rule.RuleName,
                                Severity = rule.Severity
                            };
                        }
                    }
                    break;

                case ValidationRuleType.Pattern:
                    if (value is string patternValue && !string.IsNullOrWhiteSpace(rule.RuleValue))
                    {
                        if (!System.Text.RegularExpressions.Regex.IsMatch(patternValue, rule.RuleValue))
                        {
                            return new ValidationError
                            {
                                FieldName = rule.FieldName,
                                ErrorMessage = rule.ErrorMessage,
                                RuleName = rule.RuleName,
                                Severity = rule.Severity
                            };
                        }
                    }
                    break;

                case ValidationRuleType.Custom:
                    // For ACTIVE_IND and similar custom validations
                    if (value is string customValue && !string.IsNullOrWhiteSpace(rule.RuleValue))
                    {
                        var allowedValues = rule.RuleValue.Split(',');
                        if (!allowedValues.Contains(customValue.Trim()))
                        {
                            return new ValidationError
                            {
                                FieldName = rule.FieldName,
                                ErrorMessage = rule.ErrorMessage,
                                RuleName = rule.RuleName,
                                Severity = rule.Severity
                            };
                        }
                    }
                    break;
            }

            return null;
        }
    }
}

