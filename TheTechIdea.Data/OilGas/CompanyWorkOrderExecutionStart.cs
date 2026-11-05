using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Beep.OilandGas.Models.Data;
namespace TheTechIdea.Data.OilGas;

/// <summary>The current application actor is explicitly mapped to the native assigned employee.</summary>
public sealed record CompanyWorkOrderExecutorIdentity(CompanyWorkOrderExecutionPrerequisites Prerequisites,CompanyMaintenanceAssignedStageReview Stage,
    string IdentityCatalogRevision,CompanyPersonnelIdentityVersion Identity,CompanyNativePersonFacts Person,DateTime ComparedAtUtc,string Revision)
{
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public CompanyMaintenanceQualifiedAssets? QualifiedWorkAssets { get; init; }
    public static string Digest(CompanyWorkOrderExecutorIdentity value)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(value with{Revision=""}));
    public string SourceRevision()
    {
        var original=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Prerequisites=Prerequisites.SourceRevision(),Stage,IdentityCatalogRevision,Identity,Person}));
        return QualifiedWorkAssets is null?original:ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Original=original,QualifiedWork=QualifiedWorkAssets.IdentityRevision()}));
    }
    public void Require()
    {
        if(Prerequisites?.Requirements?.Context is not {} context || Identity is null || Person is null)throw new InvalidOperationException("Complete original executor identity and prerequisite evidence is required.");
        var permit=Prerequisites.Requirements.Approval;Prerequisites.Require(context.Company,context.ActorUserId,permit.Permit);Stage.Require(context.Company,context.ActorUserId,permit.Permit.Task);Identity.Require();Person.Require(context.Company,Prerequisites.Personnel.EmployeeBaId);
        if(Identity.Company!=context.Company || Identity.Database!=context.Database || Identity.UserId!=context.ActorUserId || Identity.PersonBaId!=Person.PersonBaId || Identity.RecordedBy==context.ActorUserId ||
            Identity.PersonType!=Person.Type || Identity.EffectiveFromUtc>permit.ValidFromUtc || Identity.EffectiveToUtc<permit.ValidToUtc ||
            ComparedAtUtc.Kind!=DateTimeKind.Utc || ComparedAtUtc<Prerequisites.ComparedAtUtc || ComparedAtUtc>=permit.ValidToUtc || ComparedAtUtc<Identity.EffectiveFromUtc || ComparedAtUtc>=Identity.EffectiveToUtc ||
            Person.Employment.Revision!=Prerequisites.Personnel.EmploymentRevision || !Covers(Person.EffectiveFromUtc,Person.EffectiveToUtc) || !Covers(Person.Employment.EffectiveDate,Person.Employment.ExpiryDate) ||
            !ProcessDefinitionRevision.IsValid(IdentityCatalogRevision) || Revision!=Digest(this))throw new InvalidOperationException("One independent current account/person link must bind the actual executor to the exact assigned employee for the full permit window.");
        if(QualifiedWorkAssets is {} assets)CompanyMaintenanceQualifiedWorkScope.RequireSnapshot(assets,Prerequisites.Requirements.Order,context,ComparedAtUtc);
        bool Covers(DateTime? from,DateTime? to)=>from is {} start && to is {} end && start.Kind==DateTimeKind.Utc && end.Kind==DateTimeKind.Utc && start<=permit.ValidFromUtc && end>=permit.ValidToUtc && start<=ComparedAtUtc && end>ComparedAtUtc;
    }
}

public sealed class WORK_ORDER_EXECUTION_START:ModelEntityBase
{
    [Key,Required,MaxLength(128)] public string START_ID {get;set;}="";
    [Required,MaxLength(64)] public string OWNER_TENANT_ID {get;set;}="";
    [Required,MaxLength(128)] public string OWNER_COMPANY_BA_ID {get;set;}="";
    [Required,MaxLength(128)] public string FIELD_ID {get;set;}="";
    [Required,MaxLength(128)] public string WORK_ORDER_ID {get;set;}="";
    [Required,MaxLength(128)] public string PROCESS_INSTANCE_ID {get;set;}="";
    [Required,MaxLength(128)] public string PERMIT_ID {get;set;}="";
    [Required,MaxLength(128)] public string EXECUTOR_USER_ID {get;set;}="";
    [Required,MaxLength(128)] public string EMPLOYEE_BA_ID {get;set;}="";
    public DateTime STARTED_AT_UTC {get;set;}
    [Required,MaxLength(64)] public string PREREQUISITE_REVISION {get;set;}="";
    [Required,MaxLength(8*1024*1024)] public string COMMAND_JSON {get;set;}="";
    [Required,MaxLength(64)] public string RECORDED_REVISION {get;set;}="";
}

public sealed record CompanyWorkOrderExecutionStartCommand(CompanyWorkOrderExecutorIdentity Review,Guid IntentId,DateTime StartedAtUtc,string Reason)
{
    public const int MaximumBodyLength=8*1024*1024;
    public void Require()
    {
        if(Review is null || IntentId==Guid.Empty || StartedAtUtc.Kind!=DateTimeKind.Utc || string.IsNullOrWhiteSpace(Reason) || Reason.Length>4096 || Reason.Any(char.IsControl))throw new ArgumentException("Retain the exact reviewed executor, original start intent, explicit UTC and rationale before transport.");
        Review.Require();
        if(Review.QualifiedWorkAssets is {} assets)CompanyMaintenanceQualifiedWorkScope.RequireRecorded(assets,Review.Prerequisites.Requirements.Context.Company,Review.Prerequisites.Requirements.Approval.Permit.Task,Review.Prerequisites.Requirements.Context.ActorUserId,StartedAtUtc);
        if(StartedAtUtc<Review.ComparedAtUtc || StartedAtUtc>=Review.Prerequisites.Requirements.Approval.ValidToUtc || JsonSerializer.Serialize(this).Length>MaximumBodyLength)throw new InvalidOperationException("The actual start must follow current review within the original permit window and bounded command body.");
    }
    public void RequireQualifiedWorkCapture()
    {
        Require();if(CompanyMaintenanceQualifiedWorkScope.HasTargets(Review.Prerequisites.Requirements.Order) && Review.QualifiedWorkAssets is null)
            throw new InvalidOperationException("New qualified work requires the original reviewed native identity/installation snapshot before start.");
    }
    public string StartId(){Require();return "wo-start-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Review.Prerequisites.Requirements.Context.Company,IntentId}));}
    public void RequireCurrent(CompanyWorkOrderExecutorIdentity current)
    {
        Require();current.Require();if(current.ComparedAtUtc<Review.ComparedAtUtc || current.SourceRevision()!=Review.SourceRevision())throw new InvalidOperationException("Original executor/crew/approval/order/prerequisite facts changed before work start.");
    }
    public void RequireCurrentStage(CompanyMaintenanceAssignedStageReview current)
    {Require();var context=Review.Prerequisites.Requirements.Context;current.Require(context.Company,context.ActorUserId,Review.Prerequisites.Requirements.Approval.Permit.Task);if(JsonSerializer.Serialize(current)!=JsonSerializer.Serialize(Review.Stage))throw new InvalidOperationException("Original current assigned lifecycle step/role/source changed before work start.");}
    public WORK_ORDER_EXECUTION_START Prepare(DateTime utc)
    {
        Require();if(utc.Kind!=DateTimeKind.Utc || utc<StartedAtUtc || utc>=Review.Prerequisites.Requirements.Approval.ValidToUtc)throw new InvalidOperationException("A confirmed current UTC boundary within the original permit window is required for physical start.");
        var context=Review.Prerequisites.Requirements.Context;var approval=Review.Prerequisites.Requirements.Approval;var task=approval.Permit.Task;
        var row=new WORK_ORDER_EXECUTION_START{START_ID=StartId(),OWNER_TENANT_ID=context.Company.TenantId,OWNER_COMPANY_BA_ID=context.Company.BusinessAssociateId,FIELD_ID=task.FieldId,WORK_ORDER_ID=task.WorkOrderId,PROCESS_INSTANCE_ID=task.InstanceId,PERMIT_ID=approval.PermitId,EXECUTOR_USER_ID=context.ActorUserId,EMPLOYEE_BA_ID=Review.Person.PersonBaId,STARTED_AT_UTC=StartedAtUtc,PREREQUISITE_REVISION=Review.Prerequisites.SourceRevision(),COMMAND_JSON=JsonSerializer.Serialize(this),ACTIVE_IND="Y",SOURCE="OILGAS_EXECUTION_START",PPDM_GUID=IntentId.ToString("D"),ROW_CREATED_BY=context.ActorUserId,ROW_CREATED_DATE=utc,ROW_CHANGED_BY=context.ActorUserId,ROW_CHANGED_DATE=utc};
        row.RECORDED_REVISION=Digest(row);return row;
    }
    public static string Digest(WORK_ORDER_EXECUTION_START row){var copy=CompanyInventorySourceReview.Copy(row);copy.RECORDED_REVISION="";return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    public void RequireRecorded(WORK_ORDER_EXECUTION_START row)
    {Require();if(row?.ROW_CREATED_DATE is not {} utc || JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(Prepare(utc)))throw new InvalidOperationException("Original execution start command, scope, executor or native audit changed.");}
    public static CompanyWorkOrderExecutionStartCommand Read(WORK_ORDER_EXECUTION_START row)
    {
        if(string.IsNullOrEmpty(row.COMMAND_JSON) || row.COMMAND_JSON.Length>MaximumBodyLength)throw new InvalidOperationException("Original bounded start command is unavailable.");
        try{var command=JsonSerializer.Deserialize<CompanyWorkOrderExecutionStartCommand>(row.COMMAND_JSON)??throw new InvalidOperationException("Original start command is unavailable.");command.RequireRecorded(row);return command;}
        catch(JsonException){throw new InvalidOperationException("Original start command is malformed.");}
    }
}
public sealed record CompanyWorkOrderExecutionStartReceipt(CompanyScope Company,string ActorUserId,Guid IntentId,string StartId,string PermitId,DateTime StartedAtUtc,string RecordedRevision)
{
    public void Require(CompanyWorkOrderExecutionStartCommand command)
    {
        command.Require();var context=command.Review.Prerequisites.Requirements.Context;
        if(Company!=context.Company || ActorUserId!=context.ActorUserId || IntentId!=command.IntentId || StartId!=command.StartId() || PermitId!=command.Review.Prerequisites.Requirements.Approval.PermitId || StartedAtUtc!=command.StartedAtUtc || !ProcessDefinitionRevision.IsValid(RecordedRevision))throw new InvalidOperationException("The original physical start acknowledgement differs from its retained command.");
    }
}
public sealed record CompanyWorkOrderExecutionStartReference(CompanyWorkOrderExecutionPermitReference Permit,Guid IntentId)
{
    public void Require(){Permit.Require();if(IntentId==Guid.Empty)throw new ArgumentException("Retain the original start intent reference.");}
    public string StartId(CompanyScope company){Require();return "wo-start-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Company=company,IntentId}));}
}
public sealed record CompanyWorkOrderExecutionStartRecovery(CompanyWorkOrderExecutionStartCommand Command,WORK_ORDER_EXECUTION_START Recorded)
{
    public void Require(CompanyScope company,string actor,CompanyWorkOrderExecutionStartReference reference)
    {
        reference.Require();Command.Require();var context=Command.Review.Prerequisites.Requirements.Context;
        if(context.Company!=company || context.ActorUserId!=actor || Command.IntentId!=reference.IntentId || Command.Review.Prerequisites.Requirements.Approval.Permit!=reference.Permit || Recorded.START_ID!=reference.StartId(company))throw new UnauthorizedAccessException("The private start record belongs to another original company/account/task/reference.");
        Command.RequireRecorded(Recorded);
    }
}
/// <summary>Limited recorded work-start facts for another authorized persona; never current completion or private recovery.</summary>
public sealed record CompanyWorkOrderRecordedStart(CompanyScope Company,CompanyWorkOrderExecutionPermitReference Permit,
    Guid IntentId,string StartId,string PermitId,string ExecutorUserId,string EmployeeBaId,string AssignedRole,string StepId,
    DateTime StartedAtUtc,DateTime RecordedAtUtc,string PrerequisiteRevision,string RecordedRevision)
{
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public CompanyMaintenanceQualifiedAssets? QualifiedWorkAssets { get; init; }
    public void Require(CompanyScope company,CompanyWorkOrderExecutionPermitReference query)
    {
        if(QualifiedWorkAssets is {} assets)CompanyMaintenanceQualifiedWorkScope.RequireRecorded(assets,company,query.Task,ExecutorUserId,StartedAtUtc);
        query.Require();if(Company!=company || Permit!=query || IntentId==Guid.Empty || StartId!=new CompanyWorkOrderExecutionStartReference(query,IntentId).StartId(company) || PermitId!=CompanyWorkOrderExecutionPermitIntent.PermitIdFor(company,query.IntentId) ||
            new[]{ExecutorUserId,EmployeeBaId,AssignedRole,StepId}.Any(value=>!LeaseSummaryValidator.Identifier(value)) || StartedAtUtc.Kind!=DateTimeKind.Utc || RecordedAtUtc.Kind!=DateTimeKind.Utc || StartedAtUtc>RecordedAtUtc || !ProcessDefinitionRevision.IsValid(PrerequisiteRevision) || !ProcessDefinitionRevision.IsValid(RecordedRevision))
            throw new InvalidOperationException("Complete original recorded work-start facts are unavailable or differ from the authorized company/permit/task.");
    }
}
