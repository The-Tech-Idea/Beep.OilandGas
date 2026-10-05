using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.ProductionAccounting.Services;
using Beep.OilandGas.ProductionAccounting.Constants;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Accounting.Cost
{
    /// <summary>
    /// API controller for AFE (Authorization for Expenditure) operations.
    /// </summary>
    [ApiController]
    [Route("api/accounting/cost/afe")]
    public class AFEController : ControllerBase
    {
        private readonly ProductionAccountingService _service;
        private readonly IAfeService _afeService;
        private readonly ILogger<AFEController> _logger;

        public AFEController(
            ProductionAccountingService service,
            IAfeService afeService,
            ILogger<AFEController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _afeService = afeService ?? throw new ArgumentNullException(nameof(afeService));
            _logger = logger;
        }

        /// <summary>
        /// List all AFEs.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<object>>> GetAFEsAsync([FromQuery] string connectionName = "PPDM39")
        {
            var connName = connectionName ?? _service.DefaultConnectionName;
            var repository = _service.GetRepository(typeof(AFE), connName, "AFE");
            var entities = await repository.GetAsync(new List<TheTechIdea.Beep.Report.AppFilter>());
            var afes = (entities ?? new List<object>())
                .OfType<AFE>()
                .Select(afe => (object)new
                {
                    AfeId         = afe.AFE_ID,
                    AfeNumber     = afe.AFE_NUMBER,
                    AfeName       = afe.AFE_NAME,
                    PropertyId    = afe.PROPERTY_ID,
                    EstimatedCost = afe.ESTIMATED_COST,
                    Status        = afe.ACTIVE_IND == "Y" ? "ACTIVE" : "CLOSED",
                    EffectiveDate = afe.ROW_EFFECTIVE_DATE
                })
                .ToList();
            return Ok(afes);
        }

        /// <summary>
        /// Create an AFE.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<object>> CreateAFE(
            [FromBody] CreateAFERequest request,
            [FromQuery] string connectionName = "PPDM39")
        {
            var userId = User.ActingUserId();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var connName = connectionName ?? _service.DefaultConnectionName;

            var afe = new AFE
            {
                AFE_NUMBER = request.AfeNumber,
                AFE_NAME = request.AfeName ?? request.AfeNumber,
                PROPERTY_ID = request.PropertyId,
                ESTIMATED_COST = request.BudgetAmount,
                ROW_EFFECTIVE_DATE = request.EffectiveDate ?? DateTime.UtcNow
            };

            var created = await _afeService.CreateAfeAsync(afe, userId, connName);

            return Ok(new { AfeId = created.AFE_ID, AfeNumber = created.AFE_NUMBER });
        }

        /// <summary>
        /// Get AFE by ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetAFE(
            string id,
            [FromQuery] string connectionName = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { error = "AFE ID is required." });
            var connName = connectionName ?? _service.DefaultConnectionName;
            var repository = _service.GetRepository(typeof(AFE), connName, "AFE");

            var afe = await repository.GetByIdAsync(id) as AFE;
            if (afe == null)
                return NotFound(new { error = $"AFE with ID {id} not found." });

            return Ok(new
            {
                AfeId = afe.AFE_ID,
                AfeNumber = afe.AFE_NUMBER,
                AfeName = afe.AFE_NAME,
                PropertyId = afe.PROPERTY_ID,
                EstimatedCost = afe.ESTIMATED_COST,
                EffectiveDate = afe.ROW_EFFECTIVE_DATE
            });
        }

        /// <summary>Approve an AFE using <see cref="IAfeService"/>.</summary>
        [HttpPatch("{id}/approve")]
        public async Task<ActionResult> ApproveAFE(string id, [FromQuery] string connectionName = "PPDM39")
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { error = "AFE ID is required." });
            var connName = connectionName ?? _service.DefaultConnectionName;
            await _afeService.ApproveAfeAsync(id, DateTime.UtcNow, userId, connName);
            return NoContent();
        }

        /// <summary>Reject an AFE by reverting status to draft for revision.</summary>
        [HttpPatch("{id}/reject")]
        public async Task<ActionResult> RejectAFE(string id, [FromQuery] string connectionName = "PPDM39")
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { error = "AFE ID is required." });
            var connName = connectionName ?? _service.DefaultConnectionName;
            var repository = _service.GetRepository(typeof(AFE), connName, "AFE");
            var afe = await repository.GetByIdAsync(id) as AFE;
            if (afe == null)
                return NotFound(new { error = $"AFE {id} not found." });

            afe.STATUS = AfeStatusCodes.Draft;
            afe.ROW_CHANGED_BY = userId;
            afe.ROW_CHANGED_DATE = DateTime.UtcNow;
            await repository.UpdateAsync(afe, userId);
            return NoContent();
        }

        [HttpPost("{id}/line-items")]
        public async Task<ActionResult<AFE_LINE_ITEM>> AddLineItemAsync(
            string id,
            [FromBody] AFE_LINE_ITEM lineItem,
            [FromQuery] string connectionName = "PPDM39")
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { error = "AFE ID is required." });
            lineItem.AFE_ID = id;
            var result = await _afeService.AddLineItemAsync(lineItem, userId, connectionName ?? _service.DefaultConnectionName);
            return Ok(result);
        }

        [HttpPost("{id}/costs")]
        public async Task<ActionResult<ACCOUNTING_COST>> RecordCostAsync(
            string id,
            [FromBody] ACCOUNTING_COST cost,
            [FromQuery] string connectionName = "PPDM39")
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { error = "AFE ID is required." });
            var result = await _afeService.RecordCostAsync(id, cost, userId, connectionName ?? _service.DefaultConnectionName);
            return Ok(result);
        }

        [HttpGet("{id}/line-items")]
        public async Task<ActionResult<List<AFE_LINE_ITEM>>> GetLineItemsAsync(string id, [FromQuery] string connectionName = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { error = "AFE ID is required." });
            var lineItems = await _afeService.GetLineItemsAsync(id, connectionName ?? _service.DefaultConnectionName);
            return Ok(lineItems);
        }

        [HttpPost("{id}/variance-report")]
        public async Task<ActionResult<COST_VARIANCE_REPORT>> GenerateVarianceReportAsync(
            string id,
            [FromQuery] decimal varianceThreshold = 10m,
            [FromQuery] string connectionName = "PPDM39")
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { error = "AFE ID is required." });
            var report = await _afeService.GenerateBudgetVarianceReportAsync(
                id,
                varianceThreshold,
                userId,
                connectionName ?? _service.DefaultConnectionName);
            return Ok(report);
        }

        [HttpGet("variance-reports")]
        public async Task<ActionResult<List<COST_VARIANCE_REPORT>>> GetVarianceReportsAsync(
            [FromQuery] string? afeId = null,
            [FromQuery] string? costCenterId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string connectionName = "PPDM39")
        {
            var reports = await _afeService.GetVarianceReportsAsync(
                afeId,
                costCenterId,
                startDate,
                endDate,
                connectionName ?? _service.DefaultConnectionName);
            return Ok(reports);
        }
    }
}

