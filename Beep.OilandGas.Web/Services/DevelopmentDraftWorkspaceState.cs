using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using Beep.OilandGas.Models.Data.Security;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;

/// <summary>The planner's reviewed form, confirmed source list and retained original save intent.</summary>
public sealed partial class DevelopmentDraftWorkspaceState : IDisposable
{
    private readonly CompanyClientContext company;
    private readonly IFieldSelectionContext fields;
    private readonly IDevelopmentServiceClient client;
    private readonly IDevelopmentDraftIntentStore journal;
    private readonly Func<Exception,string,string,string> explain;
    private long generation;
    private bool disposed,operation;
    private FIELD_DEVELOPMENT_PLAN[] rows=[];
    private string[] currencies=[];
    private CreateDevelopmentPlan? review,pending;
    private DevelopmentPlanSourceReview? sourceReview;
    private DevelopmentReviewCatalog? workflows;
    public DevelopmentReviewCatalog? Workflows=>workflows is null?null:Copy(workflows);
    public string? WorkflowError{get;private set;}
    public bool CanInspectWorkflows=>Loaded && sourceReview?.Plan is not null && company.Selection?.Permissions.Contains("Workflow.View",StringComparer.Ordinal)==true;
    public DevelopmentPlanSourceReview? SourceReview=>sourceReview is null?null:Copy(sourceReview);
    public string? RequestedPlanId{get;private set;}
    public bool NotFound=>Loaded && RequestedPlanId is not null && sourceReview?.Plan is null;
    private (long Version,CompanyClientSelection? Selection)? loadedScope;
    public FieldSelectionReview? Context{get;private set;}
    public AssetDatabaseScope? PlanningDatabase{get;private set;}
    public string? FieldName{get;private set;}
    public IReadOnlyList<FIELD_DEVELOPMENT_PLAN> Rows=>Copy(rows);
    public IReadOnlyList<string> Currencies=>currencies.ToArray();
    public CreateDevelopmentPlan? Review=>review;
    public CreateDevelopmentPlan? Pending=>pending;
    private FIELD_DEVELOPMENT_PLAN? confirmed;
    public FIELD_DEVELOPMENT_PLAN? Confirmed=>confirmed is null?null:Copy(confirmed);
    public bool Loading{get;private set;}
    public bool Busy=>operation;
    public bool Loaded{get;private set;}
    public bool NeedsCompany=>company.Selection is null;
    public bool NeedsField{get;private set;}
    public bool CanView=>company.Selection?.Permissions.Contains(PermissionConstants.DevelopmentPlanning.View,StringComparer.Ordinal)==true;
    public bool CanCreate=>company.Selection?.Permissions.Contains(PermissionConstants.DevelopmentPlanning.Create,StringComparer.Ordinal)==true;
    public bool CanEdit=>RequestedPlanId is null && Loaded && CanCreate && pending is null && !operation && !Loading;
    public bool CanRecover=>Loaded && CanCreate && pending is not null && Context is not null && PlanningDatabase==pending.PlanningDatabase &&
        Context.Company==pending.SelectionContext.Company && Context.ActorUserId==pending.SelectionContext.ActorUserId && Context.Database==pending.SelectionContext.Database && Context.FieldId==pending.FieldId;
    public string? Error{get;private set;}
    public string? Confirmation{get;private set;}
    public DateTimeOffset? CheckedAtUtc{get;private set;}
    public event Action? Changed;
    public DevelopmentDraftWorkspaceState(CompanyClientContext company,IFieldSelectionContext fields,IDevelopmentServiceClient client,
        IDevelopmentDraftIntentStore journal,Func<Exception,string,string,string> explain)
    {
        this.company=company;this.fields=fields;this.client=client;this.journal=journal;this.explain=explain;
        company.Changed+=Invalidate;fields.CurrentFieldChanged+=FieldChanged;
    }
    private static T Copy<T>(T value)=>JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value))!;
    public void Clear(){if(!disposed)Invalidate();}
    private void FieldChanged(string _)=>Invalidate();
    private void Invalidate()
    {
        ++generation;rows=[];currencies=[];sourceReview=null;RequestedPlanId=null;workflows=null;WorkflowError=null;Context=null;PlanningDatabase=null;FieldName=null;review=null;pending=null;confirmed=null;
        ResetHandoff();ResetProgress();
        Loaded=false;Loading=false;NeedsField=false;loadedScope=null;CheckedAtUtc=null;Error=null;Confirmation=null;Changed?.Invoke();
    }
    public async Task LoadAsync(string? planId=null)
    {
        if(disposed)return;
        if(operation){Invalidate();RequestedPlanId=planId;return;}
        Invalidate();RequestedPlanId=planId;var version=generation;var captured=company.Snapshot;
        if(captured.Selection is null || !CanView)return;
        bool Current()=>!disposed && version==generation && company.Snapshot==captured;
        Loading=true;Changed?.Invoke();
        try
        {
            var before=await fields.GetCurrentFieldSelectionAsync();if(!Current())return;
            FieldSelectionReviewValidator.RequireCurrent(before,captured.Selection.Scope,captured.Selection.UserId);
            if(!before.SelectionAvailable){NeedsField=true;return;}
            var context=before.SelectionContext!;
            if(planId is not null)
            {
                if(!LeaseSummaryValidator.Identifier(planId))throw new InvalidOperationException("Choose a recorded development draft from the FIELD list.");
                var detail=Copy(await client.GetDraftPlanAsync(planId));if(!Current())return;
                detail.Require(context,planId);
                await RequireCurrent(context,captured,true);if(!Current())return;
                Context=context;PlanningDatabase=detail.PlanningDatabase;FieldName=before.FieldName;sourceReview=detail;
                loadedScope=captured;CheckedAtUtc=detail.CheckedAtUtc;Loaded=true;
                if(CanCreate && company.Selection?.Permissions.Contains("Workflow.View",StringComparer.Ordinal)==true)
                {
                    try
                    {
                        var reviewRecoveryInput=await journal.ReadReviewAsync(context.Company,context.ActorUserId);if(!Current())return;
                        if(reviewRecoveryInput is not null){reviewRecoveryInput.Require();if(reviewRecoveryInput.Source.Context.Company!=context.Company || reviewRecoveryInput.Source.Context.ActorUserId!=context.ActorUserId)throw new InvalidOperationException("The retained original review belongs to another company or account.");}
                        await RequireCurrent(context,captured,true);if(!Current())return;pendingHandoff=reviewRecoveryInput;
                    }
                    catch(Exception error){var text=explain(error,"loading original development review recovery","Original review recovery input could not be checked. Reload before requesting another review");if(Current()){HandoffError=text;handoffJournalUnavailable=true;}}
                }
                return;
            }
            var list=Copy(await client.GetDraftPlansAsync());if(!Current())return;
            if(list.Context!=context || list.PlanningDatabase is null || list.Plans is null || list.CheckedAtUtc==default || list.Plans.Count>200)
                throw new InvalidOperationException("The company/FIELD draft list could not be confirmed.");
            DevelopmentPlanDraftReview.RequirePopulation(list.Plans,context);
            var options=CanCreate?await client.GetDraftOptionsAsync():null;if(!Current())return;
            if(CanCreate && options is null)throw new InvalidOperationException("Development draft options were unavailable.");
            if(options is not null && (options.Context!=context || options.PlanningDatabase!=list.PlanningDatabase || options.Currencies is null ||
                options.Currencies.Any(code=>code is not {Length:3} || code.Any(c=>c is <'A' or >'Z')) || options.Currencies.Distinct(StringComparer.Ordinal).Count()!=options.Currencies.Count))
                throw new InvalidOperationException("The draft cost-currency options changed. Reload the form.");
            var retained=await journal.ReadAsync(context.Company,context.ActorUserId);if(!Current())return;
            if(retained is not null)
            {
                DevelopmentPlanDraftReview.Capture(retained,retained.SelectionContext);
                if(retained.SelectionContext.Company!=context.Company || retained.SelectionContext.ActorUserId!=context.ActorUserId)
                    throw new InvalidOperationException("The retained original draft belongs to another company or account.");
            }
            var after=await fields.GetCurrentFieldSelectionAsync();if(!Current())return;
            FieldSelectionReviewValidator.RequireCurrent(after,captured.Selection.Scope,captured.Selection.UserId);
            if(!after.SelectionAvailable || after.SelectionContext!=context)throw new InvalidOperationException("The FIELD selection changed. Reload development drafts.");
            Context=context;PlanningDatabase=list.PlanningDatabase;FieldName=before.FieldName;rows=list.Plans.ToArray();currencies=options?.Currencies.ToArray() ?? [];
            pending=retained;loadedScope=captured;CheckedAtUtc=list.CheckedAtUtc;Loaded=true;
        }
        catch(Exception error){var text=explain(error,"loading development drafts","Development drafts could not be confirmed");if(Current())Error=text;}
        finally{if(Current()){Loading=false;Changed?.Invoke();}}
    }
    public async Task LoadReviewWorkflowsAsync()
    {
        if(disposed || operation || !CanInspectWorkflows || sourceReview is null || loadedScope is null)return;
        var original=Copy(sourceReview);var captured=loadedScope.Value;var version=generation;operation=true;WorkflowError=null;workflows=null;preparedHandoff=null;Changed?.Invoke();
        bool Current()=>!disposed && generation==version && company.Snapshot==captured;
        try
        {
            await RequireCurrent(original.Context,captured,true);
            var result=Copy(await client.GetDraftReviewWorkflowsAsync(original.PlanId,original.Revision));if(!Current())return;
            result.Require(original.Context,original.PlanId,original.Revision);
            if(result.Source.PlanningDatabase!=original.PlanningDatabase)throw new InvalidOperationException("The original planning binding changed.");
            await RequireCurrent(original.Context,captured,true);if(!Current())return;
            workflows=result;
        }
        catch(Exception error)
        {
            var text=explain(error,"loading configured development review assignments","Development review workflows could not be confirmed");
            if(Current())WorkflowError=text;
        }
        finally{operation=false;if(!disposed)Changed?.Invoke();}
    }
    public void ReviewInput(string name,string? description,DateOnly? start,DateOnly? end,decimal? cost,string? currency)
    {
        if(!CanEdit || Context is null || PlanningDatabase is null)return;
        review=null;Error=null;Confirmation=null;confirmed=null;
        try
        {
            var input=new CreateDevelopmentPlan{SelectionContext=Context,PlanningDatabase=PlanningDatabase,IntentId=Guid.NewGuid(),FieldId=Context.FieldId!,PlanName=name,
                Description=description,PlannedFirstProductionDate=start,PlannedFieldLifeEndDate=end,EstimatedCapexMillions=cost,CapexCurrency=currency};
            review=DevelopmentPlanDraftReview.Capture(input,Context);
            if(currency is not null && !currencies.Contains(currency,StringComparer.Ordinal))throw new InvalidOperationException("Choose a configured cost currency.");
        }
        catch(Exception error){review=null;Error=explain(error,"reviewing development draft inputs",error.Message);}
        Changed?.Invoke();
    }
    public void EditReview(){if(operation || pending is not null)return;review=null;Changed?.Invoke();}
    private async Task RequireCurrent(FieldSelectionReview context,(long Version,CompanyClientSelection? Selection) captured,bool exact)
    {
        if(disposed || company.Snapshot!=captured)throw new InvalidOperationException("The original company/account changed.");
        var current=await fields.GetCurrentFieldSelectionAsync();
        if(disposed || company.Snapshot!=captured)throw new InvalidOperationException("The original company/account changed.");
        FieldSelectionReviewValidator.RequireCurrent(current,context.Company,context.ActorUserId);
        if(!current.SelectionAvailable || (exact?current.SelectionContext!=context:current.SelectionContext!.Database!=context.Database || current.FieldId!=context.FieldId))
            throw new InvalidOperationException("Return to the reviewed original company/FIELD before continuing.");
    }
    public async Task SaveAsync()
    {
        if(!CanEdit || review is null || loadedScope is null)return;
        var original=review;var captured=loadedScope.Value;var version=generation;operation=true;Error=null;Confirmation=null;Changed?.Invoke();
        bool Current()=>!disposed && company.Snapshot==captured && generation==version;
        try
        {
            await RequireCurrent(original.SelectionContext,captured,true);
            if(Current()){pending=original;review=null;Changed?.Invoke();}
            await journal.RetainAsync(original);
            await RequireCurrent(original.SelectionContext,captured,true);
            var saved=await client.SaveDraftPlanAsync(original);DevelopmentPlanDraftReview.RequireRecorded(original,saved);
            await RequireCurrent(original.SelectionContext,captured,true);
            await journal.ClearAsync(original);
            if(Current()){pending=null;confirmed=Copy(saved);Confirmation="Development draft saved and confirmed from the recorded source.";rows=rows.Where(row=>row.FDP_ID!=saved.FDP_ID).Append(Copy(saved)).ToArray();}
        }
        catch(Exception error){var text=explain(error,"saving the original development draft","The draft outcome is unconfirmed. Recover the retained original before saving more work");if(Current())Error=text;}
        finally{operation=false;if(!disposed)Changed?.Invoke();}
    }
    public async Task RecoverAsync()
    {
        if(!CanRecover || operation || pending is null || loadedScope is null)return;
        var original=pending;var captured=loadedScope.Value;var version=generation;operation=true;Error=null;Confirmation=null;Changed?.Invoke();
        bool Current()=>!disposed && company.Snapshot==captured && generation==version;
        try
        {
            await RequireCurrent(original.SelectionContext,captured,false);
            var result=await client.RecoverDraftPlanAsync(original);
            if(!Current())return;
            if(result.Context!=Context || result.PlanningDatabase!=original.PlanningDatabase || result.IntentId!=original.IntentId || result.RequestRevision!=DevelopmentPlanDraftReview.RequestRevision(original) || result.CheckedAtUtc==default)
                throw new InvalidOperationException("Recovery did not confirm the original draft identity and scope.");
            if(result.Plan is not null)DevelopmentPlanDraftReview.RequireRecorded(original,result.Plan);
            await RequireCurrent(original.SelectionContext,captured,false);
            await journal.ClearAsync(original);
            if(Current())
            {
                pending=null;confirmed=result.Plan is null?null:Copy(result.Plan);review=null;
                if(result.Plan is not null)rows=rows.Where(row=>row.FDP_ID!=result.Plan.FDP_ID).Append(Copy(result.Plan)).ToArray();
                Confirmation=result.Plan is null?"The original draft was confirmed absent. You can review a new save.":"The original development draft was found and confirmed. No new draft was written.";
            }
        }
        catch(Exception error){var text=explain(error,"recovering the original development draft","The original draft outcome is still unconfirmed. Its recovery input was preserved");if(Current())Error=text;}
        finally{operation=false;if(!disposed)Changed?.Invoke();}
    }
    public void Dispose(){if(disposed)return;disposed=true;company.Changed-=Invalidate;fields.CurrentFieldChanged-=FieldChanged;Invalidate();}
}
