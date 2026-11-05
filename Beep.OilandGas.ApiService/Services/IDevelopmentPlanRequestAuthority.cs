using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using System.Security.Claims;

namespace Beep.OilandGas.ApiService.Services;

public sealed record DevelopmentPlanRequestScope(FieldSelectionReview Context,AssetDatabaseScope PlanningDatabase,Func<Task> CheckCurrentAsync,Func<Task> ConfirmAsync);
public interface IDevelopmentPlanRequestAuthority
{
    Task<DevelopmentPlanRequestScope> CaptureAsync(ClaimsPrincipal user,string permission,bool write,FieldSelectionReview? assertion=null,bool exactReview=false);
}

