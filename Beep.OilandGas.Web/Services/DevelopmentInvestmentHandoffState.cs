using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.PermitsAndApplications.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Client.App;

namespace Beep.OilandGas.Web.Services;
public sealed class DevelopmentInvestmentHandoffState:IDisposable
{
    private readonly CompanyClientContext company;private readonly IPersonaContextService personas;private readonly IAfeServiceClient client;private readonly IDevelopmentDraftIntentStore journal;private readonly Func<Exception,string,string,string> explain;
    private string? processId,afeId;private long generation;private bool disposed,journalReady;
    private DevelopmentInvestmentRoutingReview? routing;private DevelopmentInvestmentHandoff? prepared,pending;private DevelopmentInvestmentHandoffReceipt? recorded;private DevelopmentInvestmentHandoffStatus? status;
    public bool Busy{get;private set;}public string? Error{get;private set;}public string? Confirmation{get;private set;}
    public DevelopmentInvestmentHandoff? Prepared=>prepared is null?null:CompanyInventorySourceReview.Copy(prepared);
    public DevelopmentInvestmentHandoff? Pending=>pending?.Review.Source.Current.Source.Review.InstanceId==processId && pending.Review.Source.AfeId==afeId?CompanyInventorySourceReview.Copy(pending):null;
    public string? RetainedProcessId=>pending?.Review.Source.Current.Source.Review.InstanceId;public string? RetainedAfeId=>pending?.Review.Source.AfeId;
    public DevelopmentInvestmentHandoffReceipt? Recorded{get=>recorded is null?null:CompanyInventorySourceReview.Copy(recorded);private set=>recorded=value;}
    public DevelopmentInvestmentHandoffStatus? Status=>status is null?null:CompanyInventorySourceReview.Copy(status);
    public bool CanView=>company.Selection is {} selected && new[]{"Accounting.View","DevelopmentPlanning.View","Workflow.View"}.All(permission=>selected.Permissions.Contains(permission,StringComparer.Ordinal));
    public bool CanPrepare=>!Busy && CanView && journalReady && pending is null && recorded?.Recorded!=true && status?.HandoffId is null && routing is {ExistingHandoffId:null} && routing.Options.Any(option=>option.Ready) &&
        company.Selection!.Permissions.Contains("Workflow.Start",StringComparer.Ordinal) && company.Selection.Permissions.Contains("Accounting.PrepareDevelopmentBudget",StringComparer.Ordinal);
    public bool CanRecover=>!Busy && CanView && Pending is not null;
    public event Action? Changed;
    public DevelopmentInvestmentHandoffState(CompanyClientContext company,IPersonaContextService personas,IAfeServiceClient client,IDevelopmentDraftIntentStore journal,Func<Exception,string,string,string> explain)
    {this.company=company;this.personas=personas;this.client=client;this.journal=journal;this.explain=explain;company.Changed+=Invalidate;personas.Changed+=Invalidate;}
    private void Invalidate(){++generation;routing=null;prepared=null;pending=null;journalReady=false;Recorded=null;status=null;Error=null;Confirmation=null;if(!disposed)Changed?.Invoke();}
    public async Task ReadStatusAsync()
    {
        if(Busy || !CanView || disposed || !LeaseSummaryValidator.Identifier(processId) || !LeaseSummaryValidator.Identifier(afeId))return;
        var epoch=generation;var selected=company.Snapshot;var process=processId!;var afe=afeId!;status=null;Error=null;Busy=true;Changed?.Invoke();bool Active()=>!disposed && epoch==generation && selected==company.Snapshot;
        try
        {
            var found=await client.ReadInvestmentHandoffAsync(process,afe);found.Require(selected.Selection!.Scope,selected.Selection.UserId,process,afe);
            if(recorded is {Recorded:true} saved && found.HandoffId!=saved.InstanceId)throw new InvalidOperationException("The recorded handoff no longer matches its acknowledged original.");
            if(Active()){status=CompanyInventorySourceReview.Copy(found);if(found.HandoffId is not null){prepared=null;routing=null;}}
        }
        catch(Exception error){var text=explain(error,"reading recorded investment handoff status","The recorded investment handoff or its absence could not be confirmed");if(Active())Error=text;}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    public async Task LoadAsync(string process,string afe)
    {
        if(disposed)return;Invalidate();processId=process;afeId=afe;var epoch=generation;var selected=company.Snapshot;bool Active()=>!disposed && epoch==generation && selected==company.Snapshot;
        if(!CanView)return;
        try
        {
            var original=await journal.ReadInvestmentHandoffAsync(selected.Selection!.Scope,selected.Selection.UserId);if(!Active())return;
            if(original is not null){original.Require();if(original.Scope().Company!=selected.Selection.Scope || original.Scope().ActorUserId!=selected.Selection.UserId)throw new InvalidOperationException("The retained investment handoff belongs to another original company or sender.");pending=CompanyInventorySourceReview.Copy(original);}journalReady=true;
        }
        catch(Exception error){var text=explain(error,"loading original investment handoff input","The retained original investment handoff could not be confirmed");if(Active())Error=text;}
        finally{if(Active())Changed?.Invoke();}
    }
    public void UseReview(DevelopmentInvestmentRoutingReview? review)
    {
        if(disposed)return;
        if(review is null){routing=null;prepared=null;return;}
        var selected=company.Selection;if(selected is null)return;
        try{review.Require(selected.Scope,selected.UserId,processId!,afeId!);if(routing?.Revision!=review.Revision)prepared=null;routing=CompanyInventorySourceReview.Copy(review);}
        catch(Exception error){routing=null;prepared=null;Error=explain(error,"checking the investment handoff source","The routing review does not match the original company or AFE source");}
    }
    public async Task PrepareAsync(string? workflow)
    {
        if(!CanPrepare)return;prepared=null;Error=null;Confirmation=null;
        if(!LeaseSummaryValidator.Identifier(workflow) || routing!.Options.SingleOrDefault(option=>option.ProcessId==workflow)?.Ready!=true){Error="Choose an eligible workflow from the current routing review.";Changed?.Invoke();return;}
        var review=CompanyInventorySourceReview.Copy(routing);var epoch=generation;var selected=company.Snapshot;Busy=true;Changed?.Invoke();
        bool Active()=>!disposed && epoch==generation && selected==company.Snapshot && routing?.Revision==review.Revision;
        try
        {
            var original=await client.PrepareInvestmentHandoffAsync(processId!,afeId!,new(review.Revision,workflow!));original.Require();
            if(original.Review.Revision!=review.Revision || original.Workflow.ProcessId!=workflow || original.Scope().Company!=selected.Selection!.Scope || original.Scope().ActorUserId!=selected.Selection.UserId)throw new InvalidOperationException("The prepared handoff differs from the current original routing source.");
            if(Active())prepared=CompanyInventorySourceReview.Copy(original);
        }
        catch(Exception error){var text=explain(error,"preparing original investment handoff","The original investment assignment could not be prepared. Inspect its routing again");if(Active())Error=text;}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    public void Cancel(){if(Busy || pending is not null)return;prepared=null;Changed?.Invoke();}
    public Task SubmitAsync()=>Record(false);public Task RecoverAsync()=>Record(true);
    private async Task Record(bool recover)
    {
        if(recover?!CanRecover:!CanPrepare || prepared is null)return;var original=CompanyInventorySourceReview.Copy(recover?pending!:prepared!);var epoch=generation;var selected=company.Snapshot;
        bool Active()=>!disposed && epoch==generation && selected==company.Snapshot;
        Busy=true;Error=null;Confirmation=null;Recorded=null;if(!recover){pending=original;prepared=null;}Changed?.Invoke();
        try
        {
            if(!recover){await journal.RetainInvestmentHandoffAsync(original);if(!Active())return;}
            var receipt=recover?await client.RecoverInvestmentHandoffAsync(original):await client.StartInvestmentHandoffAsync(original);receipt.Require(original);
            if(!recover && !receipt.Recorded)throw new InvalidOperationException("The original investment handoff was not confirmed recorded.");if(!Active())return;
            await journal.ClearInvestmentHandoffAsync(original);if(!Active())return;pending=null;prepared=null;routing=null;journalReady=true;Recorded=CompanyInventorySourceReview.Copy(receipt);
            Confirmation=receipt.Recorded?(recover?"The original investment review handoff was confirmed without another write.":"The investment review handoff was recorded and read back. Budget approval remains separate."):
                "The original handoff was confirmed absent. Inspect current routing before preparing another assignment.";
        }
        catch(Exception error){var text=explain(error,recover?"recovering original investment handoff":"recording original investment handoff","The original investment handoff is unconfirmed. Its exact recovery input was preserved");if(Active())Error=text;}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    public void Dispose(){if(disposed)return;disposed=true;company.Changed-=Invalidate;personas.Changed-=Invalidate;Invalidate();}
}
