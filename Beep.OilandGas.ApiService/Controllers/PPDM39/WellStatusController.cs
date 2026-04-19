using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.DataManagement.Repositories.WELL;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Serilog;

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
            try
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

                    WellServices.FacetTypeDef catalog = null;
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
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading facet reference data");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/wellstatus/reference/{statusType}
        /// Returns values, qualifiers, and qualifier-values for a single STATUS_TYPE.
        /// </summary>
        [HttpGet("reference/{statusType}")]
        public async Task<ActionResult<WellServices.FacetTypeDto>> GetFacetReferenceByType(string statusType)
        {
            try
            {
                var values     = await _wellServices.GetFacetValuesAsync(statusType);
                var qualifiers = await _wellServices.GetFacetQualifiersAsync(statusType);

                WellServices.FacetTypeDef catalog = null;
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
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading facet reference for {StatusType}", statusType);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/wellstatus/reference/{statusType}/qualifiers/{status}
        /// Returns the STATUS_QUALIFIER options applicable for a specific STATUS value.
        /// </summary>
        [HttpGet("reference/{statusType}/qualifiers/{status}")]
        public async Task<ActionResult<List<WellServices.FacetQualifierDto>>> GetQualifiers(string statusType, string status)
        {
            try
            {
                var result = await _wellServices.GetFacetQualifiersAsync(statusType, status);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading qualifiers for {StatusType}/{Status}", statusType, status);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/wellstatus/reference/{statusType}/qualifier-values/{status}/{qualifier}
        /// Returns the QUALIFIER_VALUE options for a STATUS_TYPE + STATUS + STATUS_QUALIFIER.
        /// </summary>
        [HttpGet("reference/{statusType}/qualifier-values/{status}/{qualifier}")]
        public async Task<ActionResult<List<WellServices.FacetQualifierValueDto>>> GetQualifierValues(
            string statusType, string status, string qualifier)
        {
            try
            {
                var result = await _wellServices.GetFacetQualifierValuesAsync(statusType, status, qualifier);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading qualifier values for {StatusType}/{Status}/{Qualifier}", statusType, status, qualifier);
                return StatusCode(500, new { error = ex.Message });
            }
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
            try
            {
                var result = await _wellServices.GetCurrentWellStatusByUwiAsync(uwi);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading current status for UWI {UWI}", uwi);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/wellstatus/{uwi}/page-data
        /// Returns all 13 facet types enriched with the current WELL_STATUS value
        /// for the given well.  This is the single call made by the classification page on load.
        /// </summary>
        [HttpGet("{uwi}/page-data")]
        public async Task<ActionResult<List<WellServices.FacetTypeDto>>> GetFacetPageData(string uwi)
        {
            try
            {
                var result = await _wellServices.GetWellFacetPageDataAsync(uwi);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading facet page data for UWI {UWI}", uwi);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/wellstatus/{uwi}/history
        /// Returns full WELL_STATUS history for the given well (all facets, all dates).
        /// </summary>
        [HttpGet("{uwi}/history")]
        public async Task<ActionResult<List<WELL_STATUS>>> GetStatusHistory(string uwi)
        {
            try
            {
                var result = await _wellServices.GetWellStatusByUwiAsync(uwi);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading status history for UWI {UWI}", uwi);
                return StatusCode(500, new { error = ex.Message });
            }
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
            if (request == null)
                return BadRequest("Request body is required");

            // Ensure UWI from route is used (prevents spoofing via body).
            request.UWI = uwi;

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "API";

            try
            {
                Log.Information("Setting facet {StatusType}={Status} for UWI {UWI} by {UserId}",
                    request.StatusType, request.Status, uwi, userId);

                var result = await _wellServices.SetFacetAsync(request, userId);

                Log.Information("Facet {StatusType} set for UWI {UWI}", request.StatusType, uwi);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error setting facet {StatusType} for UWI {UWI}", request.StatusType, uwi);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
