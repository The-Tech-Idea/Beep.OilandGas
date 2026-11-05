namespace TheTechIdea.Data.OilGas;
public sealed record CompanyWorkOrderMaterialAllocationReferenceQuery(CompanyMaintenanceReceivingQuery Task,Guid IntentId)
{
    public void Require(){if(Task is null || IntentId==Guid.Empty)throw new ArgumentException("Retain the original reservation task and intent reference.");Task.Require();}
}
public sealed record CompanyWorkOrderMaterialAllocationRecovery(CompanyWorkOrderMaterialAllocationCommand Command,WORK_ORDER_MATERIAL_ALLOCATION Recorded)
{
    public void Require(CompanyScope company,string actor,CompanyWorkOrderMaterialAllocationReferenceQuery reference)
    {
        reference.Require();if(Command is null || Recorded is null)throw new InvalidOperationException("Complete original reservation recovery is unavailable.");Command.Require();var source=Command.Review.Source;
        if(source.Context.Company!=company || source.Context.ActorUserId!=actor || source.Query.Task!=reference.Task || Command.IntentId!=reference.IntentId)throw new InvalidOperationException("Original private reservation owner/task/reference changed.");Command.RequireRecorded(Recorded);
    }
}