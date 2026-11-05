using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.HSE;

namespace Beep.OilandGas.Models.Core.Interfaces;

/// <summary>
/// Canonical HSE domain service over PPDM-backed incident storage.
/// The canonical implementation reviews the current company/FIELD/account and row policy
/// independently; <see cref="IFieldHSEService"/> also fences its captured lifecycle FIELD.
/// </summary>
public interface IHSEService
{
    Task<InvestigatorAssignmentReview> ReviewInvestigatorAssignmentAsync(string incidentId);
    Task<HSEIncidentRecord>       ReportIncidentAsync(ReportIncidentRequest request, string userId);
    Task                          AssignInvestigatorAsync(string incidentId, AssignIncidentInvestigatorRequest request, string userId);
    Task<List<HSEIncidentRecord>> GetFieldIncidentsAsync(string fieldId, DateRangeFilter? range);
    Task<HseIncidentPage> GetVisibleFieldIncidentPageAsync(string fieldId, int pageNumber, CancellationToken cancellationToken = default);
    Task<HSEIncidentRecord?>      GetByIdAsync(string incidentId);
    Task                          UpdateTierAsync(string incidentId, int tier, string userId);
    Task<bool>                    TransitionAsync(string incidentId, string trigger, string? reason, string userId);
    Task<List<string>>            GetAvailableTriggersAsync(string incidentId);
}
