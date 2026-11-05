using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;
public sealed partial class DrillingProgramDraftState:IDisposable
{
    private readonly CompanyClientContext company;private readonly IPersonaContextService personas;private readonly IDrillingServiceClient client;private readonly IAfeServiceClient afes;private readonly IDevelopmentDraftIntentStore journal;private readonly Func<Exception,string,string,string> explain;
    private long generation;private bool disposed,ready;private string? requestedWell;
    private DrillingProgramReviewReadiness? routing;
    public DrillingProgramReviewReadiness? Routing=>routing is null?null:CompanyInventorySourceReview.Copy(routing);
    private DrillingProgramDraftList? drafts;private DevelopmentInvestmentQueue? funding;private DrillingProgramBasis? basis;private CreateDrillingProgramDraft? prepared,pending;private DrillingProgramDraftReceipt? receipt;
    public bool Busy{get;private set;}public string? Error{get;private set;}public string? Confirmation{get;private set;}
    public DevelopmentInvestmentQueue? Funding=>funding is null?null:CompanyInventorySourceReview.Copy(funding);
    public DrillingProgramDraftList? Drafts=>drafts is null?null:CompanyInventorySourceReview.Copy(drafts);
    public DrillingProgramBasis? Basis=>basis is null?null:CompanyInventorySourceReview.Copy(basis);
    public CreateDrillingProgramDraft? Prepared=>prepared is null?null:CompanyInventorySourceReview.Copy(prepared);
    public CreateDrillingProgramDraft? Pending=>pending is null?null:CompanyInventorySourceReview.Copy(pending);
    public DrillingProgramDraftReceipt? Receipt=>receipt is null?null:CompanyInventorySourceReview.Copy(receipt);
    public bool CanView=>company.Selection is {} current && new[]{"Drilling.View","Accounting.View","DevelopmentPlanning.View","Workflow.View"}.All(p=>current.Permissions.Contains(p,StringComparer.Ordinal));
    public bool CanCreate=>CanView && company.Selection!.Permissions.Contains("Drilling.Create",StringComparer.Ordinal);
    public bool CanInspect=>!Busy && ready && CanView && pending is null && receipt is null && reviewPending is null && reviewReceipt is null && reviewPrepared is null && funding is not null;
    public bool CanReviewRouting=>CanInspect && drafts is not null && prepared is null;
    public bool CanPrepare=>CanInspect && CanCreate && basis is not null;
    public bool CanRecover=>!Busy && ready && CanView && pending is not null;
    public event Action? Changed;
    public DrillingProgramDraftState(CompanyClientContext company,IPersonaContextService personas,IDrillingServiceClient client,IAfeServiceClient afes,IDevelopmentDraftIntentStore journal,Func<Exception,string,string,string> explain)
    {this.company=company;this.personas=personas;this.client=client;this.afes=afes;this.journal=journal;this.explain=explain;company.Changed+=Invalidate;personas.Changed+=Invalidate;}
    private void Invalidate(){++generation;reviewPrepared=null;reviewPending=null;reviewReceipt=null;routing=null;funding=null;basis=null;drafts=null;prepared=null;pending=null;receipt=null;ready=false;Error=null;Confirmation=null;if(!disposed)Changed?.Invoke();}
    public async Task LoadAsync(string? well)
    {
        if(disposed)return;Invalidate();requestedWell=well;var epoch=generation;var selected=company.Snapshot;bool Active()=>!disposed && epoch==generation && selected==company.Snapshot;
        if(!CanView)return;Busy=true;Changed?.Invoke();
        try
        {
            var original=await journal.ReadProgramAsync(selected.Selection!.Scope,selected.Selection.UserId);if(!Active())return;
            if(original is not null){original.Require();if(original.Basis.Scope().Company!=selected.Selection.Scope || original.Basis.Scope().ActorUserId!=selected.Selection.UserId)throw new InvalidOperationException("The retained program belongs to another company or account.");pending=CompanyInventorySourceReview.Copy(original);}var reviewOriginal=await journal.ReadProgramReviewAsync(selected.Selection!.Scope,selected.Selection.UserId);if(!Active())return;
            if(reviewOriginal is not null){reviewOriginal.Require();if(reviewOriginal.Scope().Company!=selected.Selection.Scope || reviewOriginal.Scope().ActorUserId!=selected.Selection.UserId)throw new InvalidOperationException("The retained program review belongs to another company or account.");reviewPending=CompanyInventorySourceReview.Copy(reviewOriginal);}ready=true;if(pending is not null || reviewPending is not null)return;
            var values=await afes.QueueAsync();values.Require(selected.Selection.Scope,selected.Selection.UserId);if(Active())funding=CompanyInventorySourceReview.Copy(values);
        }
        catch(Exception error){var text=explain(error,"loading drilling program funding and recovery input","The current company funding or retained program input could not be confirmed");if(Active())Error=text;}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    public async Task InspectAsync(string? afeId,string? well)
    {
        if(!CanInspect)return;routing=null;basis=null;drafts=null;prepared=null;Error=null;Confirmation=null;var selected=company.Snapshot;var epoch=generation;bool Active()=>!disposed && epoch==generation && selected==company.Snapshot;
        Busy=true;Changed?.Invoke();
        try
        {
            var row=funding!.Items.SingleOrDefault(item=>item.Source.Afe.AfeId==afeId)??throw new InvalidOperationException("Choose a recorded AFE from the current company queue.");var uwi=string.IsNullOrWhiteSpace(requestedWell)?well:requestedWell;
            if(string.IsNullOrWhiteSpace(uwi) || uwi!=uwi.Trim())throw new InvalidOperationException("Choose an exact recorded well UWI.");
            var listed=CanCreate?null:await client.GetProgramDraftsAsync(row.Source.ProcessId,row.Source.Afe.AfeId,uwi);var current=listed?.Basis??await client.ReviewProgramBasisAsync(row.Source.ProcessId,row.Source.Afe.AfeId,uwi);current.Require();
            if(current.Scope().Company!=selected.Selection!.Scope || current.Scope().ActorUserId!=selected.Selection.UserId || current.Well.Uwi!=uwi || current.Funding.Source.AfeId!=afeId || current.Funding.Source.Current.Source.Review.InstanceId!=row.Source.ProcessId)throw new InvalidOperationException("The program basis belongs to another original company, source or well.");listed??=await client.GetProgramDraftsAsync(row.Source.ProcessId,row.Source.Afe.AfeId,uwi);listed.Require();if(listed.Basis.Revision!=current.Revision)throw new InvalidOperationException("The well/funding source changed before program reload.");if(Active()){basis=CompanyInventorySourceReview.Copy(current);drafts=CompanyInventorySourceReview.Copy(listed);}
        }
        catch(Exception error){var text=explain(error,"reviewing approved drilling program basis","The exact approved investment and recorded well could not be confirmed");if(Active())Error=text;}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    public async Task ReviewRoutingAsync(string programId)
    {
        if(disposed || !CanReviewRouting || basis is null)return;var source=CompanyInventorySourceReview.Copy(basis);var selected=company.Snapshot;var epoch=generation;bool Active()=>!disposed && epoch==generation && selected==company.Snapshot;
        routing=null;Error=null;Busy=true;Changed?.Invoke();
        try
        {
            var row=drafts!.Drafts.SingleOrDefault(row=>row.PROGRAM_ID==programId)??throw new InvalidOperationException("Choose an exact recorded program from the current source.");
            var current=await client.ReviewProgramRoutingAsync(source.Funding.Source.Current.Source.Review.InstanceId,source.Funding.Source.AfeId,source.Well.Uwi,programId,row.PROGRAM_DRAFT_REVISION);
            current.Require(selected.Selection!.Scope,selected.Selection.UserId,programId,row.PROGRAM_DRAFT_REVISION);if(current.Options.Count!=0 && (!selected.Selection.Permissions.Contains("Drilling.Create",StringComparer.Ordinal) || !selected.Selection.Permissions.Contains("Workflow.Start",StringComparer.Ordinal)))throw new InvalidOperationException("Current view access permits recorded outcomes, without private proposed recipients.");if(current.Source.Basis.Revision!=source.Revision)throw new InvalidOperationException("The original source changed before independent routing was confirmed.");if(Active())routing=CompanyInventorySourceReview.Copy(current);
        }
        catch(Exception error){var text=explain(error,"reviewing original program and independent routing","The exact saved program and current independent recipients could not be confirmed");if(Active())Error=text;}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    public void Prepare(string? name,string? objective,string? wellType,string? profile,decimal? md,decimal? tvd,string? unit,int? duration,DateTime? start,DateTime? end)
    {
        if(!CanPrepare)return;prepared=null;Error=null;
        try
        {
            if(md is null || tvd is null || duration is null || start is null || end is null)throw new InvalidOperationException("Explicit depths, duration and planned dates are required.");
            var value=new CreateDrillingProgramDraft(CompanyInventorySourceReview.Copy(basis!),Guid.NewGuid(),name?.Trim()??"",objective?.Trim()??"",wellType??"",profile??"",md.Value,tvd.Value,unit??"",duration.Value,DateOnly.FromDateTime(start.Value),DateOnly.FromDateTime(end.Value));value.Require();prepared=value;
        }
        catch(Exception error){Error=explain(error,"preparing original drilling program","Enter every explicit program input and review the original source before saving");}Changed?.Invoke();
    }
    public void Cancel(){if(Busy || pending is not null)return;prepared=null;Changed?.Invoke();}
    public Task SaveAsync()=>Record(false);public Task RecoverAsync()=>Record(true);
    private async Task Record(bool recover)
    {
        if(disposed || (recover?!CanRecover:!CanPrepare || prepared is null))return;var value=CompanyInventorySourceReview.Copy(recover?pending!:prepared!);var selected=company.Snapshot;var epoch=generation;bool Active()=>!disposed && epoch==generation && selected==company.Snapshot;
        Busy=true;reviewReceipt=null;routing=null;Error=null;Confirmation=null;receipt=null;if(!recover){pending=value;prepared=null;}Changed?.Invoke();
        try
        {
            if(!recover){await journal.RetainProgramAsync(value);if(!Active())return;}
            var found=recover?await client.RecoverProgramDraftAsync(value):await client.RecordProgramDraftAsync(value);found.Require(value);if(!recover && found.Recorded is null)throw new InvalidOperationException("The original draft was not confirmed recorded.");if(!Active())return;
            await journal.ClearProgramAsync(value);if(!Active())return;pending=null;basis=null;prepared=null;receipt=CompanyInventorySourceReview.Copy(found);
            Confirmation=found.Recorded is null?"The original program was confirmed absent. Reload its current funding and well before preparing another draft.":"The original version-one drilling program DRAFT was confirmed recorded. Program approval, execution and actual cost remain separate.";
        }
        catch(Exception error){var text=explain(error,recover?"recovering original drilling program":"recording original drilling program","The original program outcome is unconfirmed. Its exact recovery input was preserved");if(Active())Error=text;}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    public void Dispose(){if(disposed)return;disposed=true;company.Changed-=Invalidate;personas.Changed-=Invalidate;Invalidate();}
}
