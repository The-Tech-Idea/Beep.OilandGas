using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.ProductionAccounting.Services;
namespace Beep.OilandGas.ApiService.Services;
public sealed partial class CompanyFieldRevenueReads(ProductionAccountingService production,ICompanyAccountingReadContext context,IAccessControlService assets,Func<Task<string>> resolve)
{
    /// <summary>Reopens the original prepared source without consulting a client's current FIELD selection.</summary>
    public Task<RevenueDeliveryAcceptanceReadReview> ReadProcessDeliveryAsync(ClaimsPrincipal user,string ticketId,string preparationRevision)
    {
        if(!ProcessDefinitionRevision.IsValid(preparationRevision))throw new ArgumentException("The original delivery preparation revision is required.");
        return ReadPreparedDeliveryCoreAsync(user,ticketId,preparationRevision);
    }
    public Task<RevenueDeliveryAcceptanceReadReview> ReadPreparedDeliveryAsync(ClaimsPrincipal user,string ticketId)
        =>ReadPreparedDeliveryCoreAsync(user,ticketId,null);
    private async Task<RevenueDeliveryAcceptanceReadReview> ReadPreparedDeliveryCoreAsync(ClaimsPrincipal user,string ticketId,string? preparationRevision)
    {
        async Task<(CompanyScope Company,string Actor,string Binding,string Roles,string Grants)> Capture(string? field=null)
        {
            var current=await context.CaptureAsync(user);var access=current.Access;
            if(user.Identity?.IsAuthenticated!=true || access.Roles?.Any(user.IsInRole)!=true || access.Permissions?.Contains("Accounting.View",StringComparer.Ordinal)!=true ||
                field is not null && !(await assets.CheckAssetAccessAsync(current.Actor,field,"FIELD","READ")).HasAccess)throw new UnauthorizedAccessException();
            return(access.Scope,current.Actor,current.Binding,JsonSerializer.Serialize(access.Roles.Order(StringComparer.Ordinal)),JsonSerializer.Serialize(access.Permissions.Order(StringComparer.Ordinal)));
        }
        var authority=await Capture();var connection=await resolve();
        var native=await production.GetRecordedDeliveryAcceptanceTicketAsync(ticketId,authority.Company,connection);
        var record=RevenueDeliveryAcceptanceProof.Require(authority.Company,native);
        preparationRevision??=CompanyAccountingSourceTypes.DeliveryPreparationRevision(record);
        var currentRevision=CompanyAccountingSourceTypes.DeliveryPreparationRevision(record);
        var source=record.Preparation.Source.Source.Source;var transaction=record.Preparation.Source.Source.TransactionId;
        async Task Guard()
        {
            if(await Capture(source.FieldId)!=authority || await resolve()!=connection)throw new UnauthorizedAccessException("Original delivery source authority changed.");
        }
        await Guard();
        var review=await ReadDeliveryAcceptanceAsync(user,source.FieldId,source.From,source.Through,transaction);
        await Guard();
        if(review.Source.TicketId!=ticketId || review.Recorded is null || CompanyAccountingSourceTypes.DeliveryPreparationRevision(review.Recorded)!=currentRevision ||
            JsonSerializer.Serialize(review.Source.Ticket)!=JsonSerializer.Serialize(native))throw new InvalidOperationException("The original delivery process source changed.");
        var closing=await production.GetRecordedDeliveryAcceptanceTicketAsync(ticketId,authority.Company,connection);
        await Guard();
        if(JsonSerializer.Serialize(closing)!=JsonSerializer.Serialize(native))throw new InvalidOperationException("The complete native delivery evidence changed during review.");
        var closingReview=await ReadDeliveryAcceptanceAsync(user,source.FieldId,source.From,source.Through,transaction);
        await Guard();
        if(JsonSerializer.Serialize(closingReview)!=JsonSerializer.Serialize(review))throw new InvalidOperationException("The complete original delivery source changed during review.");
        closingReview.Require();
        if(preparationRevision!=currentRevision)
        {
            var historical=RevenueDeliveryAcceptanceProof.HistoricalReview(authority.Company,authority.Actor,closingReview.Source,preparationRevision) with{CurrentSource=CompanyInventorySourceReview.Copy(closingReview.Source)};
            historical.Require();return CompanyInventorySourceReview.Copy(historical);
        }
        return CompanyInventorySourceReview.Copy(closingReview);
    }
    private async Task<RevenueDeliverySourceReview> CurrentDelivery(ClaimsPrincipal user,string field,DateTime from,DateTime through,string transaction)
    {var source=await GetAsync(user,field,from,through);return await GetDeliveryAsync(user,field,from,through,transaction,source.Revision);}
    public async Task<RevenueDeliveryAcceptanceReadReview> ReadDeliveryAcceptanceAsync(ClaimsPrincipal user,string field,DateTime from,DateTime through,string transaction)
    {
        production.RequireDeliveryAcceptanceSchema(await resolve());var source=await CurrentDelivery(user,field,from,through,transaction);
        var record=source.Ticket.DELIVERY_WORKFLOW_JSON is null?null:RevenueDeliveryAcceptanceProof.Require(source.Source.Source.Company,source.Ticket);
        var result=new RevenueDeliveryAcceptanceReadReview(source,record);result.Require();return result;
    }
    public async Task<RevenueDeliveryAcceptanceRecord> PrepareDeliveryAcceptanceAsync(ClaimsPrincipal user,string field,RevenueDeliveryAcceptancePreparation preparation)
    {
        preparation=CompanyInventorySourceReview.Copy(preparation);preparation.Require();preparation.Facts.RequireQuantity(preparation.Source);var source=preparation.Source.Source.Source;
        if(field!=source.FieldId)throw new ArgumentException("The delivery route differs from its original FIELD.");
        var authority=await CommercialAuthority(user,preparation.Company,preparation.ActorUserId,field,"ProductionAccounting.Adjust");
        await SaveDeliveryIntentAsync(user,field,new(preparation,null));await authority.Guard();
        return await production.PrepareDeliveryAcceptanceAsync(preparation,authority.Connection,()=>CurrentDelivery(user,field,source.From,source.Through,preparation.Source.Source.TransactionId),authority.Guard);
    }
    public async Task<RevenueDeliveryAcceptanceRecord> DecideDeliveryAcceptanceAsync(ClaimsPrincipal user,string field,RevenueDeliveryAcceptanceSubmission submission)
    {
        submission=CompanyInventorySourceReview.Copy(submission);submission.Command.RequireExecution(submission.Preparation);var source=submission.Preparation.Preparation.Source.Source.Source;
        if(field!=source.FieldId)throw new ArgumentException("The delivery route differs from its original FIELD.");
        var authority=await CommercialAuthority(user,submission.Command.Company,submission.Command.ActorUserId,field,"ProductionAccounting.Approve");
        await SaveDeliveryIntentAsync(user,field,new(null,submission));await authority.Guard();
        return await production.DecideDeliveryAcceptanceAsync(submission.Preparation,submission.Command,authority.Connection,()=>CurrentDelivery(user,field,source.From,source.Through,submission.Preparation.Preparation.Source.Source.TransactionId),authority.Guard);
    }
    public async Task<RevenueDeliverySourceReview> GetDeliveryAsync(ClaimsPrincipal user,string fieldId,DateTime from,DateTime through,string transactionId,string sourceRevision)
    {
        var contract=await GetContractAsync(user,fieldId,from,through,transactionId,sourceRevision);var connection=await resolve();
        var first=await production.GetRecordedRevenueDeliveryAsync(contract,connection);
        var closing=await GetContractAsync(user,fieldId,from,through,transactionId,sourceRevision);
        if(closing.Revision!=contract.Revision || await resolve()!=connection)throw new InvalidOperationException("Original delivery authority/source changed.");
        var final=await production.GetRecordedRevenueDeliveryAsync(closing,connection);
        var authority=await GetContractAsync(user,fieldId,from,through,transactionId,sourceRevision);
        if(authority.Revision!=contract.Revision || first.Revision!=final.Revision || await resolve()!=connection)throw new InvalidOperationException("The recorded delivery chain changed during review.");
        return final;
    }
    public async Task<RevenueSalesContractReview> GetContractAsync(ClaimsPrincipal user,string fieldId,DateTime from,DateTime through,string transactionId,string sourceRevision)
    {
        var first=await GetAsync(user,fieldId,from,through);
        if(first.Revision!=sourceRevision)throw new InvalidOperationException("Revenue source changed. Reload before reviewing its contract.");
        var row=first.Rows.SingleOrDefault(row=>row.REVENUE_TRANSACTION_ID==transactionId) ??throw new InvalidOperationException("The original revenue transaction is unavailable.");
        var connection=await resolve();
        var original=RevenueSalesContractReview.Create(first,transactionId,await production.GetRecordedSalesContractAsync(row.CONTRACT_ID,first.Company,connection));
        var closing=await GetAsync(user,fieldId,from,through);
        if(closing.Revision!=first.Revision || await resolve()!=connection)throw new InvalidOperationException("Original revenue source changed during contract review.");
        var final=RevenueSalesContractReview.Create(closing,transactionId,await production.GetRecordedSalesContractAsync(row.CONTRACT_ID,closing.Company,connection));
        var authority=await GetAsync(user,fieldId,from,through);
        if(authority.Revision!=first.Revision || final.Revision!=original.Revision || await resolve()!=connection)throw new InvalidOperationException("Original revenue/contract source changed during review.");
        return final;
    }
    public async Task<FieldRevenueSourceReview> GetAsync(ClaimsPrincipal user,string fieldId,DateTime from,DateTime through)
    {
        FieldRevenueSourceReview.RequireWindow(fieldId,from,through);
        async Task<(CompanyScope Company,string Actor,string Binding,string Roles,string Grants)> Capture()
        {
            var current=await context.CaptureAsync(user);var access=current.Access;
            if(user.Identity?.IsAuthenticated!=true || access?.Roles is null || !access.Roles.Any(user.IsInRole) || access.Permissions?.Contains("Accounting.View",StringComparer.Ordinal)!=true ||
                !(await assets.CheckAssetAccessAsync(current.Actor,fieldId,"FIELD","READ")).HasAccess)throw new UnauthorizedAccessException();
            return(access.Scope,current.Actor,current.Binding,JsonSerializer.Serialize(access.Roles.Order(StringComparer.Ordinal)),JsonSerializer.Serialize(access.Permissions.Order(StringComparer.Ordinal)));
        }
        var original=await Capture();var connection=await resolve();
        var first=FieldRevenueSourceReview.Create(original.Company,original.Actor,fieldId,from,through,original.Binding,await production.GetRecordedFieldRevenueAsync(fieldId,from,through,connection));
        if(await Capture()!=original || await resolve()!=connection)throw new UnauthorizedAccessException();
        var final=FieldRevenueSourceReview.Create(original.Company,original.Actor,fieldId,from,through,original.Binding,await production.GetRecordedFieldRevenueAsync(fieldId,from,through,connection));
        if(await Capture()!=original || await resolve()!=connection)throw new UnauthorizedAccessException();
        if(first.Revision!=final.Revision)throw new InvalidOperationException("Complete recorded revenue source changed.");return final;
    }
}
