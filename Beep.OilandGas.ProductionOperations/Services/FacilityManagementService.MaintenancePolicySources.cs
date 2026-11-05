using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Report;
namespace Beep.OilandGas.ProductionOperations.Services;

public sealed partial class FacilityManagementService
{
    public Task<IReadOnlyList<CompanyMaintenanceReturnNativeSource>> ReadMaintenanceReturnNativeSourcesAsync(FieldSelectionReview context,WorkOrderDetailModel order,CompanyMaintenanceQualifiedAssets evidence,CompanyMaintenanceReturnPolicyCatalog catalog,Func<Task> authority) =>
        ReadMaintenanceReturnNativeSourcesAsync(context,order,evidence,catalog,authority,false);

    internal async Task<IReadOnlyList<CompanyMaintenanceReturnNativeSource>> ReadMaintenanceReturnNativeSourcesAsync(FieldSelectionReview context,WorkOrderDetailModel order,CompanyMaintenanceQualifiedAssets evidence,CompanyMaintenanceReturnPolicyCatalog catalog,Func<Task> authority,bool owned)
    {
        evidence.Require(order);catalog.Require();if(evidence.Context!=context || catalog.Context.Company!=context.Company || catalog.Context.Database!=context.Database || catalog.Context.ActorUserId!=context.ActorUserId || context.Database.ConnectionName!=_connectionName || _maintenanceAssetAccess is null)throw new UnauthorizedAccessException("Original qualified native sources, policy/company/account/CORE and parent READ authority are required.");
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("Original native policy source catalog is unavailable.");
        async Task Guard(){RequireMaintenanceEvidenceLane(source,owned);await authority();RequireMaintenanceEvidenceLane(source,owned);if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)) || CompanyWorkOrderExecutionRequirements.OrderRevision(order)!=evidence.OrderRevision)throw new InvalidOperationException("Original policy source/component scope changed.");}
        async Task Parents(){await Guard();var current=await ReadMaintenanceQualifiedAssetsAsync(context,order,authority,owned);if(current.IdentityRevision()!=evidence.IdentityRevision())throw new InvalidOperationException("Current native parent ownership/installation/READ authority changed during policy source review.");}
        async Task<CompanyMaintenanceReturnNativeSource[]> Observe()
        {
            await Parents();var keys=CompanyMaintenanceQualifiedReturnPolicy.RequiredNativeSources(evidence,order,catalog,DateTime.UtcNow);if(keys.Length>1024)throw new InvalidOperationException("Complete bounded native policy source population is unavailable.");
            await MaintenanceAssetDefinitionAsync(source,Guard,typeof(R_SOURCE),["SOURCE"]);var rows=new List<CompanyMaintenanceReturnNativeSource>();
            foreach(var key in keys)
            {
                var native=await MaintenanceAssetRowsCoreAsync<R_SOURCE>(Guard,["SOURCE","ACTIVE_IND"],owned,new AppFilter{FieldName="SOURCE",Operator="=",FilterValue=key});if(native.Length>1)throw new InvalidOperationException("Native policy source identity is ambiguous.");
                if(native.Length==1){var row=native[0];var confirmed=true;DateTime? Utc(DateTime? value){try{return MaintenanceIdentityUtc(value);}catch(InvalidOperationException){confirmed=false;return null;}}var from=Utc(row.EFFECTIVE_DATE);var to=Utc(row.EXPIRY_DATE);rows.Add(new(row.SOURCE,row.ACTIVE_IND,from,to,confirmed,MaintenanceIdentityRevision(row)));}
            }
            await MaintenanceAssetDefinitionAsync(source,Guard,typeof(R_SOURCE),["SOURCE"]);await Parents();await Guard();return rows.OrderBy(row=>row.Source,StringComparer.Ordinal).ToArray();
        }
        var first=await Observe();var closing=await Observe();if(System.Text.Json.JsonSerializer.Serialize(first)!=System.Text.Json.JsonSerializer.Serialize(closing))throw new InvalidOperationException("Complete native policy source references changed during review.");return closing;
    }
}
