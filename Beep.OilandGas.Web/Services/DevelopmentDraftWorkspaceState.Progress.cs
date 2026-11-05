using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;

public sealed partial class DevelopmentDraftWorkspaceState
{
    private DevelopmentPlanReviewStatus? reviewStatus;
    public DevelopmentPlanReviewStatus? ReviewStatus=>reviewStatus is null?null:Copy(reviewStatus);
    public string? ReviewStatusError{get;private set;}
    private void ResetProgress(){reviewStatus=null;ReviewStatusError=null;}
    public async Task InspectReviewStatusAsync()
    {
        if(!CanInspectWorkflows || operation || sourceReview is null || loadedScope is null)return;
        var source=Copy(sourceReview);var selected=loadedScope.Value;var epoch=generation;operation=true;ResetProgress();Changed?.Invoke();
        bool Active()=>!disposed && epoch==generation && company.Snapshot==selected;
        try
        {
            await RequireCurrent(source.Context,selected,false);if(!Active())return;
            var result=await client.GetDraftReviewStatusAsync(source.PlanId,source.Revision);result.Require(source);
            await RequireCurrent(source.Context,selected,false);if(Active()){reviewStatus=Copy(result);if(result.Review is not null)preparedHandoff=null;}
        }
        catch(Exception error){var text=explain(error,"inspecting recorded development review","The recorded review outcome could not be confirmed");if(Active())ReviewStatusError=text;}
        finally{operation=false;if(!disposed)Changed?.Invoke();}
    }
}
