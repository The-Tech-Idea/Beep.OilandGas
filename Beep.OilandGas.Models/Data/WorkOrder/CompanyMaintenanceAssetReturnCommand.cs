using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.WorkOrder;

public sealed record CompanyMaintenanceAssetReturnReview(CompanyMaintenanceAssetEvidence Assets,CompanyWorkOrderReturnReview Physical)
{
    public void Require()
    {
        if(Assets is null || Physical is null)throw new InvalidOperationException("Original asset and current independent physical return evidence are required.");Assets.Require();Physical.RequireAcceptance();var stage=Physical.Assignment.Stage;
        if(Assets.Wells.Count==0 && Assets.QualifiedAssets is not {Facilities.Count:>0} || Assets.Blockers.Any(value=>value!=CompanyMaintenanceAssetEvidence.PendingCommand && value!=CompanyMaintenanceAssetEvidence.PendingQualifiedCommand) || Physical.Completion.Context!=Assets.Context || JsonSerializer.Serialize(Physical.Completion.Report)!=JsonSerializer.Serialize(Assets.Report) || JsonSerializer.Serialize(Physical.Completion.Order)!=JsonSerializer.Serialize(Assets.Order) || stage.Context.Company!=Assets.Context.Company || stage.Context.ActorUserId!=Assets.Context.ActorUserId || stage.AssignedRole!=Assets.Closure.Role || Physical.PhysicalSubjectRevision()!=Assets.Closure.PhysicalSubjectRevision || Physical.Completion.ComparedAtUtc<Assets.Closure.RecordedAtUtc)
            throw new InvalidOperationException("Original closure, complete affected assets, current physical subject, configured independent role and all asset WRITE evidence must agree before return.");
        if(Assets.QualifiedAssets is {} qualified)CompanyMaintenanceQualifiedAssetReturn.RequireReview(qualified,Assets.Order,stage.AssignedRole);
    }
    public string SourceRevision(){Require();return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Assets=Assets.SourceRevision(),Physical=Physical.SourceRevision()}));}
}
public sealed record CompanyWellReturnActivation(string WellId,string RecordingSource,decimal? ObservedCapabilityPercent)
{public void Require(){if(!LeaseSummaryValidator.Identifier(WellId) || WellId.Length>128 || !LeaseSummaryValidator.Identifier(RecordingSource) || RecordingSource.Length>128 || ObservedCapabilityPercent is null or <0 or >100)throw new ArgumentException("Every original well requires its explicit original source stream and observed capability percentage between 0 and 100.");}}
public sealed record CompanyMaintenanceAssetReturnReference(CompanyWorkOrderCompletionQuery Query,Guid IntentId)
{
    public void Require(){if(Query is null || IntentId==Guid.Empty)throw new ArgumentException("Original closed report and retained asset-return intent are required.");Query.Require();}
    public string ReturnId(CompanyScope company){Require();return SourceId(company,Query);}
    public static string SourceId(CompanyScope company,CompanyWorkOrderCompletionQuery query){PurchaseOrderOwnership.RequireScope(company);query.Require();return "asset-return-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Company=company,Query=query}));}
}
public sealed record CompanyMaintenanceAssetReturnCommand(CompanyMaintenanceAssetReturnReview Review,Guid IntentId,DateTime ActivatedAtUtc,IReadOnlyList<CompanyWellReturnActivation> Wells,string Reason,bool Confirmed)
{
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<CompanyMaintenanceQualifiedReturnObservation>? QualifiedAssets{get;init;}
    public const int MaximumBodyLength=1024*1024;
    public void Require()
    {
        if(Review is null)throw new ArgumentException("Original reviewed asset-return evidence is required.");Review.Require();if(IntentId==Guid.Empty || !Confirmed || ActivatedAtUtc.Kind!=DateTimeKind.Utc || ActivatedAtUtc<Review.Assets.Closure.RecordedAtUtc || ActivatedAtUtc>DateTime.UtcNow || Wells is null || Wells.Count>128 || Wells.Count==0 && QualifiedAssets is not {Count:>0} || string.IsNullOrWhiteSpace(Reason) || Reason.Length>4096 || Reason.Any(ch=>char.IsControl(ch) && ch is not ('\r' or '\n' or '\t')))throw new ArgumentException("Confirm original return evidence, explicit actual activation UTC, observed capability, reason and retained intent.");
        foreach(var row in Wells)row.Require();if(!Wells.Select(row=>row.WellId).Order(StringComparer.Ordinal).SequenceEqual(Review.Assets.Wells.Select(row=>row.WellId).Order(StringComparer.Ordinal),StringComparer.Ordinal))throw new InvalidOperationException("The complete original affected well population is required; targets cannot be omitted or duplicated.");
        foreach(var well in Review.Assets.Wells)
        {
            if(Wells.Single(row=>row.WellId==well.WellId).RecordingSource!=well.Facets.Single(row=>row.Facet=="Condition" && row.ActiveIndicator=="Y" && row.Covers(well.ComparedAtUtc)).Source)throw new InvalidOperationException("Confirm the original recorded condition source stream for every well.");
            if(well.Facets.Where(row=>row.ActiveIndicator=="Y" && row.Covers(well.ComparedAtUtc)).Any(row=>!row.Covers(ActivatedAtUtc)) || !well.References.Any(row=>row.Facet=="Condition" && row.Value=="Active" && row.Covers(ActivatedAtUtc)))throw new InvalidOperationException("Explicit activation UTC is outside original native current-state or target-reference coverage.");
        }
        if(Review.Assets.QualifiedAssets is {} qualified)
        {
            if(QualifiedAssets is null || ActivatedAtUtc<Review.Physical.Completion.ComparedAtUtc)throw new InvalidOperationException("Complete qualified observations and physical review before activation are required.");
            _=CompanyMaintenanceQualifiedAssetReturn.Resolve(qualified,Review.Assets.Order,QualifiedAssets,Review.Physical.Assignment.Stage.AssignedRole,ActivatedAtUtc);
        }
        else if(QualifiedAssets is not null)throw new InvalidOperationException("Qualified observations cannot add targets to an earlier well-only return.");
        if(JsonSerializer.Serialize(this).Length>MaximumBodyLength)throw new ArgumentException("Original asset-return command exceeds its bounded retained body.");
    }
    public CompanyMaintenanceAssetReturnReference Reference()=>new(Review.Assets.Report.Query,IntentId);
    public string ReturnId()=>Reference().ReturnId(Review.Assets.Context.Company);
    public string StatusId(string well){if(!Review.Assets.Wells.Any(row=>row.WellId==well))throw new ArgumentException("Original affected well is required.");return "mr-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Return=ReturnId(),Well=well}))[..32];}
}
/// <summary>Immutable CORE return intent and exact original/effect proof; provisioned only by the explicit Beep manifest.</summary>
public sealed class WORK_ORDER_ASSET_RETURN:ModelEntityBase
{
    [Key,Required,MaxLength(128)]public string RETURN_ID{get;set;}="";
    [Required,MaxLength(64)]public string OWNER_TENANT_ID{get;set;}="";
    [Required,MaxLength(128)]public string OWNER_COMPANY_BA_ID{get;set;}="";
    [Required,MaxLength(128)]public string FIELD_ID{get;set;}="";
    [Required,MaxLength(128)]public string WORK_ORDER_ID{get;set;}="";
    [Required,MaxLength(128)]public string PROCESS_INSTANCE_ID{get;set;}="";
    [Required,MaxLength(128)]public string SUBMISSION_ID{get;set;}="";
    [Required,MaxLength(128)]public string ACTOR_USER_ID{get;set;}="";
    [Required,MaxLength(128)]public string ASSIGNED_ROLE{get;set;}="";
    [Required,MaxLength(36)]public string INTENT_ID{get;set;}="";
    public DateTime ACTIVATED_AT_UTC{get;set;}
    public DateTime RECORDED_AT_UTC{get;set;}
    [Required,MaxLength(64)]public string SOURCE_REVISION{get;set;}="";
    [Required,MaxLength(CompanyMaintenanceAssetReturnCommand.MaximumBodyLength)]public string COMMAND_JSON{get;set;}="";
    [Required,MaxLength(2*1024*1024)]public string BEFORE_JSON{get;set;}="";
    [Required,MaxLength(2*1024*1024)]public string AFTER_JSON{get;set;}="";
    [Required,MaxLength(64)]public string RECORDED_REVISION{get;set;}="";
}
public sealed record CompanyMaintenanceAssetReturnReceipt(CompanyScope Company,AssetDatabaseScope Database,string ActorUserId,Guid IntentId,string ReturnId,CompanyWorkOrderCompletionQuery Query,IReadOnlyList<string> WellIds,DateTime ActivatedAtUtc,DateTime RecordedAtUtc,string Revision)
{
    public void Require(CompanyMaintenanceAssetReturnCommand command){command.Require();var context=command.Review.Assets.Context;if(Company!=context.Company || Database!=context.Database || ActorUserId!=context.ActorUserId || IntentId!=command.IntentId || ReturnId!=command.ReturnId() || Query!=command.Reference().Query || WellIds is null || !WellIds.SequenceEqual(command.Wells.Select(row=>row.WellId).Order(StringComparer.Ordinal),StringComparer.Ordinal) || ActivatedAtUtc!=command.ActivatedAtUtc || RecordedAtUtc.Kind!=DateTimeKind.Utc || RecordedAtUtc<command.Review.Physical.Completion.ComparedAtUtc || RecordedAtUtc<ActivatedAtUtc || !ProcessDefinitionRevision.IsValid(Revision))throw new InvalidOperationException("Recorded asset return differs from its original company/account/source/complete wells/UTC/intent.");}
}
public sealed record CompanyMaintenanceAssetReturnRecovery(CompanyMaintenanceAssetReturnCommand Command,CompanyMaintenanceAssetReturnReceipt Receipt)
{
    public void Require(CompanyScope company,string actor,CompanyMaintenanceAssetReturnReference reference){reference.Require();if(Command?.Review?.Assets?.Context is not {} context || context.Company!=company || context.ActorUserId!=actor || Command.Reference()!=reference)throw new UnauthorizedAccessException("Original private asset return belongs to another company/account/report/reference.");Receipt.Require(Command);}
}
public interface ICompanyMaintenanceAssetReturnOwner:ICompanyMaintenanceWellEvidenceOwner
{
    Task<CompanyMaintenanceAssetReturnFacts?> ReadMaintenanceAssetReturnFactsAsync(FieldSelectionReview context,CompanyWorkOrderCompletionQuery query,Func<Task> authority);
    Task<CompanyMaintenanceAssetReturnRecovery> RecordMaintenanceAssetReturnAsync(CompanyMaintenanceAssetReturnCommand command,Func<string?,Task> authority,Func<bool,Task<CompanyWorkOrderReturnReview>> readPhysical,Func<bool,Task<CompanyMaintenanceClosureFacts?>> readClosure);
    Task<CompanyMaintenanceAssetReturnRecovery> RecoverMaintenanceAssetReturnAsync(FieldSelectionReview context,CompanyMaintenanceAssetReturnReference reference,Func<string?,Task> authority);
}
public sealed record CompanyMaintenanceClosedReturnSource(CompanyMaintenanceReturnAssignment Assignment,CompanyMaintenanceClosureFacts Closure)
{
    public void Require(){if(Assignment is null || Closure is null)throw new InvalidOperationException("Original recorded closure and configured return source are required.");Assignment.Require();Closure.Require();if(Assignment.Stage.Context.Company!=Closure.Company || Assignment.Stage.Task!=Closure.Query.Task || Assignment.Verification.Query!=Closure.Query || Assignment.Stage.AssignedRole!=Closure.Role)throw new InvalidOperationException("Original closed process/report/configured role source differs.");}
}
public interface ICompanyMaintenanceClosedReturnOwner
{
    Task<CompanyMaintenanceClosedReturnSource?> ReadClosedReturnSourceAsync(FieldSelectionReview context,CompanyWorkOrderCompletionQuery query,string coreConnection,bool ownedCore,Func<string?,Task> authority);
}
