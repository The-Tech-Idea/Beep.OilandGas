using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Process;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.LifeCycle.Services.Processes
{
    /// <summary>
    /// Validator for process steps and data
    /// </summary>
    public class ProcessValidator
    {
        private readonly ILogger<ProcessValidator>? _logger;

        public ProcessValidator(ILogger<ProcessValidator>? logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// Validate step data against validation rules
        /// </summary>
        public async Task<ValidationResult> ValidateStepDataAsync(
            ProcessStepDefinition stepDefinition,
            PROCESS_STEP_DATA stepData)
        {
            // Extract data from strongly-typed PROCESS_STEP_DATA using reflection
            var dataDict = new Dictionary<string, object>();
            
            // Add core properties
            if (!string.IsNullOrEmpty(stepData.StepInstanceId))
                dataDict["StepInstanceId"] = stepData.StepInstanceId;
            if (!string.IsNullOrEmpty(stepData.StepType))
                dataDict["StepType"] = stepData.StepType;
            if (!string.IsNullOrEmpty(stepData.Status))
                dataDict["Status"] = stepData.Status;
            if (stepData.LastUpdated.HasValue)
                dataDict["LastUpdated"] = stepData.LastUpdated.Value;

            // Merge Data dictionary entries
            foreach (var kvp in stepData.Data)
                dataDict[kvp.Key] = kvp.Value;

            return await ValidateStepDataAsync(stepDefinition, dataDict);
        }

        public async Task<ValidationResult> ValidateStepDataAsync(
            ProcessStepDefinition stepDefinition,
            Dictionary<string, object> stepData)
        {
            var validationResult = new ValidationResult
            {
                ValidationId = Guid.NewGuid().ToString(),
                RuleId = stepDefinition.StepId,
                IsValid = true,
                ValidatedDate = DateTime.UtcNow
            };

            if (stepDefinition.ValidationRules == null || stepDefinition.ValidationRules.Count == 0)
            {
                return validationResult;
            }

            var errors = new List<string>();

            foreach (var rule in stepDefinition.ValidationRules.Values)
            {
                var ruleResult = await ValidateRuleAsync(rule, stepData);
                if (!ruleResult.IsValid)
                {
                    validationResult.IsValid = false;
                    errors.Add(ruleResult.ErrorMessage);
                }
            }

            if (errors.Any())
            {
                validationResult.ErrorMessage = string.Join("; ", errors);
            }

            return validationResult;
        }

        /// <summary>
        /// Validate a single validation rule
        /// </summary>
        private async Task<ValidationResult> ValidateRuleAsync(
            ValidationRule rule,
            Dictionary<string, object> stepData)
        {
            var result = new ValidationResult
            {
                ValidationId = Guid.NewGuid().ToString(),
                RuleId = rule.RuleId,
                IsValid = true,
                ValidatedDate = DateTime.UtcNow
            };

            try
            {
                switch (rule.RuleType)
                {
                    case ValidationRuleType.Required:
                        result = ValidateRequired(rule, stepData);
                        break;

                    case ValidationRuleType.Range:
                        result = ValidateRange(rule, stepData);
                        break;

                    case ValidationRuleType.Format:
                    case ValidationRuleType.Pattern:
                        result = ValidateFormat(rule, stepData);
                        break;

                    case ValidationRuleType.MaxLength:
                        result = ValidateMaxLength(rule, stepData);
                        break;

                    case ValidationRuleType.MinLength:
                        result = ValidateMinLength(rule, stepData);
                        break;

                    case ValidationRuleType.DateRange:
                        result = await ValidateDateRangeAsync(rule, stepData);
                        break;

                    case ValidationRuleType.Unique:
                        result = ValidateUnique(rule, stepData);
                        break;

                    case ValidationRuleType.ForeignKey:
                        result = ValidateForeignKey(rule, stepData);
                        break;

                    case ValidationRuleType.Custom:
                        result = await ValidateCustomAsync(rule, stepData);
                        break;

                    case ValidationRuleType.BusinessRule:
                        result = await ValidateBusinessRuleAsync(rule, stepData);
                        break;

                    default:
                        _logger?.LogWarning($"Unknown validation rule type: {rule.RuleType}");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error validating rule {rule.RuleId}");
                result.IsValid = false;
                result.ErrorMessage = $"Validation error: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// Validate required field
        /// </summary>
        private ValidationResult ValidateRequired(ValidationRule rule, Dictionary<string, object> stepData)
        {
            var result = new ValidationResult
            {
                RuleId = rule.RuleId,
                IsValid = true
            };

            if (!stepData.ContainsKey(rule.FieldName) || stepData[rule.FieldName] == null)
            {
                result.IsValid = false;
                result.ErrorMessage = rule.ErrorMessage ?? $"Field {rule.FieldName} is required";
            }
            else if (stepData[rule.FieldName] is string strValue && string.IsNullOrWhiteSpace(strValue))
            {
                result.IsValid = false;
                result.ErrorMessage = rule.ErrorMessage ?? $"Field {rule.FieldName} is required";
            }

            return result;
        }

        /// <summary>
        /// Validate range
        /// </summary>
        private ValidationResult ValidateRange(ValidationRule rule, Dictionary<string, object> stepData)
        {
            var result = new ValidationResult
            {
                RuleId = rule.RuleId,
                IsValid = true
            };

            if (!stepData.ContainsKey(rule.FieldName))
            {
                return result; // Not required, skip if missing
            }

            var value = stepData[rule.FieldName];
            if (value == null)
            {
                return result;
            }

            decimal numValue;
            if (value is decimal d) numValue = d;
            else if (value is double db) numValue = (decimal)db;
            else if (value is int i) numValue = i;
            else if (value is long l) numValue = l;
            else if (!decimal.TryParse(value.ToString(), out numValue))
            {
                result.IsValid = false;
                result.ErrorMessage = rule.ErrorMessage ?? $"Field {rule.FieldName} is not a valid number";
                return result;
            }

            var hasMin = rule.RuleParameters.ContainsKey("Min");
            var hasMax = rule.RuleParameters.ContainsKey("Max");

            if (hasMin && hasMax)
            {
                var minStr = rule.RuleParameters["Min"]?.ToString();
                var maxStr = rule.RuleParameters["Max"]?.ToString();
                if (!decimal.TryParse(minStr, out var min) || !decimal.TryParse(maxStr, out var max))
                {
                    return result;
                }
                if (numValue < min || numValue > max)
                {
                    result.IsValid = false;
                    result.ErrorMessage = rule.ErrorMessage ?? $"Field {rule.FieldName} must be between {min} and {max}";
                }
            }
            else if (hasMin)
            {
                var minStr = rule.RuleParameters["Min"]?.ToString();
                if (decimal.TryParse(minStr, out var min) && numValue < min)
                {
                    result.IsValid = false;
                    result.ErrorMessage = rule.ErrorMessage ?? $"Field {rule.FieldName} must be at least {min}";
                }
            }
            else if (hasMax)
            {
                var maxStr = rule.RuleParameters["Max"]?.ToString();
                if (decimal.TryParse(maxStr, out var max) && numValue > max)
                {
                    result.IsValid = false;
                    result.ErrorMessage = rule.ErrorMessage ?? $"Field {rule.FieldName} must be at most {max}";
                }
            }

            return result;
        }

        /// <summary>
        /// Validate format
        /// </summary>
        private ValidationResult ValidateFormat(ValidationRule rule, Dictionary<string, object> stepData)
        {
            var result = new ValidationResult
            {
                RuleId = rule.RuleId,
                IsValid = true
            };

            if (!stepData.ContainsKey(rule.FieldName))
            {
                return result; // Not required, skip if missing
            }

            var value = stepData[rule.FieldName]?.ToString();
            if (string.IsNullOrEmpty(value))
            {
                return result;
            }

            if (rule.RuleParameters.ContainsKey("Pattern"))
            {
                var pattern = rule.RuleParameters["Pattern"]?.ToString();
                if (!string.IsNullOrEmpty(pattern))
                {
                    if (!System.Text.RegularExpressions.Regex.IsMatch(value, pattern))
                    {
                        result.IsValid = false;
                        result.ErrorMessage = rule.ErrorMessage ?? $"Field {rule.FieldName} does not match required format";
                    }
                }
            }

            return result;
        }

        private ValidationResult ValidateMaxLength(ValidationRule rule, Dictionary<string, object> stepData)
        {
            var result = new ValidationResult { RuleId = rule.RuleId, IsValid = true };
            if (!stepData.ContainsKey(rule.FieldName)) return result;
            var value = stepData[rule.FieldName]?.ToString();
            if (string.IsNullOrEmpty(value)) return result;
            if (rule.RuleParameters.TryGetValue("Max", out var maxObj) && int.TryParse(maxObj?.ToString(), out var max))
            {
                if (value.Length > max)
                {
                    result.IsValid = false;
                    result.ErrorMessage = rule.ErrorMessage ?? $"Field {rule.FieldName} exceeds maximum length of {max}";
                }
            }
            return result;
        }

        private ValidationResult ValidateMinLength(ValidationRule rule, Dictionary<string, object> stepData)
        {
            var result = new ValidationResult { RuleId = rule.RuleId, IsValid = true };
            if (!stepData.ContainsKey(rule.FieldName)) return result;
            var value = stepData[rule.FieldName]?.ToString();
            if (string.IsNullOrEmpty(value)) return result;
            if (rule.RuleParameters.TryGetValue("Min", out var minObj) && int.TryParse(minObj?.ToString(), out var min))
            {
                if (value.Length < min)
                {
                    result.IsValid = false;
                    result.ErrorMessage = rule.ErrorMessage ?? $"Field {rule.FieldName} must be at least {min} characters";
                }
            }
            return result;
        }

        private async Task<ValidationResult> ValidateDateRangeAsync(ValidationRule rule, Dictionary<string, object> stepData)
        {
            var result = new ValidationResult { RuleId = rule.RuleId, IsValid = true };
            if (!stepData.ContainsKey(rule.FieldName)) return result;
            var value = stepData[rule.FieldName];
            if (value == null) return result;

            if (value is DateTime fieldDate)
            {
                if (rule.RuleParameters.TryGetValue("MinDate", out var minObj) && DateTime.TryParse(minObj?.ToString(), out var minDate) && fieldDate < minDate)
                {
                    result.IsValid = false;
                    result.ErrorMessage = rule.ErrorMessage ?? $"Field {rule.FieldName} must be on or after {minDate:yyyy-MM-dd}";
                }
                if (rule.RuleParameters.TryGetValue("MaxDate", out var maxObj) && DateTime.TryParse(maxObj?.ToString(), out var maxDate) && fieldDate > maxDate)
                {
                    result.IsValid = false;
                    result.ErrorMessage = rule.ErrorMessage ?? $"Field {rule.FieldName} must be on or before {maxDate:yyyy-MM-dd}";
                }
            }
            await Task.CompletedTask;
            return result;
        }

        private ValidationResult ValidateUnique(ValidationRule rule, Dictionary<string, object> stepData)
        {
            var result = new ValidationResult { RuleId = rule.RuleId, IsValid = true };
            if (!stepData.ContainsKey(rule.FieldName) || stepData[rule.FieldName] == null) return result;
            if (rule.RuleParameters.TryGetValue("Scope", out var scope))
            {
                _logger?.LogDebug("Unique validation for {Field} in scope {Scope} (requires data layer check)", rule.FieldName, scope);
            }
            return result;
        }

        private ValidationResult ValidateForeignKey(ValidationRule rule, Dictionary<string, object> stepData)
        {
            var result = new ValidationResult { RuleId = rule.RuleId, IsValid = true };
            if (!stepData.ContainsKey(rule.FieldName) || stepData[rule.FieldName] == null) return result;
            if (rule.RuleParameters.TryGetValue("ReferenceTable", out var refTable))
            {
                _logger?.LogDebug("Foreign key validation for {Field} referencing {Table} (requires data layer check)", rule.FieldName, refTable);
            }
            return result;
        }

        private async Task<ValidationResult> ValidateCustomAsync(ValidationRule rule, Dictionary<string, object> stepData)
        {
            var result = new ValidationResult { RuleId = rule.RuleId, IsValid = true };
            if (!stepData.ContainsKey(rule.FieldName)) return result;
            if (rule.RuleParameters.TryGetValue("Expression", out var expr))
            {
                _logger?.LogDebug("Custom validation expression for {Field}: {Expression}", rule.FieldName, expr);
            }
            await Task.CompletedTask;
            return result;
        }

        /// <summary>
        /// Validate business rule
        /// </summary>
        private async Task<ValidationResult> ValidateBusinessRuleAsync(ValidationRule rule, Dictionary<string, object> stepData)
        {
            var result = new ValidationResult
            {
                RuleId = rule.RuleId,
                IsValid = true
            };

            if (!stepData.ContainsKey(rule.FieldName))
            {
                // Field not present - skip if not required
                return result;
            }

            var fieldValue = stepData[rule.FieldName];
            if (fieldValue == null)
            {
                return result;
            }

            try
            {
                // Date Range Validation: Start date < End date
                if (rule.RuleParameters.ContainsKey("CompareField"))
                {
                    var compareFieldName = rule.RuleParameters["CompareField"]?.ToString();
                    if (!string.IsNullOrEmpty(compareFieldName) && stepData.ContainsKey(compareFieldName))
                    {
                        var compareValue = stepData[compareFieldName];
                        
                        if (fieldValue is DateTime fieldDate && compareValue is DateTime compareDate)
                        {
                            var comparison = rule.RuleParameters.ContainsKey("Comparison") 
                                ? rule.RuleParameters["Comparison"]?.ToString()?.ToUpper() 
                                : "LT"; // Less Than by default

                            bool isValid = comparison switch
                            {
                                "LT" => fieldDate < compareDate,
                                "LE" => fieldDate <= compareDate,
                                "GT" => fieldDate > compareDate,
                                "GE" => fieldDate >= compareDate,
                                "EQ" => fieldDate == compareDate,
                                _ => fieldDate < compareDate
                            };

                            if (!isValid)
                            {
                                result.IsValid = false;
                                result.ErrorMessage = rule.ErrorMessage ?? 
                                    $"Field {rule.FieldName} ({fieldDate:yyyy-MM-dd}) must be {GetComparisonDescription(comparison ?? "LT")} {compareFieldName} ({compareDate:yyyy-MM-dd})";
                            }
                        }
                    }
                }

                // Numeric Range Validation: Values within acceptable ranges
                if (rule.RuleParameters.ContainsKey("MinValue") || rule.RuleParameters.ContainsKey("MaxValue"))
                {
                    decimal? minValue = null;
                    decimal? maxValue = null;

                    if (rule.RuleParameters.ContainsKey("MinValue"))
                    {
                        if (decimal.TryParse(rule.RuleParameters["MinValue"]?.ToString(), out var min))
                            minValue = min;
                    }

                    if (rule.RuleParameters.ContainsKey("MaxValue"))
                    {
                        if (decimal.TryParse(rule.RuleParameters["MaxValue"]?.ToString(), out var max))
                            maxValue = max;
                    }

                    if (minValue.HasValue || maxValue.HasValue)
                    {
                        decimal fieldNumValue = 0;
                        if (fieldValue is decimal d) fieldNumValue = d;
                        else if (fieldValue is double db) fieldNumValue = (decimal)db;
                        else if (fieldValue is int i) fieldNumValue = i;
                        else if (fieldValue is long l) fieldNumValue = l;
                        else if (decimal.TryParse(fieldValue?.ToString(), out var parsed)) fieldNumValue = parsed;

                        if (minValue.HasValue && fieldNumValue < minValue.Value)
                        {
                            result.IsValid = false;
                            result.ErrorMessage = rule.ErrorMessage ?? 
                                $"Field {rule.FieldName} ({fieldNumValue}) must be greater than or equal to {minValue.Value}";
                        }

                        if (maxValue.HasValue && fieldNumValue > maxValue.Value)
                        {
                            result.IsValid = false;
                            result.ErrorMessage = rule.ErrorMessage ?? 
                                $"Field {rule.FieldName} ({fieldNumValue}) must be less than or equal to {maxValue.Value}";
                        }
                    }
                }

                // Cross-Field Validation: Related fields are consistent
                if (rule.RuleParameters.ContainsKey("CrossFieldValidation"))
                {
                    var validationType = rule.RuleParameters["CrossFieldValidation"]?.ToString()?.ToUpper();
                    
                    if (validationType == "SUM" && rule.RuleParameters.ContainsKey("SumFields"))
                    {
                        // Validate that sum of fields equals a target value
                        var sumFields = rule.RuleParameters["SumFields"]?.ToString()?.Split(',');
                        var targetValue = rule.RuleParameters.ContainsKey("TargetValue") 
                            ? decimal.Parse(rule.RuleParameters["TargetValue"]?.ToString() ?? "0") 
                            : 0m;

                        if (sumFields != null)
                        {
                            decimal sum = 0;
                            foreach (var sumField in sumFields)
                            {
                                if (stepData.ContainsKey(sumField.Trim()))
                                {
                                    var val = stepData[sumField.Trim()];
                                    if (val is decimal d) sum += d;
                                    else if (val is double db) sum += (decimal)db;
                                    else if (val is int i) sum += i;
                                    else if (decimal.TryParse(val?.ToString(), out var parsed)) sum += parsed;
                                }
                            }

                            if (Math.Abs(sum - targetValue) > 0.01m) // Allow small floating point differences
                            {
                                result.IsValid = false;
                                result.ErrorMessage = rule.ErrorMessage ?? 
                                    $"Sum of fields ({string.Join(", ", sumFields)}) = {sum} does not equal target value {targetValue}";
                            }
                        }
                    }
                }

                // Format checks: Regex patterns
                if (rule.RuleParameters.ContainsKey("Pattern"))
                {
                    var pattern = rule.RuleParameters["Pattern"]?.ToString();
                    if (!string.IsNullOrEmpty(pattern))
                    {
                        var fieldStr = fieldValue?.ToString() ?? string.Empty;
                        if (!System.Text.RegularExpressions.Regex.IsMatch(fieldStr, pattern))
                        {
                            result.IsValid = false;
                            result.ErrorMessage = rule.ErrorMessage ?? 
                                $"Field {rule.FieldName} does not match required format pattern";
                        }
                    }
                }

                // Custom business logic examples
                // Well depth must be > 0 and < 50000 feet
                if (rule.FieldName?.ToUpper().Contains("DEPTH") == true)
                {
                    if (decimal.TryParse(fieldValue?.ToString(), out var depth))
                    {
                        if (depth <= 0 || depth >= 50000)
                        {
                            result.IsValid = false;
                            result.ErrorMessage = rule.ErrorMessage ?? 
                                $"Well depth must be greater than 0 and less than 50000 feet. Current value: {depth}";
                        }
                    }
                }

                // Production date must be <= today
                if (rule.FieldName?.ToUpper().Contains("DATE") == true && fieldValue is DateTime dateValue)
                {
                    if (dateValue > DateTime.UtcNow.Date)
                    {
                        result.IsValid = false;
                        result.ErrorMessage = rule.ErrorMessage ?? 
                            $"Date {rule.FieldName} ({dateValue:yyyy-MM-dd}) cannot be in the future";
                    }
                }

                // Cost amount must be > 0
                if (rule.FieldName?.ToUpper().Contains("COST") == true || rule.FieldName?.ToUpper().Contains("AMOUNT") == true)
                {
                    if (decimal.TryParse(fieldValue?.ToString(), out var amount))
                    {
                        if (amount <= 0)
                        {
                            result.IsValid = false;
                            result.ErrorMessage = rule.ErrorMessage ?? 
                                $"Cost/Amount field {rule.FieldName} must be greater than 0. Current value: {amount}";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error validating business rule {rule.RuleId} for field {rule.FieldName}");
                result.IsValid = false;
                result.ErrorMessage = $"Validation error: {ex.Message}";
            }

            await Task.CompletedTask;
            return result;
        }

        private string GetComparisonDescription(string comparison)
        {
            return comparison switch
            {
                "LT" => "less than",
                "LE" => "less than or equal to",
                "GT" => "greater than",
                "GE" => "greater than or equal to",
                "EQ" => "equal to",
                _ => "less than"
            };
        }

        /// <summary>
        /// Validate process completion
        /// </summary>
        public bool ValidateProcessCompletion(ProcessInstance instance, ProcessDefinition definition)
        {
            if (instance == null || definition == null)
            {
                return false;
            }

            // Check if all required steps are completed or skipped
            var requiredSteps = definition.Steps.Where(s => s.IsRequired).ToList();
            var satisfiedSteps = instance.StepInstances
                .Where(s => s.Status == StepStatus.COMPLETED || s.Status == StepStatus.SKIPPED)
                .Select(s => s.StepId)
                .ToList();

            foreach (var requiredStep in requiredSteps)
            {
                if (!satisfiedSteps.Contains(requiredStep.StepId))
                {
                    _logger?.LogWarning($"Required step {requiredStep.StepId} is not completed");
                    return false;
                }
            }

            return true;
        }
    }
}

