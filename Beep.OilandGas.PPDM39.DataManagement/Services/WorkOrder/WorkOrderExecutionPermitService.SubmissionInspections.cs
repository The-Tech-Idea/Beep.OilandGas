using System.Text.Json;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
public sealed partial class WorkOrderExecutionPermitService
{
    public Task<CompanyWorkOrderSubmissionInspectionReview> ReviewSubmissionInspectionsAsync(FieldSelectionReview context,
        CompanyWorkOrderExecutionStartReference reference,Func<Task> authority,Func<Task<CompanyWorkOrderExecutionRequirements>> readCurrent)
        =>ReviewSubmissionInspectionsAsync(context,reference,authority,_=>readCurrent(),false);
    private async Task<CompanyWorkOrderSubmissionInspectionReview> ReviewSubmissionInspectionsAsync(FieldSelectionReview context,
        CompanyWorkOrderExecutionStartReference reference,Func<Task> authority,Func<bool,Task<CompanyWorkOrderExecutionRequirements>> readCurrent,bool owned)
    {
        reference.Require();await authority();
        async Task<CompanyWorkOrderExecutionStartRecovery> Start()
        {
            var row=await ReadStartAsync(context,reference.Permit,authority,owned)??throw new InvalidOperationException("Original recorded start is required before work submission.");
            var result=new CompanyWorkOrderExecutionStartRecovery(CompanyWorkOrderExecutionStartCommand.Read(row),row);result.Require(context.Company,context.ActorUserId,reference);return result;
        }
        var original=await Start();
        var current=CompanyInventorySourceReview.Copy(await readCurrent(owned));current.Require(context.Company,context.ActorUserId,reference.Permit);await authority();
        var closing=await readCurrent(owned);closing.Require(context.Company,context.ActorUserId,reference.Permit);
        if(current.Context!=context || closing.Context!=context)throw new UnauthorizedAccessException("Exact captured execution authority and native database binding are required for both inspection observations.");
        static string Facts(CompanyWorkOrderExecutionRequirements value)=>JsonSerializer.Serialize(new{value.Context,value.Approval,Order=CompanyWorkOrderExecutionRequirements.OrderRevision(value.Order),Requirements=value.Requirements.OrderBy(row=>row.CondSeq).ToArray(),value.OriginalRequirementPopulation,value.OriginalOrderRevision,value.Blockers});
        if(Facts(current)!=Facts(closing))throw new InvalidOperationException("Complete original current order/inspection population changed during submission inspection review.");
        var retained=await Start();
        if(JsonSerializer.Serialize(original)!=JsonSerializer.Serialize(retained))throw new InvalidOperationException("Original recorded work start changed during inspection review.");
        await authority();return CompanyWorkOrderSubmissionInspectionReview.Create(retained,closing,DateTime.UtcNow);
    }
}
