using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;

namespace Beep.OilandGas.Accounting.Services;

public partial class GLAccountService
{
    private static void RequireCompanyAccountSchema(IDataSource source)
    {
        const string table="GL_ACCOUNT";
        var schema=source.GetEntityStructure(new EntityStructure{EntityName=table,DatasourceEntityName=table},true);
        var keys=schema?.Fields?.Where(field=>field.IsKey).ToArray();
        if(keys is not {Length:1} || keys[0].FieldName!="GL_ACCOUNT_ID" || keys[0].AllowDBNull || keys[0].Fieldtype!=typeof(string).FullName || Math.Max(keys[0].Size,keys[0].Size1)<128)
            throw new InvalidOperationException("Review the bounded native GL account identity through the PRODUCTION Beep migration.");
        foreach(var (name,size,nullable) in new[]{("OWNER_TENANT_ID",128,true),("OWNER_COMPANY_BA_ID",128,true),("BOOK_ID",128,true),("CURRENCY_CODE",16,true),("ACCOUNT_NUMBER",128,false),("ACCOUNT_NAME",256,false),("ACCOUNT_TYPE",32,false),("NORMAL_BALANCE",16,false)})
        {
            var fields=schema?.Fields?.Where(field=>field.FieldName==name).ToArray();
            if(fields is not {Length:1} || fields[0].Fieldtype!=typeof(string).FullName || Math.Max(fields[0].Size,fields[0].Size1)<size || fields[0].AllowDBNull!=nullable)
                throw new InvalidOperationException("The complete company/book/currency account metadata is unconfirmed.");
        }
    }
    public Task<CompanyGLAccountReview> GetCompanyAccountsAsync(CompanyScope company,string actor,string book,string currency,Func<Task> requireAuthority)=>ReadCompanyAccountsAsync(company,actor,book,currency,requireAuthority,null,null);
    internal Task<CompanyGLAccountReview> GetCompanyAccountsForBindingAsync(CompanyScope company,string actor,string book,string currency,Func<Task> requireAuthority,string connection,IDataSource source)=>ReadCompanyAccountsAsync(company,actor,book,currency,requireAuthority,connection,source);
    private async Task<CompanyGLAccountReview> ReadCompanyAccountsAsync(CompanyScope company,string actor,string book,string currency,Func<Task> requireAuthority,string? expectedConnection,IDataSource? expectedSource)
    {
        ArgumentNullException.ThrowIfNull(requireAuthority);_=CompanyGLAccountReview.Create(company,actor,book,currency,[]);await requireAuthority();
        if(_resolveConnection is null)throw new InvalidOperationException("A request-bound PRODUCTION account resolver is required.");
        var connection=await _resolveConnection();if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("The company account binding is unavailable.");
        var source=_editor.GetDataSource(connection) ?? throw new InvalidOperationException("The company account datasource is unavailable.");
        if(expectedConnection is not null && expectedConnection!=connection || expectedSource is not null && !ReferenceEquals(expectedSource,source))throw new InvalidOperationException("Account mappings require accounts from the same original native PRODUCTION binding.");
        async Task Guard(){await requireAuthority();if(await _resolveConnection()!=connection || !ReferenceEquals(_editor.GetDataSource(connection),source))throw new InvalidOperationException("The original company account binding changed.");}
        async Task<CompanyGLAccountReview> Capture()
        {
            await Guard();RequireCompanyAccountSchema(source);
            var repo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(GL_ACCOUNT),connection,"GL_ACCOUNT");
            var raw=(await repo.GetAsync([new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=company.BusinessAssociateId},
                new(){FieldName="BOOK_ID",Operator="=",FilterValue=book},new(){FieldName="CURRENCY_CODE",Operator="=",FilterValue=currency},new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}]))?.ToArray();
            await Guard();if(raw is null || raw.Length>CompanyGLAccountReview.MaximumAccounts || raw.Any(row=>row is not GL_ACCOUNT))throw new InvalidOperationException("Complete bounded company ledger accounts are unavailable.");
            var rows=CompanyGLAccountReview.Create(company,actor,book,currency,raw.Cast<GL_ACCOUNT>()).Accounts;
            foreach(var row in rows){if(row.CONFIGURATION_COMMAND_JSON is not null || row.SOURCE==CompanyGLAccountConfigurationProof.Source)_=CompanyGLAccountConfigurationProof.Require(row);row.CONFIGURATION_COMMAND_JSON=null;}
            var result=CompanyGLAccountReview.Create(company,actor,book,currency,rows);await Guard();return result;
        }
        var initial=await Capture();var closing=await Capture();
        if(initial.Revision!=closing.Revision || JsonSerializer.Serialize(initial)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The company ledger accounts changed during review.");
        await Guard();return CompanyInventorySourceReview.Copy(closing);
    }
}
