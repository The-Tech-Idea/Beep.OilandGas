using Beep.OilandGas.Models.Data.HSE;

namespace Beep.OilandGas.Models.Core.Interfaces;

/// <summary>
/// Canonical HSE domain service over PPDM-backed incident storage.
/// This interface is intentionally field-agnostic; field scoping is enforced by
/// <see cref="IFieldHSEService"/> through lifecycle orchestration.
/// </summary>
public interface IHSEService
{
    Task<HSEIncidentRecord>       ReportIncidentAsync(ReportIncidentRequest request, string userId);
    Task                          AssignInvestigatorAsync(string incidentId, string baId, string userId);
    Task<List<HSEIncidentRecord>> GetFieldIncidentsAsync(string fieldId, DateRangeFilter? range);
    Task<HSEIncidentRecord?>      GetByIdAsync(string incidentId);
    Task                          UpdateTierAsync(string incidentId, int tier, string userId);
    Task<bool>                    TransitionAsync(string incidentId, string trigger, string? reason, string userId);
    Task<List<string>>            GetAvailableTriggersAsync(string incidentId);
}
