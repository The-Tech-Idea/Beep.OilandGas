using System.Text.Json;
using System.Text.Json.Serialization;
namespace TheTechIdea.Data.OilGas;
public sealed record DrillingProgramReviewStart([property:JsonRequired]DrillingProgramReviewSource Source,[property:JsonRequired]DrillingProgramReviewOption Workflow,[property:JsonRequired]Guid IntentId)
{
    public const string PayloadKey="DrillingProgramReviewStart",InstancePrefix="drilling-program-review-";
    public const int MaximumBodyLength=8*1024*1024;
    public DevelopmentAssignedReviewScope Scope()=>Source.Basis.Scope();
    public void Require()
    {
        if(Source is null || Workflow is null || IntentId==Guid.Empty)throw new InvalidOperationException("An exact saved program, configured independent workflow and retained intent are required.");Source.Require();Workflow.Require(Source);
        if(!Workflow.Ready || JsonSerializer.SerializeToUtf8Bytes(this).Length>MaximumBodyLength)throw new InvalidOperationException("The complete bounded program workflow must have current independent recipients at every stage.");
    }
    public string InstanceId(){Require();return InstancePrefix+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Scope().Company,IntentId}));}
    public string Revision(){Require();return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(this));}
    public void RequireSource(DrillingProgramReviewSource current,bool exact=true,bool originalActor=true)
    {
        Require();current.Require();if((originalActor?current.Basis.Scope():current.Basis.Scope() with{ActorUserId=Scope().ActorUserId})!=Scope() || current.Basis.ExecutionDatabase!=Source.Basis.ExecutionDatabase || current.Program.PROGRAM_ID!=Source.Program.PROGRAM_ID || current.Program.PROGRAM_DRAFT_REVISION!=Source.Program.PROGRAM_DRAFT_REVISION ||
            current.Basis.Funding.Source.Afe()!=Source.Basis.Funding.Source.Afe() || current.Basis.Funding.Progress!.NativeRevision!=Source.Basis.Funding.Progress!.NativeRevision || exact && current.Revision!=Source.Revision)
            throw new InvalidOperationException("The original program version, company/account/well/FIELD, approved funding or bindings changed.");
    }
    public static bool HasPayload(string? json)
    {if(string.IsNullOrWhiteSpace(json))return false;using var doc=JsonDocument.Parse(json);return doc.RootElement.TryGetProperty("Data",out var data) && data.ValueKind==JsonValueKind.Object && data.TryGetProperty(PayloadKey,out _);}
    public static CompanyScope OwnerCompany(string json)
    {
        if(json.Length>MaximumBodyLength+65536)throw new InvalidOperationException("The original program assignment ownership exceeds its bound.");using var doc=JsonDocument.Parse(json);var row=doc.RootElement.GetProperty("Data").GetProperty(PayloadKey).GetProperty("Source").GetProperty("Program");var company=new CompanyScope(row.GetProperty("OWNER_TENANT_ID").GetString()!,row.GetProperty("OWNER_COMPANY_BA_ID").GetString()!);APInvoiceOwnership.RequireScope(company);return company;
    }
    public static DrillingProgramReviewStart FromPayload(string json,string instance,string field,string program)
    {
        if(json.Length>MaximumBodyLength+65536)throw new InvalidOperationException("The original program assignment exceeds its bound.");using var doc=JsonDocument.Parse(json);if(doc.RootElement.GetProperty("InstanceId").GetString()!=instance)throw new InvalidOperationException("The program process envelope changed.");var value=doc.RootElement.GetProperty("Data").GetProperty(PayloadKey).Deserialize<DrillingProgramReviewStart>()??throw new InvalidOperationException("The original program assignment is unavailable.");value.Require();
        if(value.InstanceId()!=instance || value.Scope().FieldId!=field || value.Source.Program.PROGRAM_ID!=program)throw new InvalidOperationException("The original program assignment source identity changed.");return value;
    }
}
public sealed record DrillingProgramReviewStartReceipt([property:JsonRequired]string IntentRevision,[property:JsonRequired]string InstanceId,[property:JsonRequired]bool Recorded,
    [property:JsonRequired]string? StepInstanceId,[property:JsonRequired]IReadOnlyList<string> TaskIds,[property:JsonRequired]string RecordedRevision,[property:JsonRequired]DateTimeOffset CheckedAtUtc)
{
    public void Require(DrillingProgramReviewStart original)
    {
        original.Require();if(IntentRevision!=original.Revision() || InstanceId!=original.InstanceId() || TaskIds is null || TaskIds.Count>128 || TaskIds.Any(id=>!LeaseSummaryValidator.Identifier(id)) || TaskIds.Distinct(StringComparer.Ordinal).Count()!=TaskIds.Count ||
            !ProcessDefinitionRevision.IsValid(RecordedRevision) || CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero || (Recorded?StepInstanceId!=StepId(InstanceId,original.Workflow.Stages[0].Definition.StepId) || TaskIds.Count!=original.Workflow.Stages[0].Definition.PersonaCodes.Count:StepInstanceId is not null || TaskIds.Count!=0))
            throw new InvalidOperationException("The program assignment receipt does not confirm the exact retained original and first destinations.");
    }
    public static string StepId(string instance,string step)=>"program-review-step-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{instance,step}));
}
