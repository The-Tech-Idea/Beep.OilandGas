using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.WorkOrder;

/// <summary>Append-only full release of one original reservation; never a physical stock return.</summary>
public sealed class WORK_ORDER_MATERIAL_RELEASE:ModelEntityBase
{
    [Key,Required,MaxLength(128)] public string RELEASE_ID {get;set;}="";
    [Required,MaxLength(128)] public string ALLOCATION_ID {get;set;}="";
    [Required,MaxLength(64)] public string OWNER_TENANT_ID {get;set;}="";
    [Required,MaxLength(128)] public string OWNER_COMPANY_BA_ID {get;set;}="";
    [Required,MaxLength(128)] public string INVENTORY_ITEM_ID {get;set;}="";
    [Required,MaxLength(128)] public string FIELD_ID {get;set;}="";
    [Required,MaxLength(128)] public string WORK_ORDER_ID {get;set;}="";
    [Required,MaxLength(128)] public string PROCESS_INSTANCE_ID {get;set;}="";
    public decimal RELEASED_QUANTITY {get;set;}
    public DateTime RELEASED_AT_UTC {get;set;}
    [Required,MaxLength(4194304)] public string COMMAND_JSON {get;set;}="";
    [Required,MaxLength(64)] public string RECORDED_REVISION {get;set;}="";
}
public sealed record CompanyWorkOrderMaterialReleaseCommand(CompanyWorkOrderMaterialAllocationRecovery Original,Guid IntentId,string Reason)
{
    public void Require()
    {
        if(Original?.Command is null || Original.Recorded is null)throw new ArgumentException("The complete original reservation is required for release.");var source=Original.Command.Review.Source;Original.Require(source.Context.Company,source.Context.ActorUserId,new(source.Query.Task,Original.Command.IntentId));
        if(IntentId==Guid.Empty || string.IsNullOrWhiteSpace(Reason) || Reason.Length>2048 || Reason.Any(char.IsControl))throw new ArgumentException("Retain a new release intent and explicit rationale.");
    }
    public string ReleaseId(){Require();return "wo-material-release-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Original.Recorded.ALLOCATION_ID}));}
    public WORK_ORDER_MATERIAL_RELEASE Prepare(DateTime utc)
    {
        Require();if(utc==default || utc.Kind!=DateTimeKind.Utc || utc<Original.Recorded.ROW_CREATED_DATE)throw new InvalidOperationException("An original release UTC audit after reservation is required.");var row=Original.Recorded;var body=JsonSerializer.Serialize(this);if(body.Length>4194304)throw new InvalidOperationException("Complete original release evidence exceeds retained capacity.");
        var result=new WORK_ORDER_MATERIAL_RELEASE{RELEASE_ID=ReleaseId(),ALLOCATION_ID=row.ALLOCATION_ID,OWNER_TENANT_ID=row.OWNER_TENANT_ID,OWNER_COMPANY_BA_ID=row.OWNER_COMPANY_BA_ID,INVENTORY_ITEM_ID=row.INVENTORY_ITEM_ID,FIELD_ID=row.FIELD_ID,WORK_ORDER_ID=row.WORK_ORDER_ID,PROCESS_INSTANCE_ID=row.PROCESS_INSTANCE_ID,RELEASED_QUANTITY=row.QUANTITY,RELEASED_AT_UTC=utc,COMMAND_JSON=body,PPDM_GUID=IntentId.ToString("D"),ACTIVE_IND="Y",ROW_CREATED_BY=row.ROW_CREATED_BY,ROW_CHANGED_BY=row.ROW_CREATED_BY,ROW_CREATED_DATE=utc,ROW_CHANGED_DATE=utc,SOURCE="OILGAS_WORK_ORDER_MATERIAL_RELEASE"};result.RECORDED_REVISION=Digest(result);return result;
    }
    public static string Digest(WORK_ORDER_MATERIAL_RELEASE row){var copy=CompanyInventorySourceReview.Copy(row);copy.RECORDED_REVISION="";return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    public static CompanyWorkOrderMaterialReleaseCommand Read(WORK_ORDER_MATERIAL_RELEASE row)
    {
        if(row is null || string.IsNullOrWhiteSpace(row.COMMAND_JSON) || row.COMMAND_JSON.Length>4194304)throw new InvalidOperationException("Original release evidence is unavailable.");try{return JsonSerializer.Deserialize<CompanyWorkOrderMaterialReleaseCommand>(row.COMMAND_JSON)??throw new InvalidOperationException("Original release body is unavailable.");}catch(JsonException error){throw new InvalidOperationException("Original release body is malformed.",error);}
    }
    public void RequireRecorded(WORK_ORDER_MATERIAL_RELEASE row)
    {
        Require();if(row?.ROW_CREATED_DATE is not {} utc || JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(Prepare(utc)))throw new InvalidOperationException("Original release company/task/body/quantity/audit changed.");
    }
}
public sealed record CompanyWorkOrderMaterialReleaseReference(string AllocationId,string Revision,DateTime ReleasedAtUtc,decimal Quantity)
{
    public static CompanyWorkOrderMaterialReleaseReference From(WORK_ORDER_MATERIAL_RELEASE row,WORK_ORDER_MATERIAL_ALLOCATION original)
    {
        var command=CompanyWorkOrderMaterialReleaseCommand.Read(row);command.RequireRecorded(row);
        if(JsonSerializer.Serialize(command.Original.Recorded)!=JsonSerializer.Serialize(original))throw new InvalidOperationException("Release evidence does not match the complete original reservation.");return new(row.ALLOCATION_ID,row.RECORDED_REVISION,row.RELEASED_AT_UTC,row.RELEASED_QUANTITY);
    }
}