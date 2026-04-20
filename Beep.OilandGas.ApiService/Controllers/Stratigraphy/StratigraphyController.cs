using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Core.Interfaces;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Stratigraphy
{
    /// <summary>
    /// API controller for stratigraphic data (STRAT_COLUMN, STRAT_UNIT, STRAT_HIERARCHY, STRAT_WELL_SECTION).
    ///
    /// GET /api/stratigraphy/columns              — list STRAT_COLUMN
    /// GET /api/stratigraphy/units                — list STRAT_UNIT
    /// GET /api/stratigraphy/hierarchy            — list STRAT_HIERARCHY
    /// GET /api/stratigraphy/well-sections        — list STRAT_WELL_SECTION
    /// GET /api/stratigraphy/well-sections/well/{uwi} — filter by well UWI
    /// </summary>
    [ApiController]
    [Route("api/stratigraphy")]
    public class StratigraphyController : ControllerBase
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<StratigraphyController> _logger;
        private const string ConnectionName = "PPDM39";

        public StratigraphyController(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<StratigraphyController> logger)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ── STRAT_COLUMN ────────────────────────────────────────────────────────

        /// <summary>GET /api/stratigraphy/columns</summary>
        [HttpGet("columns")]
        public async Task<ActionResult<List<STRAT_COLUMN>>> GetColumnsAsync([FromQuery] string? filters = null)
        {
            try
            {
                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(STRAT_COLUMN), ConnectionName, "STRAT_COLUMN");

                var filterList = ParseQueryFilters(filters);
                var results = await repo.GetAsync(filterList);
                return Ok(results.OfType<STRAT_COLUMN>().ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching STRAT_COLUMN");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        // ── STRAT_UNIT ──────────────────────────────────────────────────────────

        /// <summary>GET /api/stratigraphy/units</summary>
        [HttpGet("units")]
        public async Task<ActionResult<List<STRAT_UNIT>>> GetUnitsAsync([FromQuery] string? filters = null)
        {
            try
            {
                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(STRAT_UNIT), ConnectionName, "STRAT_UNIT");

                var filterList = ParseQueryFilters(filters);
                var results = await repo.GetAsync(filterList);
                return Ok(results.OfType<STRAT_UNIT>().ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching STRAT_UNIT");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        // ── STRAT_HIERARCHY ─────────────────────────────────────────────────────

        /// <summary>GET /api/stratigraphy/hierarchy</summary>
        [HttpGet("hierarchy")]
        public async Task<ActionResult<List<STRAT_HIERARCHY>>> GetHierarchyAsync([FromQuery] string? filters = null)
        {
            try
            {
                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(STRAT_HIERARCHY), ConnectionName, "STRAT_HIERARCHY");

                var filterList = ParseQueryFilters(filters);
                var results = await repo.GetAsync(filterList);
                return Ok(results.OfType<STRAT_HIERARCHY>().ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching STRAT_HIERARCHY");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        // ── STRAT_WELL_SECTION ──────────────────────────────────────────────────

        /// <summary>GET /api/stratigraphy/well-sections</summary>
        [HttpGet("well-sections")]
        public async Task<ActionResult<List<STRAT_WELL_SECTION>>> GetWellSectionsAsync([FromQuery] string? filters = null)
        {
            try
            {
                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(STRAT_WELL_SECTION), ConnectionName, "STRAT_WELL_SECTION");

                var filterList = ParseQueryFilters(filters);
                var results = await repo.GetAsync(filterList);
                return Ok(results.OfType<STRAT_WELL_SECTION>().ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching STRAT_WELL_SECTION");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>GET /api/stratigraphy/well-sections/well/{uwi}</summary>
        [HttpGet("well-sections/well/{uwi}")]
        public async Task<ActionResult<List<STRAT_WELL_SECTION>>> GetWellSectionsByUwiAsync(string uwi)
        {
            if (string.IsNullOrWhiteSpace(uwi)) return BadRequest(new { error = "UWI is required." });
            try
            {
                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(STRAT_WELL_SECTION), ConnectionName, "STRAT_WELL_SECTION");

                var filterList = new List<AppFilter>
                {
                    new AppFilter { FieldName = "UWI", Operator = "=", FilterValue = uwi }
                };
                var results = await repo.GetAsync(filterList);
                return Ok(results.OfType<STRAT_WELL_SECTION>().ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching STRAT_WELL_SECTION for UWI {Uwi}", uwi);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        // ── helpers ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Parses an optional comma-separated "COLUMN=VALUE" query string into AppFilter list.
        /// Example: ?filters=ACTIVE_IND=Y
        /// </summary>
        private static List<AppFilter> ParseQueryFilters(string? raw)
        {
            var list = new List<AppFilter>();
            if (string.IsNullOrWhiteSpace(raw))
                return list;

            foreach (var part in raw.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var eq = part.IndexOf('=');
                if (eq <= 0) continue;
                list.Add(new AppFilter
                {
                    FieldName = part[..eq].Trim(),
                    Operator = "=",
                    FilterValue = part[(eq + 1)..].Trim()
                });
            }
            return list;
        }
    }
}
