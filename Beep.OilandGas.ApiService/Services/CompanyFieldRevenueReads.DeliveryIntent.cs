using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.ProductionAccounting.Models;
using System.Security.Claims;
using System.Text.Json;
namespace Beep.OilandGas.ApiService.Services;

public sealed partial class CompanyFieldRevenueReads
{
    public async Task<DELIVERY_ACCEPTANCE_INTENT> SaveDeliveryIntentAsync(ClaimsPrincipal user,string field,RevenueDeliveryAcceptanceIntent command)
    {
        command=CompanyInventorySourceReview.Copy(command);command.RequireExecution();var source=command.Original.Source.Source.Source;
        if(command.Correction is not null)throw new ArgumentException("Save the correction through its original rejected process route.");
        if(field!=source.FieldId)throw new ArgumentException("The saved delivery route differs from its original FIELD.");
        var authority=await CommercialAuthority(user,command.Company,command.ActorUserId,field,command.Preparation is null?"ProductionAccounting.Approve":"ProductionAccounting.Adjust");
        return await production.SaveDeliveryAcceptanceIntentAsync(command,authority.Connection,authority.Guard,
            ()=>ReadDeliveryAcceptanceAsync(user,field,source.From,source.Through,command.Original.Source.Source.TransactionId));
    }
    public async Task<RevenueDeliveryAcceptanceSavedReview> ReadDeliverySavedAsync(ClaimsPrincipal user,string field,DateTime from,DateTime through,string transaction)
    {
        FieldRevenueSourceReview.RequireWindow(field,from,through);
        async Task<string> Capture()
        {
            var current=await context.CaptureAsync(user);var access=current.Access;
            if(user.Identity?.IsAuthenticated!=true || access.Roles?.Any(user.IsInRole)!=true || access.Permissions?.Contains("Accounting.View",StringComparer.Ordinal)!=true ||
                !(await assets.CheckAssetAccessAsync(current.Actor,field,"FIELD","READ")).HasAccess)throw new UnauthorizedAccessException();
            return JsonSerializer.Serialize(new{access.Scope,current.Actor,current.Binding,Roles=access.Roles.Order(StringComparer.Ordinal),Grants=access.Permissions.Order(StringComparer.Ordinal)});
        }
        var original=await context.CaptureAsync(user);var authority=await Capture();
        var expected=JsonSerializer.Serialize(new{original.Access.Scope,original.Actor,original.Binding,Roles=original.Access.Roles?.Order(StringComparer.Ordinal),Grants=original.Access.Permissions?.Order(StringComparer.Ordinal)});
        if(authority!=expected)throw new UnauthorizedAccessException("The original saved delivery company/account/binding changed before discovery.");
        var connection=await resolve();
        async Task Guard(){if(await Capture()!=authority || await resolve()!=connection)throw new UnauthorizedAccessException("The original saved delivery authority/binding changed.");}
        var source=await ReadDeliveryAcceptanceAsync(user,field,from,through,transaction);await Guard();
        source.Source.Source.Source.Require(original.Access.Scope,original.Actor,field,from,through);
        var entries=await production.GetDeliveryAcceptanceTicketIntentsAsync(original.Access.Scope,original.Actor,source.Source.TicketId,connection,Guard);
        var closing=await ReadDeliveryAcceptanceAsync(user,field,from,through,transaction);await Guard();
        var last=await production.GetDeliveryAcceptanceTicketIntentsAsync(original.Access.Scope,original.Actor,source.Source.TicketId,connection,Guard);await Guard();
        if(JsonSerializer.Serialize(source)!=JsonSerializer.Serialize(closing) || JsonSerializer.Serialize(entries)!=JsonSerializer.Serialize(last))throw new InvalidOperationException("The complete saved delivery source or journal changed during recovery.");
        var result=new RevenueDeliveryAcceptanceSavedReview(closing,last);result.Require(original.Access.Scope,original.Actor,field,from,through,transaction);return CompanyInventorySourceReview.Copy(result);
    }
}
