using System.Text.Json.Serialization;

namespace TheTechIdea.Data.OilGas;

// Complete recorded role identity. No inferred app-user mapping or extra personal/contact data.
public sealed record RecordedIncidentParticipant(
    [property: JsonRequired] decimal PartyObservation,
    [property: JsonRequired] decimal RoleObservation,
    [property: JsonRequired] string BusinessAssociateId,
    [property: JsonRequired] string Role);

// Revision binds the current account/company/database/FIELD, case authority and full active/inactive roster.
public sealed record InvestigatorAssignmentReview(
    [property: JsonRequired] FieldSelectionReview Context,
    [property: JsonRequired] string IncidentId,
    [property: JsonRequired] string Revision,
    [property: JsonRequired] IReadOnlyList<RecordedIncidentParticipant> Participants,
    [property: JsonRequired] RecordedIncidentParticipant? CurrentInvestigator);

public sealed record AssignIncidentInvestigatorRequest(
    [property: JsonRequired] string ExpectedRevision,
    [property: JsonRequired] decimal PartyObservation,
    [property: JsonRequired] decimal RoleObservation,
    [property: JsonRequired] string BusinessAssociateId);
