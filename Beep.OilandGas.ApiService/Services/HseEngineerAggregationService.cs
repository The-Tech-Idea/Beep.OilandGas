using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;

namespace Beep.OilandGas.ApiService.Services
{
    /// <summary>
    /// The HSE dashboard's figures: incidents reported this year, and incidents open now. Both are
    /// <see cref="IHSEKPIService.CountIncidentsAsync"/>'s counts. "Open" is the incident's state — the latest status its
    /// investigation recorded, neither closed nor cancelled. It had been the incidents with no HSE_INCIDENT.INCIDENT_CLASS_ID,
    /// a column nothing writes, so it counted every incident this year whatever its state (2026-10-10).
    /// </summary>
    public class HseEngineerAggregationService
    {
        private readonly IHSEKPIService _incidents;

        public HseEngineerAggregationService(IHSEKPIService incidents)
        {
            _incidents = incidents ?? throw new ArgumentNullException(nameof(incidents));
        }

        /// <summary>Gets the HSE dashboard's figures, across every field.</summary>
        public async Task<HseKpi> GetKpiAsync()
        {
            var now = DateTime.UtcNow;
            var thisYear = await _incidents.CountIncidentsAsync(null, new DateTime(now.Year, 1, 1), null);
            // Open is counted over every incident, not only this year's: an incident reported last year and still under
            // investigation is open.
            var allTime = await _incidents.CountIncidentsAsync(null, null, null);
            return new HseKpi { TotalIncidents = thisYear.Total, OpenIncidents = allTime.Open };
        }
    }

    /// <summary>HSE dashboard KPI data.</summary>
    /// <remarks>
    /// No overdue-action or active-permit count: nothing computed either, and both were answered as 0.
    /// </remarks>
    public class HseKpi
    {
        public int TotalIncidents { get; set; }
        public int OpenIncidents { get; set; }
    }
}
