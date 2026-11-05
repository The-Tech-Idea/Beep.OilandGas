using System.Security.Claims;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

public sealed record DevelopmentPlanRequestScope(FieldSelectionReview Context,AssetDatabaseScope PlanningDatabase,Func<Task> CheckCurrentAsync,Func<Task> ConfirmAsync);
public interface IDevelopmentPlanRequestAuthority
{
    Task<DevelopmentPlanRequestScope> CaptureAsync(ClaimsPrincipal user,string permission,bool write,FieldSelectionReview? assertion=null,bool exactReview=false);
}

