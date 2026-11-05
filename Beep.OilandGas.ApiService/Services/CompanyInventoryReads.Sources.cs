using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Services;
public sealed partial class CompanyInventoryReads(InventoryService inventory,ICompanyAccountingReadContext context)
{
    private async Task<(CompanyScope Company,string Actor,string Binding,string Roles,string Permissions)> Capture(ClaimsPrincipal user,bool receiving=false,bool preparing=false,bool deciding=false,bool cycling=false,PurchaseReceiptStockMovementKind? stockKind=null)
    {
        var current=await context.CaptureAsync(user);var access=current.Access;
        if(user.Identity?.IsAuthenticated!=true || access is null || access.Roles is null || !access.Roles.Any(user.IsInRole) ||
            access.Permissions?.Contains(Beep.OilandGas.Models.Data.Security.PermissionConstants.Inventory.View,StringComparer.Ordinal)!=true ||
            receiving && access.Permissions?.Contains("Procurement.View",StringComparer.Ordinal)!=true ||
            preparing && access.Permissions?.Contains(Beep.OilandGas.Models.Data.Security.PermissionConstants.Procurement.PrepareInventoryLink,StringComparer.Ordinal)!=true ||
            deciding && access.Permissions?.Contains(Beep.OilandGas.Models.Data.Security.PermissionConstants.Procurement.RecordInventoryLinkDecision,StringComparer.Ordinal)!=true ||
            cycling && access.Permissions?.Contains(Beep.OilandGas.Models.Data.Security.PermissionConstants.Procurement.ReopenInventoryLink,StringComparer.Ordinal)!=true ||
            stockKind is not null && access.Permissions?.Contains(stockKind==PurchaseReceiptStockMovementKind.Receive?
                Beep.OilandGas.Models.Data.Security.PermissionConstants.Inventory.RecordReceiptStock:
                Beep.OilandGas.Models.Data.Security.PermissionConstants.Inventory.RecordSupplierReturn,StringComparer.Ordinal)!=true ||
            string.IsNullOrWhiteSpace(current.Actor) || string.IsNullOrWhiteSpace(current.Binding))
            throw new UnauthorizedAccessException("Current app-owned company roles and Inventory.View are required.");
        PurchaseOrderOwnership.RequireScope(access.Scope);
        return (access.Scope,current.Actor,current.Binding,JsonSerializer.Serialize(access.Roles.Order(StringComparer.Ordinal)),JsonSerializer.Serialize(access.Permissions.Order(StringComparer.Ordinal)));
    }
    public async Task<CompanyInventorySourceReview?> GetSourceAsync(ClaimsPrincipal user,string itemId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId);
        var original=await Capture(user);
        var initial=await inventory.GetCompanyInventorySourceAsync(original.Company,itemId,original.Actor);
        var closing=await inventory.GetCompanyInventorySourceAsync(original.Company,itemId,original.Actor);
        if(await Capture(user)!=original || initial?.Revision!=closing?.Revision)
            throw new InvalidOperationException("Inventory evidence, company authority or PRODUCTION binding changed.");
        closing?.Require(original.Company,original.Actor,itemId);return closing;
    }
    public async Task<List<INVENTORY_ITEM>> GetQueueAsync(ClaimsPrincipal user)
    {
        var original=await Capture(user);
        var initial=await inventory.GetCompanyInventoryItemsAsync(original.Company);
        var closing=await inventory.GetCompanyInventoryItemsAsync(original.Company);
        static string Digest(List<INVENTORY_ITEM> rows)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(rows.OrderBy(row=>row.INVENTORY_ITEM_ID,StringComparer.Ordinal)));
        if(await Capture(user)!=original || Digest(initial)!=Digest(closing))
            throw new InvalidOperationException("The complete inventory queue or original company authority changed.");
        CompanyInventorySourceReview.RequireRows(original.Company,closing);return closing;
    }
}
