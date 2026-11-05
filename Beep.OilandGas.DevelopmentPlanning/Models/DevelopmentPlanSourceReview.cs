using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;

namespace Beep.OilandGas.DevelopmentPlanning.Models;

/// <summary>The exact immutable draft reviewed by a current company reader. This grants no approval.</summary>
public sealed record DevelopmentPlanSourceReview(
    [property: JsonRequired] FieldSelectionReview Context,
    [property: JsonRequired] AssetDatabaseScope PlanningDatabase,
    [property: JsonRequired] string PlanId,
    [property: JsonRequired] FIELD_DEVELOPMENT_PLAN? Plan,
    [property: JsonRequired] string Revision,
    [property: JsonRequired] DateTimeOffset CheckedAtUtc)
{
    public static string RevisionFor(FieldSelectionReview context,AssetDatabaseScope planning,string id,FIELD_DEVELOPMENT_PLAN? plan)=>
        RevisionFor(context.Company,context.Database,context.FieldId!,planning,id,plan);
    public static string RevisionFor(CompanyScope company,AssetDatabaseScope core,string fieldId,AssetDatabaseScope planning,string id,FIELD_DEVELOPMENT_PLAN? plan)=>
        ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Company=company,CoreDatabase=core,FieldId=fieldId,PlanningDatabase=planning,PlanId=id,Plan=plan}));

    public void Require(FieldSelectionReview current,string id)
    {
        FieldSelectionReviewValidator.RequireReview(current,current.Company,current.ActorUserId);
        if(Context!=current || current.FieldId is null || !LeaseSummaryValidator.Identifier(id) || PlanId!=id ||
            PlanningDatabase is null || !LeaseSummaryValidator.Identifier(PlanningDatabase.ConnectionName) || !LeaseSummaryValidator.Identifier(PlanningDatabase.Fingerprint) ||
            CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero || !ProcessDefinitionRevision.IsValid(Revision))
            throw new InvalidOperationException("The original company/FIELD development draft review is unavailable.");
        if(Plan is not null)DevelopmentPlanDraftReview.RequirePopulation([Plan],current,id);
        if(Revision!=RevisionFor(current,PlanningDatabase,id,Plan))
            throw new InvalidOperationException("The recorded development draft changed. Reload its source before continuing.");
    }
}
