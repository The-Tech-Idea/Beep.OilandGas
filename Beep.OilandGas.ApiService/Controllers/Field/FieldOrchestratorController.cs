using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.DTOs;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.ApiService.Attributes;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;

namespace Beep.OilandGas.ApiService.Controllers.Field
{
    /// <summary>
    /// API controller for FieldOrchestrator - manages complete lifecycle of a single active field
    /// </summary>
    [ApiController]
    [Route("api/field")]
    public class FieldOrchestratorController : ControllerBase
    {
        private readonly IFieldOrchestrator _fieldOrchestrator;
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<FieldOrchestratorController> _logger;
        private const string ConnectionName = "PPDM39";

        public FieldOrchestratorController(
            IFieldOrchestrator fieldOrchestrator,
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<FieldOrchestratorController> logger)
        {
            _fieldOrchestrator = fieldOrchestrator ?? throw new ArgumentNullException(nameof(fieldOrchestrator));
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
        }

        /// <summary>
        /// Get all fields for selection
        /// </summary>
        [HttpGet("fields")]
        [RequireRole(RoleDefinitions.Viewer, RoleDefinitions.Manager, RoleDefinitions.PetroleumEngineer, RoleDefinitions.ReservoirEngineer)]
        public async Task<ActionResult<List<FieldListItem>>> GetAllFields([FromQuery] string? connectionName = null)
        {
            try
            {
                var connName = connectionName ?? ConnectionName;
                var fieldMetadata = await _metadata.GetTableMetadataAsync("FIELD");
                if (fieldMetadata == null)
                {
                    return NotFound(new { error = "FIELD table metadata not found" });
                }

                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{fieldMetadata.EntityTypeName}");
                if (entityType == null)
                {
                    return NotFound(new { error = $"Entity type not found: {fieldMetadata.EntityTypeName}" });
                }

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, connName, "FIELD");

                var fields = await repo.GetAsync(new List<AppFilter>());
                
                var fieldList = fields.Select(f =>
                {
                    if (f is FIELD field)
                    {
                        return new FieldListItem
                        {
                            FieldId = field.FIELD_ID ?? string.Empty,
                            FieldName = field.FIELD_NAME ?? string.Empty,
                            Description = field.REMARK,
                            LastModifiedDate = field.ROW_CHANGED_DATE
                        };
                    }
                    return new FieldListItem();
                }).ToList();

                return Ok(fieldList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all fields");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Set the active field
        /// </summary>
        [HttpPost("set-active")]
        public async Task<ActionResult<SetActiveFieldResponse>> SetActiveField([FromBody] SetActiveFieldRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.FieldId))
                {
                    return BadRequest(new SetActiveFieldResponse 
                    { 
                        Success = false, 
                        ErrorMessage = "FieldId is required" 
                    });
                }

                var success = await _fieldOrchestrator.SetActiveFieldAsync(request.FieldId);
                
                return Ok(new SetActiveFieldResponse 
                { 
                    Success = success,
                    FieldId = success ? request.FieldId : null,
                    ErrorMessage = success ? null : "Field not found or could not be set as active"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting active field: {request?.FieldId}");
                return StatusCode(500, new SetActiveFieldResponse 
                { 
                    Success = false, 
                    ErrorMessage = ex.Message 
                });
            }
        }

        /// <summary>
        /// Get the current active field
        /// </summary>
        [HttpGet("current")]
        public async Task<ActionResult<FieldResponse>> GetCurrentField()
        {
            try
            {
                var field = await _fieldOrchestrator.GetCurrentFieldAsync();
                
                if (field == null)
                {
                    return NotFound(new FieldResponse { FieldId = null });
                }

                string? fieldId = null;
                string? fieldName = null;

                if (field is FIELD fieldEntity)
                {
                    fieldId = fieldEntity.FIELD_ID;
                    fieldName = fieldEntity.FIELD_NAME;
                }

                return Ok(new FieldResponse 
                { 
                    Field = field,
                    FieldId = fieldId,
                    FieldName = fieldName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current field");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get lifecycle summary for the current field
        /// </summary>
        [HttpGet("current/summary")]
        public async Task<ActionResult<FieldLifecycleSummary>> GetCurrentFieldSummary()
        {
            try
            {
                var summary = await _fieldOrchestrator.GetFieldLifecycleSummaryAsync();
                return Ok(summary);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current field summary");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get all wells for the current field across all phases
        /// </summary>
        [HttpGet("current/wells")]
        public async Task<ActionResult<List<WELL>>> GetCurrentFieldWells()
        {
            try
            {
                var wells = await _fieldOrchestrator.GetFieldWellsAsync();
                return Ok(wells.Cast<WELL>().ToList());
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current field wells");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get statistics for the current field
        /// </summary>
        [HttpGet("current/statistics")]
        public async Task<ActionResult<FieldStatistics>> GetCurrentFieldStatistics()
        {
            try
            {
                var statistics = await _fieldOrchestrator.GetFieldStatisticsAsync();
                return Ok(statistics);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current field statistics");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get timeline for the current field
        /// </summary>
        [HttpGet("current/timeline")]
        public async Task<ActionResult<FieldTimeline>> GetCurrentFieldTimeline()
        {
            try
            {
                var timeline = await _fieldOrchestrator.GetFieldTimelineAsync();
                return Ok(timeline);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current field timeline");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get dashboard with performance metrics for the current field
        /// </summary>
        [HttpGet("current/dashboard")]
        public async Task<ActionResult<FieldDashboard>> GetCurrentFieldDashboard()
        {
            try
            {
                var dashboard = await _fieldOrchestrator.GetFieldDashboardAsync();
                return Ok(dashboard);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current field dashboard");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
