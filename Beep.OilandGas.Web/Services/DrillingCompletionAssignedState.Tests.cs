using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public sealed partial class DrillingCompletionAssignedState
{
    private bool inspectingTest;private DrillingCompletionTestInspection? testInspection;
    public DrillingCompletionTestInspection? TestInspection=>testInspection is null?null:CompanyInventorySourceReview.Copy(testInspection);
    public bool CanInspectTest=>!Busy && current is not null && preparedDecision is null && pendingDecision is null;
    private void ResetTestInspection(){testInspection=null;inspectingTest=false;}
    public async Task InspectTestAsync(int index,string source,string type,string run,string number)
    {
        if(disposed || !CanInspectTest)return;var captured=company.Snapshot;var epoch=generation;var sequence=++request;
        bool Active()=>!disposed && generation==epoch && request==sequence && company.Snapshot==captured;
        inspectingTest=true;testInspection=null;acceptedTests.Remove(index);Error=null;Changed?.Invoke();
        try
        {
            var selection=new DrillingCompletionTestSelection(CompanyInventorySourceReview.Copy(current!),index,source.Trim(),type.Trim(),run.Trim(),number.Trim());selection.Require();
            var result=await client.InspectCompletionTestAsync(selection);result.Require(selection);if(!Active())return;testInspection=CompanyInventorySourceReview.Copy(result);
        }
        catch(Exception error){var message=explain(error,"inspecting the selected native completion test","The selected native test, original review and current FIELD authority could not be confirmed");if(Active())Error=message;}
        finally{if(Active()){inspectingTest=false;Changed?.Invoke();}}
    }
}
