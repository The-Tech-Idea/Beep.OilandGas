using System.Text.Json;
using Beep.OilandGas.Accounting.Models;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Accounting.Services;

public partial class AccountMappingService
{
    private static void RequireCompanyMappingSchema(IDataSource source)
    {
        var schema=source.GetEntityStructure(new EntityStructure{EntityName=TableName,DatasourceEntityName=TableName},true);
        if(schema?.EntityName!=TableName || schema.Fields is null)throw new InvalidOperationException("The original native GL_ACCOUNT_MAPPING table is unconfirmed.");
        var keys=schema.Fields.Where(field=>field.IsKey).ToArray();
        if(keys is not {Length:1} || keys[0].FieldName!="GL_ACCOUNT_MAPPING_ID" || keys[0].AllowDBNull || keys[0].Fieldtype!=typeof(string).FullName || Math.Max(keys[0].Size,keys[0].Size1)<128)
            throw new InvalidOperationException("Review the bounded original mapping identity through the PRODUCTION Beep migration.");
        foreach(var (name,size,nullable) in new[]{("OWNER_TENANT_ID",128,true),("OWNER_COMPANY_BA_ID",128,true),("BOOK_ID",128,true),("CURRENCY_CODE",16,true),("GL_ACCOUNT_ID",128,true),("MAPPING_KEY",128,false),("GL_ACCOUNT_NUMBER",128,false)})
        {
            var fields=schema.Fields.Where(field=>field.FieldName==name).ToArray();
            if(fields is not {Length:1} || fields[0].Fieldtype!=typeof(string).FullName || Math.Max(fields[0].Size,fields[0].Size1)<size || fields[0].AllowDBNull!=nullable)
                throw new InvalidOperationException("The complete company/book/currency and native mapping account metadata is unconfirmed.");
        }
        foreach(var name in new[]{"EFFECTIVE_DATE","EXPIRY_DATE"})
        {
            var fields=schema.Fields.Where(field=>field.FieldName==name).ToArray();
            if(fields is not {Length:1} || fields[0].Fieldtype!=typeof(DateTime).FullName || !fields[0].AllowDBNull)
                throw new InvalidOperationException("The original nullable company calendar mapping date metadata is unconfirmed.");
        }
    }
    public async Task<CompanyGLMappingReview> GetCompanyMappingsAsync(CompanyScope company,string actor,string book,string currency,DateTime date,Func<Task> requireAuthority)
    {
        ArgumentNullException.ThrowIfNull(requireAuthority);_=CompanyGLAccountReview.Create(company,actor,book,currency,[]);CompanyGLMappingReview.RequireDate(date);await requireAuthority();
        if(_resolveConnection is null || _accounts is null)throw new InvalidOperationException("A request-bound original PRODUCTION mapping/account owner is required.");
        var connection=await _resolveConnection();if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("The company mapping binding is unavailable.");
        var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("The company mapping datasource is unavailable.");
        async Task Guard(){await requireAuthority();if(await _resolveConnection()!=connection || !ReferenceEquals(_editor.GetDataSource(connection),source))throw new InvalidOperationException("The original company mapping binding changed.");}
        async Task<CompanyGLMappingReview> Capture()
        {
            await Guard();RequireCompanyMappingSchema(source);
            var accounts=await _accounts.GetCompanyAccountsForBindingAsync(company,actor,book,currency,Guard,connection,source);await Guard();
            var repo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(GLAccountMapping),connection,TableName);
            var raw=(await repo.GetAsync([new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=company.BusinessAssociateId},
                new(){FieldName="BOOK_ID",Operator="=",FilterValue=book},new(){FieldName="CURRENCY_CODE",Operator="=",FilterValue=currency},new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}]))?.ToArray();
            await Guard();if(raw is null || raw.Length>CompanyGLMappingReview.MaximumMappings || raw.Any(row=>row is not GLAccountMapping))throw new InvalidOperationException("The complete company mapping history is unavailable.");
            var closingAccounts=await _accounts.GetCompanyAccountsForBindingAsync(company,actor,book,currency,Guard,connection,source);await Guard();
            if(accounts.Revision!=closingAccounts.Revision || JsonSerializer.Serialize(accounts)!=JsonSerializer.Serialize(closingAccounts))throw new InvalidOperationException("The original account set changed while reading its mappings.");
            var validated=CompanyGLMappingReview.Create(closingAccounts,date,raw.Cast<GLAccountMapping>());
            foreach(var row in validated.Records){if(row.CONFIGURATION_TRANSITION_JSON is not null)_=CompanyGLMappingTransitionProof.Require(row);if(row.CONFIGURATION_COMMAND_JSON is not null || row.SOURCE==CompanyGLMappingConfigurationProof.Source)_=CompanyGLMappingConfigurationProof.Require(row);row.CONFIGURATION_COMMAND_JSON=null;row.CONFIGURATION_TRANSITION_JSON=null;}
            var result=CompanyGLMappingReview.Create(closingAccounts,date,validated.Records);await Guard();return result;
        }
        var initial=await Capture();var closing=await Capture();await Guard();
        if(initial.Revision!=closing.Revision || JsonSerializer.Serialize(initial)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The complete account/mapping evidence changed during review.");
        return CompanyInventorySourceReview.Copy(closing);
    }
}
