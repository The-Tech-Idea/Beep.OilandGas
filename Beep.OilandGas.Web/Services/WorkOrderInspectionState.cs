using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data.WorkOrder;
namespace Beep.OilandGas.Web.Services;
/// <summary>Page-lifetime retained intent; durable receipts remain required.</summary>
public sealed class WorkOrderInspectionState : IDisposable
{
    private readonly CompanyClientContext company;
    private readonly HashSet<(string Actor,TheTechIdea.Data.OilGas.CompanyScope Company,string Field,string Order)> pending=[];
    private bool disposed;
    private long accessGeneration;
    public long Version {get;private set;}
    public string? Field {get;private set;}
    public string? Order {get;private set;}
    public bool Confirmed {get;private set;}
    public bool ReviewingWriteAccess {get;private set;}
    public bool WriteAccessConfirmed {get;private set;}
    public string? WriteAccessError {get;private set;}
    public bool CanEdit=>Confirmed && WriteAccessConfirmed && !ReviewingWriteAccess && !Blocked;
    public bool CanEditPermitPrerequisites=>CanEdit && company.Selection?.Permissions.Contains("HSE.ApprovePermit",StringComparer.Ordinal)==true;
    public IReadOnlyList<InspectionCondition> Rows {get;private set;}=[];
    public IReadOnlyList<CategoryProgress> Progress=>Confirmed
        ? Rows.GroupBy(row=>row.CondType,StringComparer.Ordinal).OrderBy(group=>group.Key,StringComparer.Ordinal)
            .Select(group=>new CategoryProgress(group.Key,group.Count(),group.Count(row=>row.Status=="UNRECORDED"),
                group.Count(row=>row.Status=="PASS"),group.Count(row=>row.Status=="FAIL"),group.Count(row=>row.Status=="NA"))).ToArray()
        : [];
    public sealed record CategoryProgress(string Category,int Requirements,int Unrecorded,int Passed,int Failed,int NotApplicable);
    public bool Blocked=>company.Selection is {} c && Field is not null && Order is not null && pending.Contains((c.UserId,c.Scope,Field,Order));
    public WorkOrderInspectionState(CompanyClientContext company){this.company=company;company.Changed+=Clear;}
    public void Clear(){++Version;++accessGeneration;Rows=[];Field=null;Order=null;Confirmed=false;WriteAccessConfirmed=false;ReviewingWriteAccess=false;WriteAccessError=null;}
    public async Task ReviewWriteAccessAsync(Func<string,string,string,Task<bool>> check)
    {
        WriteAccessConfirmed=false;WriteAccessError=null;
        var version=Version;var accessVersion=++accessGeneration;var scope=company.Snapshot;var field=Field;var order=Order;
        if(disposed || !Confirmed || field is null || order is null || scope.Selection is null)return;
        bool Current()=>!disposed && Version==version && accessVersion==accessGeneration && scope==company.Snapshot && Field==field && Order==order;
        ReviewingWriteAccess=true;
        try
        {
            var allowed=await check(scope.Selection.UserId,field,"FIELD");
            if(!Current())return;
            if(allowed)allowed=await check(scope.Selection.UserId,order,"WORK_ORDER");
            if(!Current())return;
            WriteAccessConfirmed=allowed;
            if(!allowed)WriteAccessError="Current WRITE access to the original FIELD and work order is required to edit inspections.";
        }
        catch {if(Current())WriteAccessError="Inspection write access could not be confirmed. Recorded evidence remains available.";throw;}
        finally{if(Current())ReviewingWriteAccess=false;}
    }
    public async Task LoadAsync(string field,string order,Func<Task<List<InspectionCondition>>> read)
    {
        Clear();var version=Version;var scope=company.Snapshot;if(disposed || scope.Selection is null)return;
        var rows=await read();if(disposed || version!=Version || scope!=company.Snapshot)return;
        RequireRows(field,order,rows);Field=field;Order=order;Rows=rows.AsReadOnly();Confirmed=true;
    }
    public static void RequireRows(string field,string order,IReadOnlyList<InspectionCondition>? rows)=>WorkOrderInspectionRules.RequireProjected(field,order,rows);
    private CompanyClientSelection Begin()
    {
        if(disposed || !CanEdit || Field is null || Order is null || company.Selection is not {} c)throw new InvalidOperationException("Review recorded inspections and current write access before submitting an action.");
        if(Blocked)throw new InvalidOperationException("An earlier inspection outcome is unconfirmed. Review its recorded outcome before another submission.");return c;
    }
    public Attempt BeginRequirement(AddWorkOrderInspectionRequirementRequest request)
    {
        var c=Begin();if(request.FieldId!=Field)throw new InvalidOperationException("Requirement FIELD changed.");if(request.PermitPrerequisiteKind is not null && !CanEditPermitPrerequisites)throw new UnauthorizedAccessException("Current HSE.ApprovePermit is required for explicit prerequisite inspections.");
        WorkOrderInspectionRules.PrepareRequirement(request,Order!,c.UserId,Guid.NewGuid().ToString("N"),DateTime.UtcNow);
        var copy=new AddWorkOrderInspectionRequirementRequest{FieldId=request.FieldId,CondSeq=request.CondSeq,CondType=request.CondType,RequiredText=request.RequiredText,Jurisdiction=request.Jurisdiction,RegulatoryRef=request.RegulatoryRef,ApplicableStage=request.ApplicableStage,PermitPrerequisiteKind=request.PermitPrerequisiteKind};
        pending.Add((c.UserId,c.Scope,Field!,Order!));return new(Version,c,Field!,Order!,request.CondSeq,copy,null,null){CompanyVersion=company.Snapshot.Version};
    }
    public Attempt BeginResult(InspectionCondition source,string result,string notes)
    {
        var c=Begin();if(!Rows.Contains(source) || source.Status!="UNRECORDED" || result is not ("PASS" or "FAIL" or "NA") || string.IsNullOrWhiteSpace(notes))throw new InvalidOperationException("Choose an unrecorded reviewed requirement, explicit result and rationale.");
        if(source.PermitPrerequisiteKind is not null && !CanEditPermitPrerequisites)throw new UnauthorizedAccessException("Current HSE.ApprovePermit is required for explicit prerequisite results.");
        var request=new RecordInspectionResultRequest{FieldId=Field!,InspectionId=source.InspectionId,ExpectedSourceRevision=source.SourceRevision,Result=result,Notes=notes};
        pending.Add((c.UserId,c.Scope,Field!,Order!));return new(Version,c,Field!,Order!,source.CondSeq,null,request,source){CompanyVersion=company.Snapshot.Version};
    }
    public bool IsCurrent(Attempt a)=>!disposed && Version==a.Version && company.Snapshot==(a.CompanyVersion,a.Company) && Field==a.Field && Order==a.Order;
    public void Confirm(Attempt a,InspectionCondition response,InspectionCondition readback)
    {
        RequireRows(a.Field,a.Order,[response]);RequireRows(a.Field,a.Order,[readback]);
        if(response!=readback || response.CondSeq!=a.Seq)throw new InvalidOperationException("Inspection response and recorded readback do not confirm the original command.");
        if(a.Requirement is {} requirement)
        {
            if(response.ApplicableStage!=requirement.ApplicableStage || response.CondType!=requirement.CondType || response.RequiredText!=requirement.RequiredText || response.Jurisdiction!=requirement.Jurisdiction || response.RegulatoryRef!=requirement.RegulatoryRef || response.PermitPrerequisiteKind!=requirement.PermitPrerequisiteKind || response.RequirementActor!=a.Company.UserId || response.Status!="UNRECORDED")
                throw new InvalidOperationException("The saved requirement differs from its submitted intent.");
        }
        else if(a.Result is {} result && (response.InspectionId!=result.InspectionId || response.Status!=result.Result || response.ResultText!=result.Notes || response.Inspector!=a.Company.UserId || response.SourceRevision==result.ExpectedSourceRevision ||
            response.ApplicableStage!=a.Source!.ApplicableStage || response.CondType!=a.Source!.CondType || response.RequiredText!=a.Source.RequiredText || response.Jurisdiction!=a.Source.Jurisdiction || response.RegulatoryRef!=a.Source.RegulatoryRef || response.PermitPrerequisiteKind!=a.Source.PermitPrerequisiteKind || response.RequirementActor!=a.Source.RequirementActor))
            throw new InvalidOperationException("The saved result differs from the original reviewed requirement and result intent.");
        pending.Remove((a.Company.UserId,a.Company.Scope,a.Field,a.Order));
    }
    public void Dispose(){disposed=true;company.Changed-=Clear;Clear();}
    public sealed record Attempt(long Version,CompanyClientSelection Company,string Field,string Order,int Seq,AddWorkOrderInspectionRequirementRequest? Requirement,RecordInspectionResultRequest? Result,InspectionCondition? Source)
    {
        public long CompanyVersion {get;init;}
    }
}
