using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DevelopmentPlanning.Models;
using System.Data;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
namespace Beep.OilandGas.ProductionAccounting.Services;
public partial class AfeService
{
    public async Task<IReadOnlyList<DevelopmentAfeIndexItem>> ReadDevelopmentIndexAsync(CompanyScope company,AssetDatabaseScope database,IReadOnlyList<string> fields,Func<Task> authority)
    {
        PurchaseOrderOwnership.RequireScope(company);if(database is null || !LeaseSummaryValidator.Identifier(database.ConnectionName) || !LeaseSummaryValidator.Identifier(database.Fingerprint) || fields is null || fields.Count>128 || fields.Any(field=>!LeaseSummaryValidator.Identifier(field)) || fields.Distinct(StringComparer.Ordinal).Count()!=fields.Count)throw new InvalidOperationException("The current bounded exact FIELD/company request index scope is required.");
        fields=fields.ToArray();var source=_editor.GetDataSource(database.ConnectionName)??throw new InvalidOperationException("The current company AFE datasource is unavailable.");
        async Task Guard(bool owned)
        {
            await authority();if(!ReferenceEquals(source,_editor.GetDataSource(database.ConnectionName)))throw new InvalidOperationException("The native AFE index binding changed.");MigrationConnectionTarget.Validate(_editor,source,database.ConnectionName);
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);RequireOwnedBudgetSchema(source);
        }
        async Task<DevelopmentAfeIndexItem[]> Read()
        {
            var result=new List<DevelopmentAfeIndexItem>();foreach(var field in fields.Order(StringComparer.Ordinal))
            {
                var rows=await ReadDevelopmentRows<AFE>(source,database.ConnectionName,"AFE",[AfeFilter("OWNER_TENANT_ID",company.TenantId),AfeFilter("OWNER_COMPANY_BA_ID",company.BusinessAssociateId),AfeFilter("FIELD_ID",field)],()=>Guard(true));
                var development=rows.Where(row=>row.DEVELOPMENT_BUDGET_JSON is not null || row.FDP_BASELINE_ID is not null || row.SOURCE=="COMPANY_DEVELOPMENT_BUDGET").ToArray();
                if(development.Length>DevelopmentInvestmentQueue.MaximumItems || result.Count+development.Length>DevelopmentInvestmentQueue.MaximumItems)throw new InvalidOperationException("The complete company investment queue exceeds its bounded source population.");
                foreach(var row in development)
                {
                    if(row.OWNER_TENANT_ID!=company.TenantId || row.OWNER_COMPANY_BA_ID!=company.BusinessAssociateId || row.FIELD_ID!=field || row.SOURCE!="COMPANY_DEVELOPMENT_BUDGET")throw new UnauthorizedAccessException("AFE index ownership and exact current FIELD close before private proof.");
                    var command=DevelopmentAfeCommand.FromRecord(row);var original=command.Review;
                    if(original.BudgetDatabase!=database || original.Source.Review.Scope.Company!=company || original.Source.Review.Scope.FieldId!=field)throw new InvalidOperationException("The AFE index original bindings or source ownership changed.");
                    var item=new DevelopmentAfeIndexItem(DevelopmentAfeCommand.Facts(row),original.Source.Review.InstanceId,field,original.Source.Baseline!.FDP_NAME,original.Source.Baseline.FDP_VERSION);item.Require();result.Add(item);
                }
            }
            if(result.Select(row=>row.Afe.AfeId).Distinct(StringComparer.Ordinal).Count()!=result.Count)throw new InvalidOperationException("The native development request index is ambiguous.");return result.OrderBy(row=>row.Afe.AfeId,StringComparer.Ordinal).ToArray();
        }
        await Guard(false);DevelopmentAfeIndexItem[] result=[];await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>{await Guard(true);var first=await Read();result=await Read();if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(result))throw new InvalidOperationException("The complete development request index changed during observation.");await Guard(true);});await Guard(false);return CompanyInventorySourceReview.Copy(result);
    }
}
