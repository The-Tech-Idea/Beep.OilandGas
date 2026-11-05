using System.Security.Claims;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Services;
public sealed partial class CompanyFieldRevenueReads
{
    public async Task<DELIVERY_ACCEPTANCE_INTENT> SaveDeliveryCorrectionIntentAsync(ClaimsPrincipal user,CompanyDeliveryCorrectionSubmission command,Func<Task> requireLifecycleAuthority)
    {
        command=CompanyInventorySourceReview.Copy(command);command.Require();var original=new RevenueDeliveryAcceptanceIntent(null,null){Correction=command};var source=command.Correction.Preparation.Source.Source.Source;
        var authority=await CommercialAuthority(user,command.Review.Company,command.Review.ActorUserId,source.FieldId,"ProductionAccounting.Adjust");
        async Task Guard(){await authority.Guard();await requireLifecycleAuthority();await authority.Guard();}
        return await production.SaveDeliveryAcceptanceIntentAsync(original,authority.Connection,Guard,()=>ReadDeliveryAcceptanceAsync(user,source.FieldId,source.From,source.Through,command.Correction.Preparation.Source.Source.TransactionId));
    }
    public async Task<RevenueDeliveryAcceptanceRecord> CorrectDeliveryAcceptanceAsync(ClaimsPrincipal user,CompanyDeliveryCorrectionSubmission command,Func<Task> requireLifecycleAuthority)
    {
        command=CompanyInventorySourceReview.Copy(command);command.Require();var source=command.Correction.Preparation.Source.Source.Source;
        var authority=await CommercialAuthority(user,command.Review.Company,command.Review.ActorUserId,source.FieldId,"ProductionAccounting.Adjust");
        async Task Guard(){await authority.Guard();await requireLifecycleAuthority();await authority.Guard();}
        return await production.CorrectDeliveryAcceptanceAsync(command.Correction,authority.Connection,()=>CurrentDelivery(user,source.FieldId,source.From,source.Through,command.Correction.Preparation.Source.Source.TransactionId),Guard);
    }
}
