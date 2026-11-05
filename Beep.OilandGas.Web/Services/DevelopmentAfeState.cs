using System.Text.Json;
using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;

public sealed class DevelopmentAfeState:IDisposable
{
    private readonly CompanyClientContext company;private readonly IPersonaContextService personas;private readonly IAfeServiceClient client;
    private readonly IDevelopmentDraftIntentStore journal;private readonly Func<Exception,string,string,string> explain;
    private string? processId;private DevelopmentAfeReview? review;private DevelopmentAfeCommand? prepared,pending;private DevelopmentAfeFacts? recorded;
    private long generation;private bool disposed,journalReady;
    public bool Busy{get;private set;}public string? Error{get;private set;}public string? Confirmation{get;private set;}
    public DevelopmentAfeReview? Review=>review is null?null:Copy(review);
    public DevelopmentAfeCommand? Prepared=>prepared is null?null:Copy(prepared);
    public DevelopmentAfeCommand? Pending=>pending?.Review.Source.Review.InstanceId==processId?Copy(pending):null;
    public string? RetainedProcessId=>pending?.Review.Source.Review.InstanceId;
    public DevelopmentAfeFacts? Recorded=>recorded;
    public bool CanView=>company.Selection is {} selected && new[]{"Accounting.View","DevelopmentPlanning.View","Workflow.View"}.All(permission=>selected.Permissions.Contains(permission,StringComparer.Ordinal));
    public bool CanPrepare=>!Busy && CanView && journalReady && pending is null && review is not null && company.Selection!.Permissions.Contains("Accounting.PrepareDevelopmentBudget",StringComparer.Ordinal);
    public bool CanRecover=>!Busy && CanView && Pending is not null && pending!.Review.Source.Review.Scope.Company==company.Selection!.Scope && pending.Review.Source.Review.Scope.ActorUserId==company.Selection.UserId;
    public event Action? Changed;
    private static T Copy<T>(T value)=>JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value))!;
    public DevelopmentAfeState(CompanyClientContext company,IPersonaContextService personas,IAfeServiceClient client,IDevelopmentDraftIntentStore journal,Func<Exception,string,string,string> explain)
    {this.company=company;this.personas=personas;this.client=client;this.journal=journal;this.explain=explain;company.Changed+=Invalidate;personas.Changed+=Invalidate;}
    private void Invalidate(){++generation;review=null;prepared=null;pending=null;recorded=null;journalReady=false;Error=null;Confirmation=null;Changed?.Invoke();}
    public async Task LoadAsync(string? sourceProcessId)
    {
        if(disposed)return;Invalidate();processId=sourceProcessId;var epoch=generation;var selected=company.Snapshot;
        bool Active()=>!disposed && epoch==generation && selected==company.Snapshot;
        if(selected.Selection is null || !CanView || Busy)return;Busy=true;Changed?.Invoke();
        try
        {
            var original=await journal.ReadAfeAsync(selected.Selection.Scope,selected.Selection.UserId);if(!Active())return;
            if(original is not null){original.Require();RequireScope(original.Review,selected.Selection.Scope,selected.Selection.UserId,original.Review.Source.Review.InstanceId);pending=Copy(original);}
            journalReady=true;if(string.IsNullOrWhiteSpace(processId) || pending is not null)return;
            var found=await client.ReviewDevelopmentAsync(processId);RequireScope(found,selected.Selection.Scope,selected.Selection.UserId,processId);if(Active())review=Copy(found);
        }
        catch(Exception error){var text=explain(error,"opening approved FDP AFE workspace","The approved source, company books or retained original request could not be confirmed");if(Active())Error=text;}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    private static void RequireScope(DevelopmentAfeReview found,CompanyScope selected,string actor,string process)
    {found.Require();if(found.Source.Review.Scope.Company!=selected || found.Source.Review.Scope.ActorUserId!=actor || found.Source.Review.InstanceId!=process)throw new InvalidOperationException("The financial workspace belongs to another original account, company or approved FDP.");}
    public void Prepare(string? book,string number,string name,decimal? amount,DateTime? effectiveDate,string rationale)
    {
        if(!CanPrepare)return;prepared=null;Error=null;Confirmation=null;
        try
        {
            if(amount is null || effectiveDate is null)throw new InvalidOperationException("Enter an explicit request amount and effective calendar date.");
            var original=new DevelopmentAfeCommand(Copy(review!),Guid.NewGuid(),book??"",number.Trim(),name.Trim(),amount.Value,DateOnly.FromDateTime(effectiveDate.Value),rationale.Trim());original.Require();prepared=original;
        }
        catch(Exception error){Error=explain(error,"reviewing development AFE request","Choose a configured book and complete the amount at its recorded precision, calendar date, number, name and rationale");}Changed?.Invoke();
    }
    public void Cancel(){if(Busy || pending is not null)return;prepared=null;Changed?.Invoke();}
    public Task SubmitAsync()=>RecordAsync(false);public Task RecoverAsync()=>RecordAsync(true);
    private async Task RecordAsync(bool recover)
    {
        if(recover?!CanRecover:!CanPrepare || prepared is null)return;
        var original=Copy(recover?pending!:prepared!);var epoch=generation;var selected=company.Snapshot;Busy=true;Error=null;Confirmation=null;recorded=null;review=null;
        if(!recover){pending=original;prepared=null;}Changed?.Invoke();
        bool Active()=>!disposed && epoch==generation && selected==company.Snapshot;
        try
        {
            if(!recover){await journal.RetainAfeAsync(original);if(!Active())return;}
            var receipt=recover?await client.RecoverDevelopmentAsync(original):await client.RecordDevelopmentAsync(original);receipt.Require(original);
            if(!recover && receipt.Recorded is null)throw new InvalidOperationException("The original AFE request was not confirmed recorded.");if(!Active())return;
            await journal.ClearAfeAsync(original);if(!Active())return;pending=null;prepared=null;journalReady=true;
            recorded=receipt.Recorded is null?null:DevelopmentAfeCommand.Facts(receipt.Recorded);
            Confirmation=recorded is not null?(recover?"The original DRAFT AFE request was confirmed without another write.":"The DRAFT AFE request was recorded and confirmed. Independent investment approval remains separate."):
                "The original request was confirmed absent. Reload the source and books before preparing another request.";
        }
        catch(Exception error){var text=explain(error,recover?"recovering original development AFE":"recording development AFE","The original AFE outcome is unconfirmed. Its exact recovery input was preserved");if(Active())Error=text;}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    public void Dispose(){if(disposed)return;disposed=true;company.Changed-=Invalidate;personas.Changed-=Invalidate;Invalidate();}
}
