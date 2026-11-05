using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.Models.Data.Company;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.FieldOrchestrator;
using Beep.OilandGas.Models.Data.EnhancedRecovery;

namespace Beep.OilandGas.Client.App.Services.Field
{
    /// <summary>
    /// Service interface for Field/LifeCycle operations
    /// </summary>
    public interface IFieldService
    {
        Task<InvestigatorAssignmentReview> ReviewInvestigatorAssignmentAsync(string incidentId, string expectedFieldId, CancellationToken cancellationToken = default);
        Task AssignInvestigatorAsync(InvestigatorAssignmentReview review, RecordedIncidentParticipant participant, CancellationToken cancellationToken = default);
        Task<CAPlanResolution> CreateCorrectiveActionPlanAsync(string incidentId, CreateCAPlanSubmission submission, CancellationToken cancellationToken = default);
        Task<CAPlanCreationReview> ReviewCorrectiveActionPlanCreationAsync(string incidentId, string expectedField, CancellationToken cancellationToken = default);
        Task<CAPlanResolution> GetCorrectiveActionPlanAsync(string incidentId, FieldSelectionReview context, CancellationToken cancellationToken = default);
        Task SetCurrentFieldAsync(string fieldId, CancellationToken cancellationToken = default);
        Task<string?> GetCurrentFieldAsync(CancellationToken cancellationToken = default);
        Task<FieldOrchestratorResponse> GetFieldDetailsAsync(string fieldId, CancellationToken cancellationToken = default);
        Task<List<WELL>> GetFieldWellsAsync(string fieldId, CancellationToken cancellationToken = default);
        Task<FieldProductionSummary> GetFieldProductionSummaryAsync(string fieldId, CancellationToken cancellationToken = default);
    }
}

