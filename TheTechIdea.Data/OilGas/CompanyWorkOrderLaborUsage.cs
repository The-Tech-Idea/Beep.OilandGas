using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Beep.OilandGas.Models.Data;
namespace TheTechIdea.Data.OilGas;

/// <summary>Exact elapsed-time units. Unspecified cannot become a default unit; calendar days are not elapsed-time units.</summary>
public enum WorkOrderLaborTimeUnit { Unspecified, Second, Minute, Hour }

public sealed record CompanyWorkOrderLaborOccupancy(string LaborId,string Revision,DateTime FromUtc,DateTime ToUtc)
{
    public void Require()
    {
        if(!LeaseSummaryValidator.Identifier(LaborId) || !ProcessDefinitionRevision.IsValid(Revision) ||
            FromUtc==default || FromUtc.Kind!=DateTimeKind.Utc || ToUtc.Kind!=DateTimeKind.Utc || ToUtc<=FromUtc)
            throw new InvalidOperationException("Original labor identity/audit and complete increasing UTC interval are required.");
    }
}

/// <summary>Current own-executor assignment and complete employee time population, without other tasks' private reports.</summary>
public sealed record CompanyWorkOrderLaborUsageReview(CompanyWorkOrderSubmissionInspectionReview Execution,
    CompanyMaintenanceAssignedStageReview Stage,CompanyWorkOrderPersonnelAssignmentFacts Assignment,
    IReadOnlyList<CompanyWorkOrderLaborOccupancy> Occupancies,DateTime? AssignmentReleasedAtUtc,DateTime ComparedAtUtc)
{
    public const int MaximumEntries=4096;
    public void Require()
    {
        if(Execution?.Current?.Context is not {} context || Assignment is null || Stage is null || Occupancies is null)
            throw new InvalidOperationException("Current original executor/start/assignment and complete time population are required.");
        var start=Execution.Start;var reference=new CompanyWorkOrderExecutionStartReference(start.Permit,start.IntentId);
        Execution.Require(context.Company,context.ActorUserId,reference);Assignment.Require();Stage.Require(context.Company,context.ActorUserId,start.Permit.Task);
        if(Execution.Current.Blockers.Count!=0 || Assignment.Company!=context.Company || Assignment.Database!=context.Database ||
            Assignment.Permit!=start.Permit || Assignment.EmployeeBaId!=start.EmployeeBaId || Stage.AssignedRole!=start.AssignedRole || Stage.StepId!=start.StepId ||
            Assignment.ValidFromUtc>start.StartedAtUtc || Assignment.ValidToUtc<Execution.Current.Approval.ValidToUtc ||
            ComparedAtUtc.Kind!=DateTimeKind.Utc || ComparedAtUtc<Execution.ComparedAtUtc || ComparedAtUtc>=Execution.Current.Approval.ValidToUtc ||
            Occupancies.Count>MaximumEntries || Occupancies.Any(row=>row is null) || Occupancies.Select(row=>row.LaborId).Distinct(StringComparer.Ordinal).Count()!=Occupancies.Count ||
            AssignmentReleasedAtUtc is {} released && (released.Kind!=DateTimeKind.Utc || released<Assignment.AssignedAtUtc || released>ComparedAtUtc))
            throw new InvalidOperationException("Original company/employee/duty/stage/window or complete labor membership differs.");
        foreach(var row in Occupancies)row.Require();
    }
    public string SourceRevision()
    {
        Require();return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{
            Execution=Execution.SourceRevision(),Stage,Assignment,Occupancies=Occupancies.OrderBy(row=>row.LaborId,StringComparer.Ordinal),AssignmentReleasedAtUtc}));
    }
}

/// <summary>Immutable original labor report; no wage, valuation, currency or journal posting is inferred.</summary>
public sealed class WORK_ORDER_LABOR_USAGE:ModelEntityBase
{
    [Key,Required,MaxLength(128)] public string LABOR_ID{get;set;}="";
    [Required,MaxLength(64)] public string OWNER_TENANT_ID{get;set;}="";
    [Required,MaxLength(128)] public string OWNER_COMPANY_BA_ID{get;set;}="";
    [Required,MaxLength(128)] public string FIELD_ID{get;set;}="";
    [Required,MaxLength(128)] public string WORK_ORDER_ID{get;set;}="";
    [Required,MaxLength(128)] public string PROCESS_INSTANCE_ID{get;set;}="";
    [Required,MaxLength(128)] public string START_ID{get;set;}="";
    [Required,MaxLength(128)] public string ASSIGNMENT_ID{get;set;}="";
    [Required,MaxLength(128)] public string EMPLOYEE_BA_ID{get;set;}="";
    [Required,MaxLength(128)] public string EXECUTOR_USER_ID{get;set;}="";
    [Required,MaxLength(128)] public string DUTY_CODE{get;set;}="";
    public DateTime LABOR_FROM_UTC{get;set;}
    public DateTime LABOR_TO_UTC{get;set;}
    public decimal QUANTITY{get;set;}
    [Required,MaxLength(32)] public string QUANTITY_UNIT{get;set;}="";
    [Required,MaxLength(8*1024*1024)] public string COMMAND_JSON{get;set;}="";
    [Required,MaxLength(64)] public string RECORDED_REVISION{get;set;}="";
}

public sealed record CompanyWorkOrderLaborUsageCommand(CompanyWorkOrderLaborUsageReview Review,Guid IntentId,
    DateTime FromUtc,DateTime ToUtc,decimal Quantity,WorkOrderLaborTimeUnit Unit,string WorkPerformed,string Reason)
{
    public const int MaximumBodyLength=8*1024*1024;
    public static long TicksPerUnit(WorkOrderLaborTimeUnit unit)=>unit switch{
        WorkOrderLaborTimeUnit.Second=>TimeSpan.TicksPerSecond,WorkOrderLaborTimeUnit.Minute=>TimeSpan.TicksPerMinute,
        WorkOrderLaborTimeUnit.Hour=>TimeSpan.TicksPerHour,_=>throw new ArgumentException("Choose an explicit supported elapsed-time unit.")};
    public void Require()
    {
        if(Review is null)throw new ArgumentException("Original reviewed start and duty assignment are required.");Review.Require();
        var start=Review.Execution.Start;var approval=Review.Execution.Current.Approval;
        var unitTicks=TicksPerUnit(Unit);var elapsedTicks=ToUtc.Ticks-FromUtc.Ticks;
        if(IntentId==Guid.Empty || FromUtc.Kind!=DateTimeKind.Utc || ToUtc.Kind!=DateTimeKind.Utc || ToUtc<=FromUtc ||
            FromUtc<start.StartedAtUtc || FromUtc<Review.Assignment.ValidFromUtc || ToUtc>Review.ComparedAtUtc ||
            ToUtc>Review.Assignment.ValidToUtc || ToUtc>=approval.ValidToUtc ||
            Review.AssignmentReleasedAtUtc is {} released && ToUtc>released || Quantity<=0 ||
            Quantity>(decimal)elapsedTicks/unitTicks || Quantity*unitTicks!=elapsedTicks ||
            string.IsNullOrWhiteSpace(WorkPerformed) || WorkPerformed.Length>16384 || WorkPerformed.Any(value=>char.IsControl(value) && value is not ('\r' or '\n' or '\t')) ||
            string.IsNullOrWhiteSpace(Reason) || Reason.Length>4096 || Reason.Any(char.IsControl))
            throw new ArgumentException("Report the actual positive elapsed quantity/unit and UTC interval inside the original start/duty/window, with work and rationale.");
        if(Review.Occupancies.Any(row=>FromUtc<row.ToUtc && ToUtc>row.FromUtc))
            throw new InvalidOperationException("This employee already has recorded labor in that interval; overlapping work is not duplicated.");
        if(JsonSerializer.Serialize(this).Length>MaximumBodyLength)throw new InvalidOperationException("Original labor report exceeds its retained capacity.");
    }
    public string LaborId()=>"wo-labor-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Company=Review.Execution.Start.Company,IntentId}));
    public CompanyWorkOrderLaborUsageReference Reference()=>new(new(Review.Execution.Start.Permit,Review.Execution.Start.IntentId),IntentId);
    public void RequireCurrent(CompanyWorkOrderLaborUsageReview current)
    {
        Require();current.Require();if(current.ComparedAtUtc<Review.ComparedAtUtc || current.SourceRevision()!=Review.SourceRevision())
            throw new InvalidOperationException("Original start/duty/requirement/time population changed before labor reporting.");
    }
    public WORK_ORDER_LABOR_USAGE Prepare(DateTime utc)
    {
        Require();if(utc.Kind!=DateTimeKind.Utc || utc<Review.ComparedAtUtc || utc<ToUtc || utc>=Review.Execution.Current.Approval.ValidToUtc)
            throw new InvalidOperationException("Current explicit labor audit UTC within the original permit window is required.");
        var context=Review.Execution.Current.Context;var start=Review.Execution.Start;var task=start.Permit.Task;
        var row=new WORK_ORDER_LABOR_USAGE{LABOR_ID=LaborId(),OWNER_TENANT_ID=context.Company.TenantId,OWNER_COMPANY_BA_ID=context.Company.BusinessAssociateId,
            FIELD_ID=task.FieldId,WORK_ORDER_ID=task.WorkOrderId,PROCESS_INSTANCE_ID=task.InstanceId,START_ID=start.StartId,ASSIGNMENT_ID=Review.Assignment.AssignmentId,
            EMPLOYEE_BA_ID=start.EmployeeBaId,EXECUTOR_USER_ID=context.ActorUserId,DUTY_CODE=Review.Assignment.DutyCode,LABOR_FROM_UTC=FromUtc,LABOR_TO_UTC=ToUtc,
            QUANTITY=Quantity,QUANTITY_UNIT=Unit.ToString(),COMMAND_JSON=JsonSerializer.Serialize(this),ACTIVE_IND="Y",SOURCE="OILGAS_WORK_ORDER_LABOR",
            PPDM_GUID=IntentId.ToString("D"),ROW_CREATED_BY=context.ActorUserId,ROW_CHANGED_BY=context.ActorUserId,ROW_CREATED_DATE=utc,ROW_CHANGED_DATE=utc};
        row.RECORDED_REVISION=Digest(row);return row;
    }
    public static string Digest(WORK_ORDER_LABOR_USAGE row)
    {var copy=CompanyInventorySourceReview.Copy(row);copy.RECORDED_REVISION="";return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    public void RequireRecorded(WORK_ORDER_LABOR_USAGE row)
    {
        if(row?.ROW_CREATED_DATE is not {} utc || JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(Prepare(utc)))
            throw new InvalidOperationException("Original labor quantity/employee/duty/body/audit differs.");
    }
    public static CompanyWorkOrderLaborUsageCommand Read(WORK_ORDER_LABOR_USAGE row)
    {
        if(row?.COMMAND_JSON is not {Length:>0 and <=MaximumBodyLength} body)throw new InvalidOperationException("Original bounded labor report is unavailable.");
        try{var command=JsonSerializer.Deserialize<CompanyWorkOrderLaborUsageCommand>(body)??throw new InvalidOperationException("Original labor command is unavailable.");command.RequireRecorded(row);return command;}
        catch(JsonException error){throw new InvalidOperationException("Original labor report is malformed.",error);}
    }
}
public sealed record CompanyWorkOrderLaborUsageReference(CompanyWorkOrderExecutionStartReference Start,Guid IntentId)
{
    public void Require(){if(Start is null || IntentId==Guid.Empty)throw new ArgumentException("Retain the original start and labor intent reference.");Start.Require();}
}
public sealed record CompanyWorkOrderLaborUsageRecovery(CompanyWorkOrderLaborUsageCommand Command,WORK_ORDER_LABOR_USAGE Recorded)
{
    public void Require(CompanyScope company,string actor,CompanyWorkOrderLaborUsageReference reference)
    {
        reference.Require();if(Command?.Review?.Execution?.Current?.Context is not {} context || context.Company!=company || context.ActorUserId!=actor ||
            Command.IntentId!=reference.IntentId || Command.Reference()!=reference)throw new UnauthorizedAccessException("Original private labor report belongs to another company/account/start/reference.");
        Command.RequireRecorded(Recorded);
    }
}
