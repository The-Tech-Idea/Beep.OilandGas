using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using System.Security.Claims;
using Beep.OilandGas.Models.Data.Security;
namespace Beep.OilandGas.ApiService.Services;

/// <summary>Use standard roles from the current app-owned company catalog, with independently assigned accounting read permission.</summary>
public static class PurchaseOrderReadChecks
{
    public static void RequireInventoryEvidence(PurchaseOrderSourceReview source,RepositoryCompanyAccess current)
    {
        if(source.Lines.Any(line=>line.INVENTORY_LINK_REVIEW_JSON is not null || line.INVENTORY_LINK_DECISION_JSON is not null || line.INVENTORY_LINK_HISTORY_REVISION is not null) && current.Permissions?.Contains(PermissionConstants.Inventory.View,StringComparer.Ordinal)!=true)
            throw new UnauthorizedAccessException("Inventory.View is required to read retained inventory-link source evidence.");
    }
    public static bool CanRead(ClaimsPrincipal user,CompanyScope expected,RepositoryCompanyAccess? current) =>
        user.Identity?.IsAuthenticated==true && current is not null && current.Scope==expected &&
        current.Roles is not null && current.Roles.Any(user.IsInRole) &&
        current.Permissions?.Contains(PermissionConstants.Procurement.View,StringComparer.Ordinal)==true;
}
