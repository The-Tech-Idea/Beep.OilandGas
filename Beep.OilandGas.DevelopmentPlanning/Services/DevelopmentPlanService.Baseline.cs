using System.Text.Json;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.DevelopmentPlanning.Services;

public partial class DevelopmentPlanService
{
    private static void RequireBaselineSchema(IDataSource source)
    {
        var schema=source.GetEntityStructure(new EntityStructure{EntityName=Plans,DatasourceEntityName=Plans},true)??throw new InvalidOperationException("Review FDP baseline lineage through Beep Migration.");
        foreach(var (name,size) in new[]{("SOURCE_DRAFT_FDP_ID",128),("SOURCE_REVIEW_PROCESS_ID",128),("RELEASE_INTENT_ID",36),("RELEASE_REQUEST_REVISION",64),("RELEASE_COMMAND_JSON",DevelopmentPlanRelease.MaximumBodyLength),("FDP_STATUS",32)})
        {
            var fields=schema.Fields.Where(field=>field.FieldName==name).ToArray();
            if(fields.Length!=1 || fields[0].Fieldtype!=typeof(string).FullName || Math.Max(fields[0].Size,fields[0].Size1)<size || name!="FDP_STATUS" && !fields[0].AllowDBNull)
                throw new InvalidOperationException($"Review baseline column {Plans}.{name} through Beep Migration before using approved FDP baselines.");
        }
    }
    private async Task RequireReviewedDraft(IDataSource source,string connection,DevelopmentReviewProgress review,Func<Task> guard)
    {
        var rows=await Read<FIELD_DEVELOPMENT_PLAN>(source,connection,Plans,[Filter("FDP_ID",review.Plan.FDP_ID)],guard);
        DevelopmentPlanDraftReview.RequirePopulation(rows,review.Scope.Company,review.Scope.FieldId,review.Plan.FDP_ID);
        if(rows.Length!=1 || DevelopmentPlanSourceReview.RevisionFor(review.Scope.Company,review.Scope.CoreDatabase,review.Scope.FieldId,review.Scope.PlanningDatabase,review.Plan.FDP_ID,rows[0])!=review.SourceRevision)
            throw new InvalidOperationException("The exact reviewed source draft changed or is unavailable.");
    }
    private async Task<FIELD_DEVELOPMENT_PLAN?> ReadBaseline(IDataSource source,string connection,DevelopmentReviewProgress review,Func<Task> guard)
    {
        var id=DevelopmentPlanRelease.BaselineId(review.Scope.Company,review.Plan.FDP_ID);
        var rows=await Read<FIELD_DEVELOPMENT_PLAN>(source,connection,Plans,[Filter("FDP_ID",id)],guard);
        if(rows.Length>1)throw new InvalidOperationException("The original FDP baseline identity is ambiguous.");
        var row=rows.SingleOrDefault();var result=new DevelopmentPlanBaselineReview(review,row,DateTimeOffset.UtcNow);result.Require(review);return row;
    }
    public Task<DevelopmentPlanBaselineReview> GetBaselineAsync(DevelopmentReviewProgress review,Func<Task> authority)
    {
        review.Require(review.Scope.Company,review.Scope.ActorUserId,review.InstanceId);review=Copy(review);
        return Owned(review.Scope.PlanningDatabase,authority,async(source,connection,guard)=>{
            RequireBaselineSchema(source);await RequireReviewedDraft(source,connection,review,guard);
            var first=await ReadBaseline(source,connection,review,guard);var closing=await ReadBaseline(source,connection,review,guard);
            if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The FDP baseline or its absence changed during the read.");
            await RequireReviewedDraft(source,connection,review,guard);return new DevelopmentPlanBaselineReview(review,closing,DateTimeOffset.UtcNow);
        });
    }
    public async Task<DevelopmentPlanReleaseReceipt> ApproveBaselineAsync(DevelopmentPlanRelease original,Func<Task> authority,bool recover=false)
    {
        original.Require();original=Copy(original);var review=original.Review;
        if(recover)
        {
            var found=new DevelopmentPlanReleaseReceipt(original.Revision(),await GetBaselineAsync(review,authority));found.Require(original);return found;
        }
        await Owned(review.Scope.PlanningDatabase,authority,async(source,connection,guard)=>{
            RequireBaselineSchema(source);await RequireReviewedDraft(source,connection,review,guard);var existing=await ReadBaseline(source,connection,review,guard);
            if(existing is not null)
            {
                if(DevelopmentPlanRelease.FromRecord(existing).Revision()!=original.Revision())throw new InvalidOperationException("This reviewed draft already has an approved baseline owned by another original command.");
                return true;
            }
            async Task EnsureReferences()
            {
                var statuses=await Read<R_DEVELOPMENT_PLANNING_REFERENCE_CODE>(source,connection,References,[Filter("REFERENCE_SET","FDP_STATUS"),Filter("REFERENCE_CODE","APPROVED"),Filter("ACTIVE_IND","Y")],guard);
                if(statuses.Length!=1 || statuses[0].REFERENCE_SET!="FDP_STATUS" || statuses[0].REFERENCE_CODE!="APPROVED" || statuses[0].ACTIVE_IND!="Y")throw new InvalidOperationException("The explicitly configured approved FDP status is required.");
                if(review.Plan.TOTAL_CAPEX_MM is not null && !(await Currencies(source,connection,guard)).Contains(review.Plan.CAPEX_CURRENCY,StringComparer.Ordinal))throw new InvalidOperationException("The recorded development CAPEX currency is no longer active.");
            }
            await EnsureReferences();await guard();var row=original.ToRecord(DateTime.UtcNow);
            BeepOperationResult.EnsureSuccess(source.InsertEntity(Plans,row),"Insert approved baseline",Plans);
            if(source.ErrorObject is {} write && !BeepOperationResult.IsSuccess(write))throw new InvalidOperationException("The approved FDP baseline insert is unconfirmed.");
            await guard();var saved=await ReadBaseline(source,connection,review,guard);
            if(saved is null || DevelopmentPlanRelease.FromRecord(saved).Revision()!=original.Revision())throw new InvalidOperationException("The original approved FDP baseline could not be read back.");
            await RequireReviewedDraft(source,connection,review,guard);await EnsureReferences();return true;
        });
        var receipt=new DevelopmentPlanReleaseReceipt(original.Revision(),await GetBaselineAsync(review,authority));receipt.Require(original);
        if(receipt.Result.Baseline is null)throw new InvalidOperationException("The original approved FDP baseline commit could not be confirmed. Recover its retained intent.");return receipt;
    }
}
