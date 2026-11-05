using System.Text.Json;
using Beep.OilandGas.Models.Data.WorkOrder;

namespace TheTechIdea.Data.OilGas;

/// <summary>Current assigned role and recorded order/inspection facts; not an execution-readiness decision.</summary>
public sealed record CompanyMaintenanceInspectionReview(CompanyMaintenanceAssignedStageReview Stage,AssetDatabaseScope InspectionDatabase,
    WorkOrderDetailModel Order,IReadOnlyList<InspectionCondition> Inspections,string SnapshotRevision,DateTimeOffset ReviewedAtUtc)
{
    /// <summary>Recorded business facts; reviewer identity and request authority remain independently enforced.</summary>
    public string FactsRevision()
    {
        if(Stage?.Context is null || Stage.Task is null)throw new InvalidOperationException("The complete original assigned source is unavailable.");
        Require(Stage.Context.Company,Stage.Context.ActorUserId,Stage.Task);
        return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new
        {
            Company=Stage.Context.Company,LifecycleDatabase=Stage.Context.Database,Stage.Context.FieldId,Stage.Task,
            Stage.StepInstanceId,Stage.StepId,Stage.StepName,Stage.AssignedRole,Stage.ReceivingActor,
            Stage.ReceivingHistoryId,Stage.ReceivingApprovalId,Stage.DefinitionRevision,Stage.ProcessRevision,
            InspectionDatabase,Order,Inspections=Inspections.OrderBy(row=>row.CondSeq).ToArray()
        }));
    }
    public string RequirementFactsRevision()
    {
        Require(Stage.Context.Company,Stage.Context.ActorUserId,Stage.Task);if(Inspections.Count==0 || Inspections.Any(row=>!ProcessDefinitionRevision.IsValid(row.RequirementRevision)))throw new InvalidOperationException("Complete immutable native requirement population is required.");
        var original=JsonSerializer.Serialize(new{Company=Stage.Context.Company,LifecycleDatabase=Stage.Context.Database,Stage.Context.FieldId,Stage.Task,Stage.StepInstanceId,Stage.StepId,Stage.StepName,Stage.AssignedRole,Stage.ReceivingActor,Stage.ReceivingHistoryId,Stage.ReceivingApprovalId,Stage.DefinitionRevision,Stage.ProcessRevision,InspectionDatabase,Order,Requirements=Inspections.OrderBy(row=>row.CondSeq).Select(row=>new{row.InspectionId,row.FieldId,row.WorkOrderId,row.CondSeq,row.CondType,row.RequiredText,row.Jurisdiction,row.RegulatoryRef,row.RequirementActor,row.PermitPrerequisiteKind,row.RequirementRevision}).ToArray()});return ProcessDefinitionRevision.FromJson(Inspections.Any(row=>row.ApplicableStage is not null)?JsonSerializer.Serialize(new{Original=original,Applicability=Inspections.OrderBy(row=>row.CondSeq).Select(row=>new{row.InspectionId,row.ApplicableStage}).ToArray()}):original);
    }
    public static string Revision(CompanyMaintenanceAssignedStageReview stage,AssetDatabaseScope database,WorkOrderDetailModel order,IReadOnlyList<InspectionCondition> rows)
    {
        if(stage?.Task is null || database is null || order is null || rows is null)throw new InvalidOperationException("The complete recorded inspection snapshot is unavailable.");
        WorkOrderInspectionRules.RequireProjected(stage.Task.FieldId,stage.Task.WorkOrderId,rows);
        return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Stage=stage,InspectionDatabase=database,Order=order,Inspections=rows.OrderBy(row=>row.CondSeq).ToArray()}));
    }
    public void Require(CompanyScope company,string actor,CompanyMaintenanceReceivingQuery task)
    {
        if(Stage is null || InspectionDatabase is null || Order is null || Inspections is null)throw new InvalidOperationException("The original assigned order/inspection review is unavailable.");
        Stage.Require(company,actor,task);
        if(!LeaseSummaryValidator.Identifier(InspectionDatabase.ConnectionName) || !LeaseSummaryValidator.Identifier(InspectionDatabase.Fingerprint) ||
            Order.InstanceId!=task.WorkOrderId || Order.FieldId!=task.FieldId || Order.Source!="WORK_ORDER" || Order.State!=string.Empty || string.IsNullOrWhiteSpace(Order.EvidenceUnavailableReason) ||
            ReviewedAtUtc==default || ReviewedAtUtc.Offset!=TimeSpan.Zero)
            throw new InvalidOperationException("The recorded header/database/time does not match the original maintenance task.");
        RecordedWorkOrderComponentChecks.Require(Order);WorkOrderInspectionRules.RequireProjected(task.FieldId,task.WorkOrderId,Inspections);
        if(!ProcessDefinitionRevision.IsValid(SnapshotRevision) || SnapshotRevision!=Revision(Stage,InspectionDatabase,Order,Inspections))throw new InvalidOperationException("The complete recorded order/inspection review changed.");
    }
}

/// <summary>Explicit versioned native requirement proof; existing checklist output remains historical-compatible.</summary>
public interface ICompanyWorkOrderRequirementSource
{
    Task<IReadOnlyList<InspectionCondition>> ReadRequirementProofAsync(FieldSelectionReview context,string workOrderId,Func<Task> requireAuthority);
    Task<IReadOnlyList<InspectionCondition>> ReadRequirementProofCoreAsync(FieldSelectionReview context,string workOrderId,Func<Task> requireAuthority);
}
