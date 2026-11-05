using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.ProductionAccounting.Models;
using System.Security.Claims;
using System.Text.Json;
namespace Beep.OilandGas.ApiService.Services;

public sealed partial class CompanyFieldRevenueReads
{
    private async Task<(CompanyScope Company,string Actor,string Binding,string Connection,Func<string?,Task> Guard)> DeliveryRecoveryAuthority(ClaimsPrincipal user)
    {
        var original=await context.CaptureAsync(user);var connection=await resolve();
        var expected=JsonSerializer.Serialize(new{original.Access.Scope,original.Actor,original.Binding,Roles=original.Access.Roles?.Order(StringComparer.Ordinal),Grants=original.Access.Permissions?.Order(StringComparer.Ordinal)});
        async Task Guard(string? field)
        {
            var current=await context.CaptureAsync(user);var access=current.Access;
            if(user.Identity?.IsAuthenticated!=true || access.Roles?.Any(user.IsInRole)!=true || access.Permissions?.Contains("Accounting.View",StringComparer.Ordinal)!=true ||
                JsonSerializer.Serialize(new{access.Scope,current.Actor,current.Binding,Roles=access.Roles.Order(StringComparer.Ordinal),Grants=access.Permissions.Order(StringComparer.Ordinal)})!=expected || await resolve()!=connection ||
                field is not null && !(await assets.CheckAssetAccessAsync(current.Actor,field,"FIELD","READ")).HasAccess)throw new UnauthorizedAccessException("Original company/account delivery recovery authority changed.");
        }
        await Guard(null);return(original.Access.Scope,original.Actor,original.Binding,connection,Guard);
    }
    public async Task<CompanyDeliverySavedWorkReview> ReadDeliveryIntentAsync(ClaimsPrincipal user,string intent)
    {
        CompanyDeliverySavedWorkReview.RequireIntentId(intent);var authority=await DeliveryRecoveryAuthority(user);
        var entry=await production.GetDeliveryAcceptanceIntentByIdAsync(authority.Company,authority.Actor,intent,authority.Connection,()=>authority.Guard(null)) ??throw new InvalidOperationException("The original owned delivery intent is unavailable.");
        return await ReviewDeliveryEntry(user,entry,authority.Company,authority.Actor,authority.Binding,authority.Connection,authority.Guard);
    }
    private async Task<CompanyDeliverySavedWorkReview> ReviewDeliveryEntry(ClaimsPrincipal user,DELIVERY_ACCEPTANCE_INTENT entry,CompanyScope company,string actor,string binding,string connection,Func<string?,Task> requireAuthority)
    {
        entry=CompanyInventorySourceReview.Copy(entry);var command=entry.Restore(company,actor,entry.SCOPE_KEY);var original=command.Original.Source.Source.Source;
        Task Guard()=>requireAuthority(original.FieldId);
        async Task<RevenueDeliveryAcceptanceReadReview?> Current()
        {
            await Guard();RevenueDeliveryAcceptanceReadReview? current;
            try{current=await ReadDeliveryAcceptanceAsync(user,original.FieldId,original.From,original.Through,command.Original.Source.Source.TransactionId);}
            catch(Exception unavailable) when(unavailable is InvalidOperationException or JsonException){current=null;}
            await Guard();return CompanyInventorySourceReview.Copy(current);
        }
        var first=await Current();var last=await Current();
        var closing=await production.GetDeliveryAcceptanceIntentByIdAsync(company,actor,entry.DELIVERY_ACCEPTANCE_INTENT_ID,connection,Guard);await Guard();
        if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(last) || JsonSerializer.Serialize(entry)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The complete original delivery journal/source changed during recovery review.");
        return CompanyDeliverySavedWorkReview.Create(company,actor,binding,entry,last);
    }
    public async Task<CompanyDeliverySavedWorkPage> ReadDeliverySavedPageAsync(ClaimsPrincipal user,int page,int size)
    {
        DELIVERY_ACCEPTANCE_INTENT.RequirePage(page,size);var authority=await DeliveryRecoveryAuthority(user);
        async Task<CompanyDeliverySavedWorkPage> Capture()
        {
            await authority.Guard(null);var native=await production.GetDeliveryAcceptanceIntentPageAsync(authority.Company,authority.Actor,authority.Connection,page,size,()=>authority.Guard(null));
            var rows=new List<CompanyDeliverySavedWorkReview>();
            foreach(var entry in native.Entries)rows.Add(await ReviewDeliveryEntry(user,entry,authority.Company,authority.Actor,authority.Binding,authority.Connection,authority.Guard));
            await authority.Guard(null);return CompanyDeliverySavedWorkPage.Create(authority.Company,authority.Actor,authority.Binding,rows,page,size,native.Total);
        }
        var first=await Capture();var last=await Capture();
        if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(last))throw new InvalidOperationException("The complete delivery recovery page/counts or native evidence changed.");
        return CompanyInventorySourceReview.Copy(last);
    }
}
