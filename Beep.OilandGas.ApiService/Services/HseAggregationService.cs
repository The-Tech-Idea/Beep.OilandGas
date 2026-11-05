using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;

namespace Beep.OilandGas.ApiService.Services
{
    /// <summary>
    /// The HSE incident summary behind <c>GET api/hse/aggregation/incidents</c>. Its counts are
    /// <see cref="IHSEKPIService.CountIncidentsAsync"/>'s, the one rule for an incident's tier (the severity on its primary
    /// detail). This service had counted tiers by HSE_INCIDENT.INCIDENT_CLASS_ID, which nothing writes, so its Tier 1 count
    /// was zero while the KPI service counted the recorded tier; and it ignored the field it was asked about (2026-10-10).
    /// </summary>
    public class HseAggregationService
    {
        private readonly IHSEKPIService _incidents;

        public HseAggregationService(IHSEKPIService incidents)
        {
            _incidents = incidents ?? throw new ArgumentNullException(nameof(incidents));
        }

        public async Task<IncidentSummary> GetIncidentSummaryAsync(string? fieldId = null, DateTime? start = null, DateTime? end = null)
        {
            var counts = await _incidents.CountIncidentsAsync(fieldId, start, end);
            return new IncidentSummary { Total = counts.Total, Tier1 = counts.Tier1, Tier2 = counts.Tier2 };
        }
    }

    // No incidents-by-type figure: it was a dictionary nothing filled, answered as "no incidents of any type".
    public class IncidentSummary { public int Total { get; set; } public int Tier1 { get; set; } public int Tier2 { get; set; } }
}
