using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.Models.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Report;
using ValidationResult = Beep.OilandGas.Models.Data.ValidationResult;

namespace Beep.OilandGas.ApiService.Controllers.PPDM39
{
    /// <summary>
    /// API controller for PPDM39 data validation operations
    /// </summary>
    [Authorize]
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
        public async Task<ActionResult<ValidationResult>> ValidateEntity(
            string tableName,
            [FromBody] ValidationRequest request)
        {
                if (string.IsNullOrWhiteSpace(tableName))
                    return BadRequest(new { error = "Table name is required." });
                if (request == null || request.EntityData == null)
                {
                    return BadRequest(new { error = "Entity data is required." });
                }

                _logger.LogInformation("Validating entity in table {TableName}", tableName);

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

        /// <summary>
        /// Validate multiple entities in batch
        /// </summary>
        [HttpPost("{tableName}/validate-batch")]
        public async Task<ActionResult<List<ValidationResult>>> ValidateBatch(
            string tableName,
            [FromBody] BatchValidationRequest request)
        {
                if (string.IsNullOrWhiteSpace(tableName))
                    return BadRequest(new { error = "Table name is required." });
                if (request == null || request.Entities == null || !request.Entities.Any())
                {
                return BadRequest(new List<ValidationResult>());
                }

                _logger.LogInformation("Validating {Count} entities in table {TableName}", request.Entities.Count, tableName);

                // An entity the validator fails on is not an invalid entity: the batch is answered as the failure it is (the
                // API's handler reports it, with its reference) rather than one of its results reading "not valid"
                // (OILGAS-CATCH-01).
                var results = new List<ValidationResult>();
                foreach (var entityData in request.Entities)
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

                return Ok(results);
        }

        /// <summary>
        /// Get validation rules for a table
        /// </summary>
        [HttpGet("{tableName}/rules")]
        public async Task<ActionResult<List<ValidationRule>>> GetValidationRules(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                return BadRequest(new { error = "Table name is required." });
            _logger.LogInformation("Getting validation rules for table {TableName}", tableName);
            var rules = await _validationService.GetValidationRulesAsync(tableName);
            return Ok(rules ?? new List<ValidationRule>());
        }
    }
}
