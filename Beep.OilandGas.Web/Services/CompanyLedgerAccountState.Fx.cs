using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.Web.Services;
public sealed partial class CompanyLedgerAccountState
{
    private CompanyFxRateReview? fxRates;
    private CompanyFxRateConfigurationReview? fxConfigurationReview;
    private CompanyFxRateConfigurationSubmission? pendingFx;
    private FX_RATE? fxResult;
    public bool HasPendingFx=>pendingFx is not null;
    public CompanyFxRateReview? FxRates=>fxRates is null?null:CompanyInventorySourceReview.Copy(fxRates);
    public CompanyFxRateConfigurationReview? FxConfigurationReview=>fxConfigurationReview is null?null:CompanyInventorySourceReview.Copy(fxConfigurationReview);
    public FX_RATE? FxResult=>fxResult is null?null:CompanyInventorySourceReview.Copy(fxResult);
    private bool PendingFxMatches=>BookContext && pendingFx is {} pending && company.Selection!.Scope==pending.Command.Company && company.Selection.UserId==pending.Command.ActorUserId;
    public CompanyFxRateConfigurationSubmission? PendingFx=>PendingFxMatches?CompanyInventorySourceReview.Copy(pendingFx!):null;
    public async Task LoadFxRatesAsync(string book,string source,DateTime date,IAccountingServiceClient client,bool configure=false,DateTime? through=null)
    {
        if(configure){ConfigurationReady();if(HasPendingLedgerRequest)throw new InvalidOperationException("Reconcile the original retained ledger request first.");}else if(!BookContext || Busy)throw new UnauthorizedAccessException();var snapshot=company.Snapshot;Invalidate();var generation=version;Busy=true;Changed?.Invoke();
        try{CompanyFxRateReview loaded;if(configure){var review=through is null?await client.GetCompanyFxRateConfigurationReviewAsync(book,source,date):await client.GetCompanyFxRateConfigurationReviewAsync(book,source,date,through.Value);review.Require(snapshot.Selection!.Scope,snapshot.Selection.UserId,book,source,date);review.Source.RequireWindow(date,through??date);loaded=review.Source;if(!disposed && generation==version && snapshot==company.Snapshot)fxConfigurationReview=CompanyInventorySourceReview.Copy(review);}else{loaded=await client.GetCompanyFxRatesAsync(book,source,date);loaded.RequireWindow(date,date);}loaded.Require(snapshot.Selection!.Scope,snapshot.Selection.UserId,book,source,date);if(!disposed && generation==version && snapshot==company.Snapshot)fxRates=CompanyInventorySourceReview.Copy(loaded);}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    public async Task ConfigureFxAsync(string version,decimal rate,int precision,string direction,DateTime quote,DateTime from,DateTime through,DateTime observed,string provider,string provenance,string authority,string allocation,string reason,IAccountingServiceClient client)
    {
        ConfigurationReady();if(HasPendingLedgerRequest || fxConfigurationReview is not {} source)throw new InvalidOperationException("Review the complete original rate source first.");var review=source.Source;source.Require(company.Selection!.Scope,company.Selection.UserId,review.BookId,review.SourceCurrency,review.ReviewDate);var book=review.Books.Books.Single(row=>row.BookId==review.BookId);var command=new CompanyFxRateConfigurationCommand(Guid.NewGuid().ToString("N"),review.Books.Company,review.Books.ActorUserId,Guid.NewGuid().ToString("N"),book,review.SourceCurrency,version,rate,precision,direction,quote,from,through,observed,provider,provenance,authority,allocation,reason,review.Revision,source.BindingRevision);command.Require(review);pendingFx=new(command,CompanyInventorySourceReview.Copy(review));await SubmitFxAsync(client);
    }
    public async Task SubmitFxAsync(IAccountingServiceClient client)
    {
        ConfigurationReady();if(!PendingFxMatches)throw new InvalidOperationException("Return to the original rate producer company/account.");var pending=CompanyInventorySourceReview.Copy(pendingFx!);var snapshot=company.Snapshot;Invalidate();var generation=version;Busy=true;Changed?.Invoke();bool Current()=>!disposed && generation==version && snapshot==company.Snapshot;
        try{var row=await client.ConfigureCompanyFxRateAsync(CompanyInventorySourceReview.Copy(pending));if(CompanyFxRateProof.Require(row).Command!=pending.Command)throw new InvalidOperationException("The exact native rate outcome is unconfirmed.");if(!Current())return;await ConfirmFxAsync(row,client,Current);}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    public async Task RecoverFxAsync(string id,IAccountingServiceClient client)
    {
        if(!BookContext || Busy || HasPendingBook || HasPendingConfiguration || HasPendingMappingConfiguration || HasPendingMappingTransition || HasPendingPeriod || HasPendingPeriodTransition || HasPendingFx && (!PendingFxMatches || pendingFx!.Command.Id!=id))throw new InvalidOperationException("Recover the retained original ledger request in its own context.");var retained=pendingFx;var snapshot=company.Snapshot;Invalidate();var generation=version;Busy=true;Changed?.Invoke();bool Current()=>!disposed && generation==version && snapshot==company.Snapshot;
        try{var row=await client.GetCompanyFxRateConfigurationAsync(id) ??throw new InvalidOperationException("The original native rate configuration is unavailable.");var command=CompanyFxRateProof.Require(row).Command;if(command.Id!=id || command.Company!=snapshot.Selection!.Scope || command.ActorUserId!=snapshot.Selection.UserId || retained is not null && retained.Command!=command)throw new InvalidOperationException("The exact original rate context is unconfirmed.");if(!Current())return;pendingFx=new(CompanyInventorySourceReview.Copy(command),null);await ConfirmFxAsync(row,client,Current);}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    private async Task ConfirmFxAsync(FX_RATE row,IAccountingServiceClient client,Func<bool> current)
    {
        var command=CompanyFxRateProof.Require(row).Command;var review=await client.GetCompanyFxRatesAsync(command.Book.BookId,command.SourceCurrency,command.EffectiveFrom);review.Require(command.Company,command.ActorUserId,command.Book.BookId,command.SourceCurrency,command.EffectiveFrom);var expected=CompanyFxRateReview.Create(review.Books,review.BookId,review.SourceCurrency,review.ReviewDate,[row]).Rates.Single();if(review.Rates.SingleOrDefault(rate=>rate.Id==command.Id)!=expected)throw new InvalidOperationException("The complete public rate facts do not confirm the original native configuration.");if(!current())return;fxRates=CompanyInventorySourceReview.Copy(review);fxResult=CompanyInventorySourceReview.Copy(row);pendingFx=null;
    }
}
