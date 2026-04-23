using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Analytics;
using Beep.OilandGas.ApiService.Attributes;

namespace Beep.OilandGas.ApiService.Controllers.BusinessProcess
{
    /// <summary>
    /// Field-scoped analytics endpoints for process performance KPIs.
    /// Standards: SPE PRMS §7, API RP 97, IOGP KPI Report 2022e, NI 51-101.
    /// All routes: GET api/field/current/process/analytics/...
    /// </summary>
    [ApiController]
    [Route("api/field/current/process/analytics")]
    [Authorize]
    [RequireCurrentFieldAccess]
    public class ProcessAnalyticsController : ControllerBase
    {
        private readonly IFieldOrchestrator      _fieldOrchestrator;
        private readonly IProcessAnalyticsService _analytics;
        private readonly ILogger<ProcessAnalyticsController> _logger;

        public ProcessAnalyticsController(
            IFieldOrchestrator fieldOrchestrator,
            IProcessAnalyticsService analytics,
            ILogger<ProcessAnalyticsController> logger)
        {
            _fieldOrchestrator = fieldOrchestrator;
            _analytics         = analytics;
            _logger            = logger;
        }

        // ── Dashboard summary ─────────────────────────────────────────────────

        /// <summary>
        /// Returns the complete analytics dashboard summary for the active field.
        /// </summary>
        /// <param name="days">Lookback window in days (default 30).</param>
        /// <param name="exposureHours">Man-hours worked for PSE / TRIR rate denominators (default 1 000 000).</param>
        [HttpGet("dashboard")]
        [ProducesResponseType(typeof(AnalyticsDashboardSummary), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<AnalyticsDashboardSummary>> GetDashboardAsync(
            [FromQuery] int days = 30,
            [FromQuery] double exposureHours = 1_000_000)
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrWhiteSpace(fieldId))
                    return BadRequest(new { error = "No active field selected." });

            var range  = DateRangeFilter.LastNDays(days);
            var result = await _analytics.GetDashboardSummaryAsync(fieldId, range, exposureHours);
            return Ok(result);
        }

        // ── Work Orders ───────────────────────────────────────────────────────

        /// <summary>Returns work order KPIs for the active field.</summary>
        [HttpGet("workorders")]
        [ProducesResponseType(typeof(WorkOrderKPISet), 200)]
        public async Task<ActionResult<WorkOrderKPISet>> GetWorkOrderKPIsAsync(
            [FromQuery] int days = 30)
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrWhiteSpace(fieldId))
                    return BadRequest(new { error = "No active field selected." });

            var result = await _analytics.GetWorkOrderKPIsAsync(fieldId, DateRangeFilter.LastNDays(days));
            return Ok(result);
        }

        // ── Gate Reviews ──────────────────────────────────────────────────────

        /// <summary>Returns gate-review KPIs (cycle time, pass rate, deferred gates).</summary>
        [HttpGet("gatereviews")]
        [ProducesResponseType(typeof(GateReviewKPISet), 200)]
        public async Task<ActionResult<GateReviewKPISet>> GetGateReviewKPIsAsync(
            [FromQuery] int days = 30)
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrWhiteSpace(fieldId))
                    return BadRequest(new { error = "No active field selected." });

            var result = await _analytics.GetGateReviewKPIsAsync(fieldId, DateRangeFilter.LastNDays(days));
            return Ok(result);
        }

        // ── HSE ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns HSE KPIs including API RP 754 PSE Tier 1/2 rates and TRIR.
        /// </summary>
        [HttpGet("hse")]
        [ProducesResponseType(typeof(HSEKPISet), 200)]
        public async Task<ActionResult<HSEKPISet>> GetHSEKPIsAsync(
            [FromQuery] int days = 30,
            [FromQuery] double exposureHours = 1_000_000)
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrWhiteSpace(fieldId))
                    return BadRequest(new { error = "No active field selected." });

            var result = await _analytics.GetHSEKPIsAsync(
                fieldId, DateRangeFilter.LastNDays(days), exposureHours);
            return Ok(result);
        }

        // ── Compliance ────────────────────────────────────────────────────────

        /// <summary>Returns compliance obligation KPIs (on-time rate, overdue count).</summary>
        [HttpGet("compliance")]
        [ProducesResponseType(typeof(ComplianceKPISet), 200)]
        public async Task<ActionResult<ComplianceKPISet>> GetComplianceKPIsAsync(
            [FromQuery] int days = 30)
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrWhiteSpace(fieldId))
                    return BadRequest(new { error = "No active field selected." });

            var result = await _analytics.GetComplianceKPIsAsync(fieldId, DateRangeFilter.LastNDays(days));
            return Ok(result);
        }

        // ── Production trend ──────────────────────────────────────────────────

        /// <summary>Returns monthly production trend (BOE) for charting.</summary>
        [HttpGet("production/trend")]
        [ProducesResponseType(typeof(List<KPITrendPoint>), 200)]
        public async Task<ActionResult<List<KPITrendPoint>>> GetProductionTrendAsync(
            [FromQuery] int days = 365)
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrWhiteSpace(fieldId))
                    return BadRequest(new { error = "No active field selected." });

            var result = await _analytics.GetProductionTrendAsync(
                fieldId, DateRangeFilter.LastNDays(days));
            return Ok(result);
        }

        // ── Reserves maturation (NI 51-101 / SPE PRMS) ───────────────────────

        /// <summary>
        /// Returns reserves maturation funnel (Prospective → Contingent → Proved).
        /// Authorized for ReservesEngineer, Manager, and Admin roles only.
        /// </summary>
        [HttpGet("reserves/maturation")]
        [Authorize(Roles = "ReservesEngineer,Manager,Admin")]
        [ProducesResponseType(typeof(ReservesMaturationSummary), 200)]
        public async Task<ActionResult<ReservesMaturationSummary>> GetReservesMaturationAsync()
        {
            var fieldId = _fieldOrchestrator.CurrentFieldId ?? string.Empty;
            if (string.IsNullOrWhiteSpace(fieldId))
                    return BadRequest(new { error = "No active field selected." });

            var result = await _analytics.GetReservesMaturationAsync(fieldId);
            return Ok(result);
        }
    }
}
