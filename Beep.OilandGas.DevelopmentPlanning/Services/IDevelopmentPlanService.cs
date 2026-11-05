using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.DevelopmentPlanning.Services;

public interface IDevelopmentPlanService
{
    Task<DevelopmentPlanBaselineReview> GetBaselineAsync(DevelopmentReviewProgress review,Func<Task> requireAuthority);
    Task<DevelopmentPlanReleaseReceipt> ApproveBaselineAsync(DevelopmentPlanRelease original,Func<Task> requireAuthority,bool recover=false);
    Task<DevelopmentPlanDraftOptions> GetDraftOptionsAsync(FieldSelectionReview current, AssetDatabaseScope planning, Func<Task> requireAuthority);
    Task<DevelopmentPlanDraftList> GetDevelopmentPlansAsync(FieldSelectionReview current, AssetDatabaseScope planning, Func<Task> requireAuthority);
    Task<DevelopmentPlanSourceReview> GetDevelopmentPlanAsync(FieldSelectionReview current, AssetDatabaseScope planning, string id, Func<Task> requireAuthority);
    Task<FIELD_DEVELOPMENT_PLAN> CreateDevelopmentPlanAsync(CreateDevelopmentPlan request, Func<Task> requireAuthority);
    Task<DevelopmentPlanDraftRecovery> RecoverDevelopmentPlanAsync(CreateDevelopmentPlan original, FieldSelectionReview current, AssetDatabaseScope planning, Func<Task> requireAuthority);
}
