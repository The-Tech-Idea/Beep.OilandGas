namespace TheTechIdea.Data.OilGas;

// The API resolves the authenticated actor and current reviewed FIELD; clients select only the recorded participant.
public sealed record AssignCorrectiveActionResponsibleRequest(string BusinessAssociateId);
