using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.ApiService.Models;
using Beep.OilandGas.PPDM39.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Report;
using ValidationResult = Beep.OilandGas.ApiService.Models.ValidationResult;
using ValidationError = Beep.OilandGas.ApiService.Models.ValidationError;

namespace Beep.OilandGas.ApiService.Controllers.PPDM39
{
    /// <summary>
    /// API controller for PPDM39 data validation operations
    /// </summary>
    [ApiController]
    [Route("api/ppdm39/validation")]
    public class PPDM39ValidationController : ControllerBase
    {
        private readonly IPPDMDataValidationService _validationService;
        private readonly ILogger<PPDM39ValidationController> _logger;

        public PPDM39ValidationController(
            IPPDMDataValidationService validationService,
            ILogger<PPDM39ValidationController> logger)
        {
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Validate a single entity
        /// </summary>
        [HttpPost("{tableName}/validate")]
        public async Task<ActionResult<Beep.OilandGas.ApiService.Models.ValidationResult>> ValidateEntity(
            string tableName,
            [FromBody] ValidationRequest request)
        {
            try
            {
                if (request == null || request.EntityData == null)
                {
                    return BadRequest(new ValidationResult { IsValid = false, Errors = new List<ValidationError> { new ValidationError { ErrorMessage = "Entity data is required" } } });
                }

                _logger.LogInformation("Validating entity in table {TableName}", tableName);

                // Convert dictionary to entity object (simplified - in real scenario would use proper deserialization)
                // For now, we'll need to work with the service's actual entity type
                // This is a placeholder - actual implementation would need entity type resolution
                var validationResult = await _validationService.ValidateAsync(request.EntityData, tableName);

                return Ok(new ValidationResult
                {
                    IsValid = validationResult.IsValid,
                    Errors = validationResult.Errors?.Select(e => new ValidationError
                    {
                        FieldName = e.FieldName ?? string.Empty,
                        ErrorMessage = e.ErrorMessage ?? string.Empty,
                        ErrorCode = e.RuleName // Map RuleName to ErrorCode
                    }).ToList() ?? new List<ValidationError>()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating entity in table {TableName}", tableName);
                return StatusCode(500, new ValidationResult
                {
                    IsValid = false,
                    Errors = new List<ValidationError> { new ValidationError { ErrorMessage = ex.Message } }
                });
            }
        }

        /// <summary>
        /// Validate multiple entities in batch
        /// </summary>
        [HttpPost("{tableName}/validate-batch")]
        public async Task<ActionResult<List<Beep.OilandGas.ApiService.Models.ValidationResult>>> ValidateBatch(
            string tableName,
            [FromBody] BatchValidationRequest request)
        {
            try
            {
                if (request == null || request.Entities == null || !request.Entities.Any())
                {
                    return BadRequest(new List<ValidationResult>());
                }

                _logger.LogInformation("Validating {Count} entities in table {TableName}", request.Entities.Count, tableName);

                var results = new List<ValidationResult>();
                foreach (var entityData in request.Entities)
                {
                    try
                    {
                        var validationResult = await _validationService.ValidateAsync(entityData, tableName);
                        results.Add(new ValidationResult
                        {
                            IsValid = validationResult.IsValid,
                            Errors = validationResult.Errors?.Select(e => new ValidationError
                            {
                                FieldName = e.FieldName ?? string.Empty,
                                ErrorMessage = e.ErrorMessage ?? string.Empty,
                                ErrorCode = e.RuleName // Map RuleName to ErrorCode
                            }).ToList() ?? new List<ValidationError>()
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error validating entity in batch");
                        results.Add(new ValidationResult
                        {
                            IsValid = false,
                            Errors = new List<ValidationError> { new ValidationError { ErrorMessage = ex.Message } }
                        });
                    }
                }

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating batch entities in table {TableName}", tableName);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get validation rules for a table
        /// </summary>
        [HttpGet("{tableName}/rules")]
        public async Task<ActionResult<List<ValidationRule>>> GetValidationRules(string tableName)
        {
            try
            {
                _logger.LogInformation("Getting validation rules for table {TableName}", tableName);
                var rules = await _validationService.GetValidationRulesAsync(tableName);
                return Ok(rules ?? new List<ValidationRule>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting validation rules for table {TableName}", tableName);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
