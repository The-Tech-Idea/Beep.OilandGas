using System.Text.Json.Serialization;

namespace TheTechIdea.Data.OilGas;

// A wire submission binds the existing creation intent to the FIELD review the caller used.
public sealed record CreateCAPlanSubmission(
    [property: JsonRequired] FieldSelectionReview Context,
    [property: JsonRequired] CreateCAPlanRequest Intent);

// Current recorded linkage under a FIELD review; this is not a lifetime creation-intent receipt.
public sealed record CAPlanResolution(
    [property: JsonRequired] FieldSelectionReview Context,
    [property: JsonRequired] string IncidentId,
    [property: JsonRequired] string PlanId);

// Recorded creation choices; revision describes evidence and is not a durable intent receipt.
public sealed record CAPlanCreationReview(
    [property: JsonRequired] FieldSelectionReview Context,
    [property: JsonRequired] string IncidentId,
    [property: JsonRequired] string Revision,
    [property: JsonRequired] IReadOnlyList<RecordedIncidentParticipant> Participants,
    [property: JsonRequired] IReadOnlyList<string> ResponseTypes);
