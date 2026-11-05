using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Beep.OilandGas.ApiService.Attributes;

namespace Beep.OilandGas.ApiService.Controllers.Field
{
    // Lifecycle consumers still require their own company-bound FIELD restoration/domain audit.
    public sealed partial class FieldOrchestratorController
    {
        private IFieldOrchestrator _fieldOrchestrator => HttpContext.RequestServices.GetRequiredService<IFieldOrchestrator>();
        [HttpGet("current/summary")]
        [RequireCurrentFieldAccess]
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
        [RequireCurrentFieldAccess]
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
        [RequireCurrentFieldAccess]
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
        [RequireCurrentFieldAccess]
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
        [RequireCurrentFieldAccess]
        public async Task<ActionResult<FieldDashboard>> GetCurrentFieldDashboard()
        {
            if (string.IsNullOrEmpty(_fieldOrchestrator.CurrentFieldId))
                return BadRequest(new { error = "No active field selected." });
            var dashboard = await _fieldOrchestrator.GetFieldDashboardAsync();
            return Ok(dashboard);
        }
    }
}
