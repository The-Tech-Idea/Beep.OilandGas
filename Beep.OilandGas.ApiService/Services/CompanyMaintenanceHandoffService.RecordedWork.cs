using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Models.Data.WorkOrder;
namespace Beep.OilandGas.ApiService.Services;
public sealed partial class CompanyMaintenanceHandoffService
{
    public async Task<CompanyMaintenanceRecordedWorkPage> ReadRecordedWorkPageAsync(ClaimsPrincipal user,CompanyMaintenanceRecordedWorkWindow window,int page,int size)
    {
        window.Require();CompanyMaintenanceRecordedWorkPage.RequirePage(page,size);if(executionPermits is null || workOrders is null)throw new InvalidOperationException("Existing native work-order/report owners are required.");
        async Task<(FieldSelectionReview Context,string[] Orders)> Capture()
        {
            var current=await selection.GetCurrentFieldSelectionAsync();var access=await company.RefreshCompanyAsync();var actor=user.ActingUserId();FieldSelectionReviewValidator.RequireCurrent(current,access.Scope,actor);
            if(user.Identity?.IsAuthenticated!=true || access.UserId!=actor || !access.Roles.Any(user.IsInRole) || !access.Permissions.Contains("Maintenance.ReturnToService",StringComparer.Ordinal) || !current.SelectionAvailable || current.FieldId!=window.FieldId || field.CurrentFieldId!=window.FieldId)throw new UnauthorizedAccessException("Current standard company role, return-read capability and original selected FIELD are required.");
            var preference=current.SelectionContext!;await preferences.RequireStoredSelectionAsync(preference);var rows=await grants.ReadAsync(actor,preference.Database.Fingerprint);
            var owned=rows.Where(row=>row.UserId==actor && row.IsActive && row.OrganizationId==access.Scope.BusinessAssociateId && row.DatabaseScope==preference.Database.Fingerprint).ToArray();var fieldRows=owned.Where(row=>row.AssetType=="FIELD" && row.AssetId==window.FieldId).ToArray();
            if(fieldRows.Length!=1 || !AssetAccessLevelChecks.Meets(fieldRows[0].AccessLevel,"READ"))throw new UnauthorizedAccessException("Current direct selected FIELD READ authority is required for recorded work.");
            var orderGrants=owned.Where(row=>row.AssetType=="WORK_ORDER").GroupBy(row=>row.AssetId,StringComparer.Ordinal).ToArray();if(orderGrants.Any(group=>group.Count()!=1))throw new InvalidOperationException("Current work-order read grants are ambiguous.");
            var direct=orderGrants.Where(group=>AssetAccessLevelChecks.Meets(group.Single().AccessLevel,"READ")).Select(group=>group.Key).ToHashSet(StringComparer.Ordinal);
            WorkOrderSummary[] headers;
            if(window.WorkOrderId is {} order)
            {
                if(!direct.Contains(order))throw new UnauthorizedAccessException("The exact original work order requires current direct READ authority.");
                var detail=await workOrders.GetByIdAsync(window.FieldId,order)??throw new InvalidOperationException("The requested recorded work order is unavailable in this FIELD.");RecordedWorkOrderComponentChecks.Require(detail);
                if(detail.InstanceId!=order || detail.FieldId!=window.FieldId || detail.Source!="WORK_ORDER")throw new InvalidOperationException("The native detail does not establish the requested original work-order identity.");
                headers=[new(){InstanceId=detail.InstanceId,FieldId=detail.FieldId,Source=detail.Source}];
            }
            else headers=(await workOrders.GetByFieldAsync(window.FieldId)).ToArray();
            if(headers.Length>1024 || headers.Any(header=>header is null || !LeaseSummaryValidator.Identifier(header.InstanceId) || header.FieldId!=window.FieldId || header.Source!="WORK_ORDER") || headers.Select(header=>header.InstanceId).Distinct(StringComparer.Ordinal).Count()!=headers.Length)throw new InvalidOperationException("Complete unique native work-order FIELD membership is required; select an exact work order for a broader scope.");
            var ids=headers.Select(header=>header.InstanceId).Where(direct.Contains).Order(StringComparer.Ordinal).ToArray();if(ids.Length>CompanyMaintenanceRecordedWorkWindow.MaximumOrders)throw new InvalidOperationException("Recorded-work source scope is too broad; select an exact work order.");
            var name=await modules.ResolveAsync("PPDM_CORE");var database=new AssetDatabaseScope(name,await modules.GetMigrationBindingFingerprintAsync(["PPDM_CORE"],name));await preferences.RequireStoredSelectionAsync(preference);
            var context=preference with{Database=database,Revision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Preference=preference,Roles=access.Roles.Order(StringComparer.Ordinal),Permissions=access.Permissions.Order(StringComparer.Ordinal),Grants=owned.OrderBy(row=>row.Id,StringComparer.Ordinal),Database=database,Orders=ids}))};return(context,ids);
        }
        var captured=await Capture();async Task Guard(){var current=await Capture();if(current.Context!=captured.Context || !current.Orders.SequenceEqual(captured.Orders,StringComparer.Ordinal))throw new UnauthorizedAccessException("Original company/account/FIELD/work-order/CORE read scope changed during report discovery.");}
        var reports=await executionPermits.ReadRecordedWorkAsync(captured.Context,window,captured.Orders,Guard);await Guard();return CompanyMaintenanceRecordedWorkPage.Create(captured.Context,window,page,size,reports,DateTime.UtcNow);
    }
}
