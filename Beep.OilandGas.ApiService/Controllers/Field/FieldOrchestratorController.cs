using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.ApiService.Attributes;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core;
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
        public async Task<ActionResult<List<FieldListItem>>> GetAllFields([FromQuery] string connectionName = "PPDM39")
        {
            var connName = connectionName ?? ConnectionName;
            var fieldMetadata = await _metadata.GetTableMetadataAsync("FIELD");
            if (fieldMetadata == null)
            {
                    return NotFound(new { error = "FIELD table metadata not found." });
            }

            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{fieldMetadata.EntityTypeName}");
            if (entityType == null)
            {
                    return NotFound(new { error = $"Entity type not found: {fieldMetadata.EntityTypeName}." });
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

        /// <summary>
        /// Set the active field
        /// </summary>
        [HttpPost("set-active")]
        public async Task<ActionResult<SetActiveFieldResponse>> SetActiveField([FromBody] SetActiveFieldRequest request)
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

        /// <summary>
        /// Get the current active field
        /// </summary>
        [HttpGet("current")]
        public async Task<ActionResult<FieldResponse>> GetCurrentField()
        {
            var field = await _fieldOrchestrator.GetCurrentFieldAsync();
            
            if (field == null)
            {
                return NotFound(new FieldResponse { FieldId = string.Empty });
            }

            string? fieldId = null;
            string? fieldName = null;

            if (field is FIELD fieldEntity)
            {
                fieldId = fieldEntity.FIELD_ID ?? string.Empty;
                fieldName = fieldEntity.FIELD_NAME ?? string.Empty;
            }

            return Ok(new FieldResponse 
            { 
                Field = field,
                FieldId = fieldId ?? string.Empty,
                FieldName = fieldName ?? string.Empty
            });
        }

        /// <summary>
        /// Get lifecycle summary for the current field
        /// </summary>
        [HttpGet("current/summary")]
        public async Task<ActionResult<FieldLifecycleSummary>> GetCurrentFieldSummary()
        {
            if (string.IsNullOrEmpty(_fieldOrchestrator.CurrentFieldId))
                return BadRequest(new { error = "No active field selected." });
            var summary = await _fieldOrchestrator.GetFieldLifecycleSummaryAsync();
            return Ok(summary);
        }

        /// <summary>
        /// Get all wells for the current field across all phases
        /// </summary>
        [HttpGet("current/wells")]
        public async Task<ActionResult<List<WELL>>> GetCurrentFieldWells()
        {
            if (string.IsNullOrEmpty(_fieldOrchestrator.CurrentFieldId))
                return BadRequest(new { error = "No active field selected." });
            var wells = await _fieldOrchestrator.GetFieldWellsAsync();
            return Ok(wells.Cast<WELL>().ToList());
        }

        /// <summary>
        /// Get statistics for the current field
        /// </summary>
        [HttpGet("current/statistics")]
        public async Task<ActionResult<FieldStatistics>> GetCurrentFieldStatistics()
        {
            if (string.IsNullOrEmpty(_fieldOrchestrator.CurrentFieldId))
                return BadRequest(new { error = "No active field selected." });
            var statistics = await _fieldOrchestrator.GetFieldStatisticsAsync();
            return Ok(statistics);
        }

        /// <summary>
        /// Get timeline for the current field
        /// </summary>
        [HttpGet("current/timeline")]
        public async Task<ActionResult<FieldTimeline>> GetCurrentFieldTimeline()
        {
            if (string.IsNullOrEmpty(_fieldOrchestrator.CurrentFieldId))
                return BadRequest(new { error = "No active field selected." });
            var timeline = await _fieldOrchestrator.GetFieldTimelineAsync();
            return Ok(timeline);
        }

        /// <summary>
        /// Get dashboard with performance metrics for the current field
        /// </summary>
        [HttpGet("current/dashboard")]
        public async Task<ActionResult<FieldDashboard>> GetCurrentFieldDashboard()
        {
            if (string.IsNullOrEmpty(_fieldOrchestrator.CurrentFieldId))
                return BadRequest(new { error = "No active field selected." });
            var dashboard = await _fieldOrchestrator.GetFieldDashboardAsync();
            return Ok(dashboard);
        }
    }
}
