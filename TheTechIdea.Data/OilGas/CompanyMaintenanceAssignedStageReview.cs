namespace TheTechIdea.Data.OilGas;

/// <summary>Recorded receiving audit and current configured maintenance duty, including independent review after report handoff; not a physical approval.</summary>
public sealed record CompanyMaintenanceAssignedStageReview(FieldSelectionReview Context,CompanyMaintenanceReceivingQuery Task,
    string StepInstanceId,string StepId,string StepName,string AssignedRole,string ReceivingActor,string ReceivingHistoryId,
    string ReceivingApprovalId,string DefinitionRevision,string ProcessRevision)
{
    public void Require(CompanyScope company,string actor,CompanyMaintenanceReceivingQuery task)
    {
        if(Context is null || Task is null || task is null)throw new InvalidOperationException("The recorded next-role maintenance review is unavailable.");
        FieldSelectionReviewValidator.RequireReview(Context,company,actor);Task.Require();task.Require();
        if(Task!=task || Context.FieldId!=task.FieldId || new[]{StepInstanceId,StepId,StepName,AssignedRole,ReceivingActor,ReceivingHistoryId,ReceivingApprovalId}.Any(value=>!LeaseSummaryValidator.Identifier(value)) ||
            !ProcessDefinitionRevision.IsValid(DefinitionRevision) || !ProcessDefinitionRevision.IsValid(ProcessRevision))
            throw new InvalidOperationException("The next-role maintenance review differs from its original company/account/FIELD/order/task.");
    }
}
public interface ICompanyMaintenanceAssignedStageReads
{
    Task<CompanyMaintenanceAssignedStageReview> ReviewMaintenanceAssignedStageAsync(FieldSelectionReview current,
        CompanyMaintenanceReceivingQuery task,Func<string?,Task> requireCurrentAssignedRole);
}
