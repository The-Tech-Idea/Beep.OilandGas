using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.HSE.Models;

// Review validation shared by the SDK and later UI; the API independently enforces actual authority.
public static class InvestigatorAssignmentReviewValidator
{
    public static void Require(InvestigatorAssignmentReview? review, CompanyScope company, string actor, string incident, string field)
    {
        if (review is null) throw new InvalidOperationException("The investigator review is absent.");
        FieldSelectionReviewValidator.RequireReview(review.Context, company, actor);
        if (!LeaseSummaryValidator.Identifier(incident) || !LeaseSummaryValidator.Identifier(field) || review.IncidentId != incident || review.Context.FieldId != field ||
            review.Revision is not { Length: 64 } || review.Revision.Any(value => !char.IsAsciiHexDigit(value) || char.IsAsciiLetterLower(value)) || review.Participants is null ||
            review.Participants.Count > 256 || review.Participants.Any(row => row is null || !Observation(row.PartyObservation) || !Observation(row.RoleObservation) ||
                !LeaseSummaryValidator.Identifier(row.BusinessAssociateId) || !LeaseSummaryValidator.Identifier(row.Role)) ||
            review.Participants.Select(row => (row.PartyObservation, row.RoleObservation)).Distinct().Count() != review.Participants.Count ||
            review.CurrentInvestigator is not null && review.Participants.Count(row => row == review.CurrentInvestigator) != 1)
            throw new InvalidOperationException("The investigator review has inconsistent context, revision or complete role identities.");
    }
    public static AssignIncidentInvestigatorRequest Select(InvestigatorAssignmentReview review, RecordedIncidentParticipant participant)
    {
        if (participant is null || review.Participants.Count(row => row == participant) != 1)
            throw new InvalidOperationException("Select one exact role from the recorded investigator review.");
        return new(review.Revision, participant.PartyObservation, participant.RoleObservation, participant.BusinessAssociateId);
    }
    private static bool Observation(decimal value) => value >= 0 && decimal.Truncate(value) == value;
}
