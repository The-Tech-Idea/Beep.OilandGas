using System.Text.Json;
namespace TheTechIdea.Data.OilGas;

/// <summary>Frozen reviewed intent, recorded in the existing native process payload.</summary>
public sealed record CompanyAccountingProcessStart(CompanyScope Company, string ActorUserId, string IntentId,
    string ProcessId, string EntityType, string EntityId, string SourceRevision, string DefinitionRevision,
    string BindingRevision, string SourcePreparerUserId)
{
    public const string PayloadKey = "CompanyAccountingStart";
    public void Require()
    {
        APInvoiceOwnership.RequireScope(Company);
        if (!Guid.TryParseExact(IntentId, "N", out var intent) || intent == Guid.Empty || IntentId != intent.ToString("N") ||
            EntityType is not ("AP_INVOICE" or "PURCHASE_ORDER" or "SALES_CONTRACT" or "RUN_TICKET" or "INVOICE" or "AR_INVOICE" or "JOURNAL_ENTRY" or "AR_PAYMENT") ||
            new[] { ActorUserId, ProcessId, EntityId, SourcePreparerUserId }.Any(value => string.IsNullOrWhiteSpace(value) || value.Length > 128 || value.Any(char.IsControl)) ||
            !ProcessDefinitionRevision.IsValid(SourceRevision) || !ProcessDefinitionRevision.IsValid(DefinitionRevision) || !ProcessDefinitionRevision.IsValid(BindingRevision))
            throw new InvalidOperationException("A complete original company/source/definition/actor start review is required.");
    }
    public string InstanceId()
    {
        Require();
        return "company-process-" + ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new { Company, IntentId }));
    }
}

public sealed record CompanyAccountingProcessStartQuery(string ProcessId,string EntityType,string EntityId,string? ExpectedSourceRevision=null)
{
    public void Require()
    {
        if(EntityType is not ("AP_INVOICE" or "PURCHASE_ORDER" or "SALES_CONTRACT" or "RUN_TICKET" or "INVOICE" or "AR_INVOICE" or "JOURNAL_ENTRY" or "AR_PAYMENT") || new[]{ProcessId,EntityId}.Any(value=>string.IsNullOrWhiteSpace(value) || value.Length>128 || value.Any(char.IsControl)) ||
            ExpectedSourceRevision is not null && !ProcessDefinitionRevision.IsValid(ExpectedSourceRevision))
            throw new ArgumentException("A bounded accounting workflow and source identity are required.");
    }
}

public sealed record CompanyAccountingProcessStartReview(CompanyScope Company,string ActorUserId,string ProcessId,string EntityType,string EntityId,
    string SourceRevision,string DefinitionRevision,string BindingRevision,string SourcePreparerUserId,string ProcessName,string ProcessVersion,string InitialRole)
{
    public void Require()
    {
        APInvoiceOwnership.RequireScope(Company);new CompanyAccountingProcessStartQuery(ProcessId,EntityType,EntityId).Require();
        if(new[]{ActorUserId,SourcePreparerUserId}.Any(value=>string.IsNullOrWhiteSpace(value) || value.Length>128 || value.Any(char.IsControl)) ||
            !ProcessDefinitionRevision.IsValid(SourceRevision) || !ProcessDefinitionRevision.IsValid(DefinitionRevision) || !ProcessDefinitionRevision.IsValid(BindingRevision) ||
            string.IsNullOrWhiteSpace(ProcessName) || ProcessName.Length>512 || ProcessName.Any(char.IsControl) ||
            new[]{ProcessVersion,InitialRole}.Any(value=>string.IsNullOrWhiteSpace(value) || value.Length>128 || value.Any(char.IsControl)))
            throw new InvalidOperationException("The recorded company/source/preparer/workflow review is incomplete.");
    }
    public CompanyAccountingProcessStart CreateIntent(string intentId)
    {
        Require();
        var intent=new CompanyAccountingProcessStart(Company,ActorUserId,intentId,ProcessId,EntityType,EntityId,SourceRevision,DefinitionRevision,BindingRevision,SourcePreparerUserId);
        intent.Require();
        return intent;
    }
    public void RequireIntent(CompanyAccountingProcessStart intent)
    {
        ArgumentNullException.ThrowIfNull(intent);intent.Require();
        if(CreateIntent(intent.IntentId)!=intent)throw new InvalidOperationException("The submission does not match the current original company/source/preparer/workflow review.");
    }
}

public sealed record CompanyAccountingProcessStartReceipt(CompanyScope Company, string ActorUserId, string IntentId,
    string InstanceId, string EntityType, string EntityId, string StepInstanceId, IReadOnlyList<string> TaskIds, string RecordedRevision)
{
    public void Require(CompanyAccountingProcessStart intent)
    {
        intent.Require();
        if(Company!=intent.Company || ActorUserId!=intent.ActorUserId || IntentId!=intent.IntentId || InstanceId!=intent.InstanceId() || EntityType!=intent.EntityType || EntityId!=intent.EntityId ||
            string.IsNullOrWhiteSpace(StepInstanceId) || StepInstanceId.Length>128 || StepInstanceId.Any(char.IsControl) ||
            TaskIds is null || TaskIds.Count is <1 or >128 || TaskIds.Any(value=>string.IsNullOrWhiteSpace(value) || value.Length>128 || value.Any(char.IsControl)) ||
            TaskIds.Distinct(StringComparer.Ordinal).Count()!=TaskIds.Count || !ProcessDefinitionRevision.IsValid(RecordedRevision))
            throw new InvalidOperationException("The recorded company handoff receipt does not match its retained intent.");
    }
}

/// <summary>The API supplies authoritative original company/binding/write checks; native source review remains API-owned.</summary>
public interface ICompanyAccountingProcessCommands
{
    Task<CompanyAccountingStepCommand?> GetRecordedCompanyAccountingStepCommandAsync(string instanceId);
    Task<CompanyAccountingStepReceipt> CompleteCompanyAccountingStepAsync(CompanyAccountingStepCommand command,Func<Task> requireCurrentReview);
    Task<CompanyAccountingProcessStartReceipt> StartCompanyAccountingAsync(CompanyAccountingProcessStart intent, Func<Task> requireCurrentReview);
}
