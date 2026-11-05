using Beep.OilandGas.Models.Data.Process;
namespace TheTechIdea.Data.OilGas;

/// <summary>Recorded source identity and content reviewed before starting a process, not financial or maintenance approval.</summary>
public sealed record ProcessStartSourceReview(CompanyScope Company,string ActorUserId,string FieldId,
    string ProcessId,string DefinitionRevision,string EntityId,string EntityType,string? RecordedReference,
    string? Description,string? RecordedStatus,string? RecordedBy,string Revision,DateTimeOffset RetrievedAtUtc)
{
    public void Require(CompanyScope company,string actor,ProcessInstanceRequest request)
    {
        if(company is null || string.IsNullOrWhiteSpace(company.TenantId) || string.IsNullOrWhiteSpace(company.BusinessAssociateId) ||
            string.IsNullOrWhiteSpace(actor) || Company!=company || ActorUserId!=actor ||
            string.IsNullOrWhiteSpace(request.ExpectedFieldId) || FieldId!=request.ExpectedFieldId ||
            string.IsNullOrWhiteSpace(request.ProcessId) || ProcessId!=request.ProcessId ||
            !ProcessDefinitionRevision.IsValid(request.ExpectedDefinitionRevision) || DefinitionRevision!=request.ExpectedDefinitionRevision ||
            string.IsNullOrWhiteSpace(request.EntityId) || EntityId!=request.EntityId || EntityType!=request.EntityType ||
            EntityType is not ("JOURNAL_ENTRY" or "WORK_ORDER") || !ProcessDefinitionRevision.IsValid(Revision) ||
            RetrievedAtUtc==default || RetrievedAtUtc.Offset!=TimeSpan.Zero)
            throw new InvalidOperationException("The recorded start source does not match the original company, actor, FIELD and definition.");
    }
}
