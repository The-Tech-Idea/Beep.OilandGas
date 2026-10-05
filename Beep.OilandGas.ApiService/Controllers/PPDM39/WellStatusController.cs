using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.PPDM39.DataManagement.Repositories.WELL;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.PPDM39
{

    /// <summary>
    /// API for PPDM 3.9 Well Status faceted taxonomy (WSC v3, R-3 June 2020).
    ///
    /// Schema involved:
    ///   R_WELL_STATUS_TYPE      — facet type definitions (one row per STATUS_TYPE)
    ///   R_WELL_STATUS           — valid STATUS values per STATUS_TYPE
    ///   R_WELL_STATUS_QUAL      — STATUS_QUALIFIER names per STATUS_TYPE
    ///   R_WELL_STATUS_QUAL_VALUE — valid QUALIFIER_VALUE per STATUS_TYPE+STATUS+QUALIFIER
    ///   WELL_STATUS             — actual per-well facet assignments (UWI+SOURCE+STATUS_ID)
    /// </summary>
    [ApiController]
    [Route("api/wellstatus")]
    [Authorize]
    public class WellStatusController : ControllerBase
    {
        private readonly WellServices _wellServices;
        private readonly ILogger<WellStatusController> _logger;

        public WellStatusController(WellServices wellServices, ILogger<WellStatusController> logger)
        {
            _wellServices = wellServices ?? throw new ArgumentNullException(nameof(wellServices));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Reference data  (R_WELL_STATUS_TYPE / R_WELL_STATUS / R_WELL_STATUS_QUAL*)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// GET /api/wellstatus/reference
        /// Returns the full WSC v3 facet catalog (all 13 STATUS_TYPEs, each with
        /// its valid STATUS values, qualifiers, and qualifier-values) sourced from
        /// the database with fallback to the embedded static catalog.
        /// No UWI is needed — this is pure reference data.
        /// </summary>
        [HttpGet("reference")]
        public async Task<ActionResult<List<WellServices.FacetTypeDto>>> GetFacetReference()
        {
            var result = new List<WellServices.FacetTypeDto>();
            foreach (var facetType in WellServices.DEFAULT_WELL_STATUS_TYPES)
            {
                var values     = await _wellServices.GetFacetValuesAsync(facetType);
                var qualifiers = await _wellServices.GetFacetQualifiersAsync(facetType);

                var qualByStatus = new Dictionary<string, List<WellServices.FacetQualifierDto>>(StringComparer.OrdinalIgnoreCase);
                foreach (var q in qualifiers)
                {
                    var key = q.Status ?? "*";
                    if (!qualByStatus.ContainsKey(key))
                        qualByStatus[key] = new();
                    qualByStatus[key].Add(q);
                }

                WellServices.FacetTypeDef? catalog = null;
                WellServices.FACET_CATALOG.TryGetValue(facetType, out catalog);

                result.Add(new WellServices.FacetTypeDto
                {
                    StatusType = facetType,
                    LongName   = catalog?.LongName ?? facetType,
                    Scope      = catalog?.Scope,
                    Values     = values,
                    Qualifiers = qualByStatus
                });
            }
            return Ok(result);
        }

        /// <summary>
        /// GET /api/wellstatus/reference/{statusType}
        /// Returns values, qualifiers, and qualifier-values for a single STATUS_TYPE.
        /// </summary>
        [HttpGet("reference/{statusType}")]
        public async Task<ActionResult<WellServices.FacetTypeDto>> GetFacetReferenceByType(string statusType)
        {
            if (string.IsNullOrWhiteSpace(statusType))
                return BadRequest(new { error = "Status type is required." });
            var values     = await _wellServices.GetFacetValuesAsync(statusType);
            var qualifiers = await _wellServices.GetFacetQualifiersAsync(statusType);

            WellServices.FacetTypeDef? catalog = null;
            WellServices.FACET_CATALOG.TryGetValue(statusType, out catalog);

            var qualByStatus = new Dictionary<string, List<WellServices.FacetQualifierDto>>(StringComparer.OrdinalIgnoreCase);
            foreach (var q in qualifiers)
            {
                var key = q.Status ?? "*";
                if (!qualByStatus.ContainsKey(key))
                    qualByStatus[key] = new();
                qualByStatus[key].Add(q);
            }

            return Ok(new WellServices.FacetTypeDto
            {
                StatusType = statusType,
                LongName   = catalog?.LongName ?? statusType,
                Scope      = catalog?.Scope,
                Values     = values,
                Qualifiers = qualByStatus
            });
        }

        /// <summary>
        /// GET /api/wellstatus/reference/{statusType}/qualifiers/{status}
        /// Returns the STATUS_QUALIFIER options applicable for a specific STATUS value.
        /// </summary>
        [HttpGet("reference/{statusType}/qualifiers/{status}")]
        public async Task<ActionResult<List<WellServices.FacetQualifierDto>>> GetQualifiers(string statusType, string status)
        {
            if (string.IsNullOrWhiteSpace(statusType))
                return BadRequest(new { error = "Status type is required." });
            if (string.IsNullOrWhiteSpace(status))
                return BadRequest(new { error = "Status is required." });
            var result = await _wellServices.GetFacetQualifiersAsync(statusType, status);
            return Ok(result);
        }

        /// <summary>
        /// GET /api/wellstatus/reference/{statusType}/qualifier-values/{status}/{qualifier}
        /// Returns the QUALIFIER_VALUE options for a STATUS_TYPE + STATUS + STATUS_QUALIFIER.
        /// </summary>
        [HttpGet("reference/{statusType}/qualifier-values/{status}/{qualifier}")]
        public async Task<ActionResult<List<WellServices.FacetQualifierValueDto>>> GetQualifierValues(
            string statusType, string status, string qualifier)
        {
            if (string.IsNullOrWhiteSpace(statusType))
                return BadRequest(new { error = "Status type is required." });
            if (string.IsNullOrWhiteSpace(status))
                return BadRequest(new { error = "Status is required." });
            if (string.IsNullOrWhiteSpace(qualifier))
                return BadRequest(new { error = "Qualifier is required." });
            var result = await _wellServices.GetFacetQualifierValuesAsync(statusType, status, qualifier);
            return Ok(result);
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Per-well current status
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// GET /api/wellstatus/{uwi}/current
        /// Returns the current STATUS_TYPE → WELL_STATUS mapping for the given well.
        /// One entry per facet type; most recent EFFECTIVE_DATE wins within each type.
        /// </summary>
        [HttpGet("{uwi}/current")]
        public async Task<ActionResult<Dictionary<string, WELL_STATUS>>> GetCurrentStatus(string uwi)
        {
            if (string.IsNullOrWhiteSpace(uwi)) return BadRequest(new { error = "UWI is required." });
            var result = await _wellServices.GetCurrentWellStatusByUwiAsync(uwi);
            return Ok(result);
        }

        /// <summary>
        /// GET /api/wellstatus/{uwi}/page-data
        /// Returns all 13 facet types enriched with the current WELL_STATUS value
        /// for the given well.  This is the single call made by the classification page on load.
        /// </summary>
        [HttpGet("{uwi}/page-data")]
        public async Task<ActionResult<List<WellServices.FacetTypeDto>>> GetFacetPageData(string uwi)
        {
            if (string.IsNullOrWhiteSpace(uwi)) return BadRequest(new { error = "UWI is required." });
            var result = await _wellServices.GetWellFacetPageDataAsync(uwi);
            return Ok(result);
        }

        /// <summary>
        /// GET /api/wellstatus/{uwi}/history
        /// Returns full WELL_STATUS history for the given well (all facets, all dates).
        /// </summary>
        [HttpGet("{uwi}/history")]
        public async Task<ActionResult<List<WELL_STATUS>>> GetStatusHistory(string uwi)
        {
            if (string.IsNullOrWhiteSpace(uwi)) return BadRequest(new { error = "UWI is required." });
            var result = await _wellServices.GetWellStatusByUwiAsync(uwi);
            return Ok(result);
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Facet assignment (write)
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// PUT /api/wellstatus/{uwi}/facet
        /// Sets (inserts or transitions) a single STATUS_TYPE facet for the given well.
        /// The currently-active record for this STATUS_TYPE is expired first if one exists.
        /// </summary>
        [HttpPut("{uwi}/facet")]
        public async Task<ActionResult<WELL_STATUS>> SetFacet(
            string uwi, [FromBody] WellServices.SetFacetRequest request)
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(uwi)) return BadRequest(new { error = "UWI is required." });
            if (request == null)
                    return BadRequest(new { error = "Request body is required." });

            // Ensure UWI from route is used (prevents spoofing via body).
            request.UWI = uwi;

            _logger.LogInformation("Setting facet {StatusType}={Status} for UWI {UWI} by {UserId}",
                request.StatusType, request.Status, uwi, userId);

            var result = await _wellServices.SetFacetAsync(request, userId);

            _logger.LogInformation("Facet {StatusType} set for UWI {UWI}", request.StatusType, uwi);
            return Ok(result);
        }
    }
}
