using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
namespace Beep.OilandGas.Accounting.Services;
public partial class AccountingPolicyService
{
    public async Task<CompanyMatchingPolicyReview> GetCompanyMatchingPoliciesAsync(CompanyScope company,string actor)
    {
        PurchaseOrderOwnership.RequireScope(company);
        if(string.IsNullOrWhiteSpace(actor) || actor.Length>128 || actor.Any(char.IsControl))throw new InvalidOperationException("The current company account is required.");
        if(_resolveConnection is null)throw new InvalidOperationException("A request-bound PRODUCTION policy resolver is required.");
        var connection=await _resolveConnection();if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("The selected production binding is unavailable.");
        var source=_editor.GetDataSource(connection) ?? throw new InvalidOperationException("The policy datasource is unavailable.");
        async Task Guard(){if(await _resolveConnection()!=connection || !ReferenceEquals(_editor.GetDataSource(connection),source))throw new InvalidOperationException("The original company policy datasource changed.");}
        await Guard();
        var schema=source.GetEntityStructure(new EntityStructure{EntityName="ACCOUNTING_METHOD",DatasourceEntityName="ACCOUNTING_METHOD"},true);
        var keys=schema?.Fields?.Where(field=>field.IsKey).ToArray();
        if(keys is not {Length:1} || keys[0].FieldName!="ACCOUNTING_METHOD_ID" || keys[0].AllowDBNull)throw new InvalidOperationException("The policy identity is unavailable. Review the PRODUCTION Beep migration.");
        foreach(var (name,size) in new[]{("OWNER_TENANT_ID",128),("OWNER_COMPANY_BA_ID",128),("MATCHING_POLICY_JSON",4000),("MATCHING_POLICY_STATUS",32)})
        {
            var fields=schema?.Fields?.Where(field=>field.FieldName==name).ToArray();
            if(fields is not {Length:1} || fields[0].Fieldtype!=typeof(string).FullName || fields[0].Size<size || !fields[0].AllowDBNull)
                throw new InvalidOperationException("The company matching-policy schema is unconfirmed. Review the nullable PRODUCTION Beep columns.");
        }
        var repository=await GetRepoAsync<ACCOUNTING_METHOD>("ACCOUNTING_METHOD",connection);
        var raw=await repository.GetAsync([new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=company.TenantId},
            new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=company.BusinessAssociateId},new(){FieldName="METHOD_TYPE",Operator="=",FilterValue=CompanyMatchingPolicyReview.MethodType}]);
        await Guard();var rows=raw?.ToArray();
        if(rows is null || rows.Any(row=>row is not ACCOUNTING_METHOD))throw new InvalidOperationException("The complete policy source is unavailable.");
        return CompanyMatchingPolicyReview.Create(company,actor,rows.Cast<ACCOUNTING_METHOD>().ToArray());
    }
}
