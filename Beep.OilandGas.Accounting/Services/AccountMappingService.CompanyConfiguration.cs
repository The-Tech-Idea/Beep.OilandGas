using System.Data;
using System.Text.Json;
using Beep.OilandGas.Accounting.Models;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Accounting.Services;

public partial class AccountMappingService
{
    private static void RequireMappingConfigurationSchema(IDataSource source)
    {
        RequireCompanyMappingSchema(source);var schema=source.GetEntityStructure(new EntityStructure{EntityName=TableName,DatasourceEntityName=TableName},true);
        var fields=schema?.Fields?.Where(field=>field.FieldName=="CONFIGURATION_COMMAND_JSON").ToArray();
        if(fields is not {Length:1} || fields[0].Fieldtype!=typeof(string).FullName || !fields[0].AllowDBNull || Math.Max(fields[0].Size,fields[0].Size1)<CompanyGLMappingConfigurationProof.MaximumLength)
            throw new InvalidOperationException("Review the bounded original mapping receipt column through the PRODUCTION Beep migration.");
    }
    public async Task<CompanyGLMappingReview> GetCompanyMappingConfigurationReviewAsync(CompanyScope company,string actor,string book,string currency,DateTime date,Func<Task> requireAuthority)
    {
        ArgumentNullException.ThrowIfNull(requireAuthority);await requireAuthority();
        if(_resolveConnection is null || _accounts is null)throw new InvalidOperationException("The original request-bound mapping/account owner is required.");
        var connection=await _resolveConnection();if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("The configuration binding is unavailable.");
        var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("The configuration datasource is unavailable.");
        async Task Guard(){await requireAuthority();if(await _resolveConnection()!=connection || !ReferenceEquals(_editor.GetDataSource(connection),source))throw new InvalidOperationException("The original mapping binding changed.");RequireMappingConfigurationSchema(source);}
        await Guard();var result=await GetCompanyMappingsAsync(company,actor,book,currency,date,Guard);await Guard();return result;
    }
    public async Task<GLAccountMapping> ConfigureCompanyMappingAsync(CompanyGLMappingConfigurationSubmission submission,Func<Task> requireAuthority)
    {
        ArgumentNullException.ThrowIfNull(submission);ArgumentNullException.ThrowIfNull(submission.Command);ArgumentNullException.ThrowIfNull(requireAuthority);submission=CompanyInventorySourceReview.Copy(submission);var command=submission.Command;command.Require();await requireAuthority();
        if(_resolveConnection is null || _accounts is null)throw new InvalidOperationException("The original request-bound mapping/account owner is required.");
        var connection=await _resolveConnection();if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("The original configuration binding is unavailable.");
        var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("The original mapping datasource is unavailable.");
        async Task Guard(bool owned){await requireAuthority();if(await _resolveConnection()!=connection || !ReferenceEquals(_editor.GetDataSource(connection),source))throw new InvalidOperationException("The original mapping configuration binding changed.");if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);}
        async Task<GLAccountMapping?> Read(bool owned)
        {
            await Guard(owned);RequireMappingConfigurationSchema(source);var repo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(GLAccountMapping),connection,TableName);
            var rows=(await repo.GetAsync([new(){FieldName="GL_ACCOUNT_MAPPING_ID",Operator="=",FilterValue=command.MappingId}]))?.ToArray();await Guard(owned);
            if(rows is null || rows.Length>1 || rows.Any(row=>row is not GLAccountMapping))throw new InvalidOperationException("The original mapping identity is unavailable or ambiguous.");
            var row=rows.SingleOrDefault() as GLAccountMapping;if(row is not null)
            {if(row.OWNER_TENANT_ID!=command.Company.TenantId || row.OWNER_COMPANY_BA_ID!=command.Company.BusinessAssociateId || row.BOOK_ID!=command.BookId || row.CURRENCY_CODE!=command.CurrencyCode)throw new InvalidOperationException("The mapping identity belongs to another company book/currency.");_=CompanyGLMappingConfigurationProof.Require(row);}
            return row is null?null:CompanyInventorySourceReview.Copy(row);
        }
        GLAccountMapping? confirmed=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Guard(true);RequireMappingConfigurationSchema(source);var repo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(GLAccountMapping),connection,TableName);
            var reserved=(await repo.GetAsync([new(){FieldName="PPDM_GUID",Operator="=",FilterValue=Guid.ParseExact(command.IntentId,"N").ToString()}]))?.ToArray();await Guard(true);
            if(reserved is null || reserved.Length>1 || reserved.Any(row=>row is not GLAccountMapping mapping || mapping.GL_ACCOUNT_MAPPING_ID!=command.MappingId))throw new InvalidOperationException("The original mapping intent is unavailable or reserved by another mapping.");
            var existing=await Read(true);if((reserved.Length==1)!=(existing is not null))throw new InvalidOperationException("The native mapping identity and original intent reservation are inconsistent.");
            if(existing is not null){if(CompanyGLMappingConfigurationProof.Require(existing).Command!=command)throw new InvalidOperationException("Another original command owns this mapping.");confirmed=existing;await Guard(true);return;}
            if(submission.Review is null)throw new InvalidOperationException("A complete original mapping review is required before a new insertion.");command.Require(submission.Review);
            var current=await GetCompanyMappingsAsync(command.Company,command.ActorUserId,command.BookId,command.CurrencyCode,command.ReviewDate,()=>Guard(true));command.Require(current);
            if(JsonSerializer.Serialize(current)!=JsonSerializer.Serialize(submission.Review))throw new InvalidOperationException("The complete original account/mapping source changed before configuration.");
            var row=command.CreateRow();var writer=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(GLAccountMapping),connection,TableName,rowAccess:new MappingConfigurationMutation(command,()=>Guard(true)));
            await writer.InsertAsync(row,command.ActorUserId);var stored=await Read(true);
            if(stored is null || JsonSerializer.Serialize(stored)!=JsonSerializer.Serialize(row) || CompanyGLMappingConfigurationProof.Require(stored).Command!=command)throw new InvalidOperationException("The native mapping differs from the original authorized configuration.");
            var closing=await GetCompanyMappingsAsync(command.Company,command.ActorUserId,command.BookId,command.CurrencyCode,command.ReviewDate,()=>Guard(true));
            var before=CompanyGLMappingReview.Create(closing.Accounts,command.ReviewDate,closing.Records.Where(mapping=>mapping.GL_ACCOUNT_MAPPING_ID!=command.MappingId));
            if(JsonSerializer.Serialize(before)!=JsonSerializer.Serialize(current))throw new InvalidOperationException("Other account/mapping facts changed during configuration.");confirmed=stored;await Guard(true);
        });
        var durable=await Read(false);if(durable is null || JsonSerializer.Serialize(durable)!=JsonSerializer.Serialize(confirmed) || CompanyGLMappingConfigurationProof.Require(durable).Command!=command)throw new InvalidOperationException("The committed mapping outcome is unconfirmed. Retain and reconcile the original request.");
        await Guard(false);return CompanyInventorySourceReview.Copy(durable);
    }
    public async Task<CompanyGLMappingConfigurationCommand?> GetCompanyMappingConfigurationAsync(CompanyScope company,string actor,string book,string currency,string id,Func<Task> requireAuthority)
    {
        ArgumentNullException.ThrowIfNull(requireAuthority);_=CompanyGLAccountReview.Create(company,actor,book,currency,[]);
        if(!Guid.TryParseExact(id,"N",out var identity) || identity==Guid.Empty || identity.ToString("N")!=id)throw new InvalidOperationException("The original mapping identity is required.");
        await requireAuthority();if(_resolveConnection is null)throw new InvalidOperationException("The original request-bound mapping owner is required.");
        var connection=await _resolveConnection();if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("The original configuration binding is unavailable.");
        var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("The original configuration datasource is unavailable.");
        async Task Guard(){await requireAuthority();if(await _resolveConnection()!=connection || !ReferenceEquals(_editor.GetDataSource(connection),source))throw new InvalidOperationException("The original configuration binding changed.");RequireMappingConfigurationSchema(source);}
        async Task<GLAccountMapping?> Capture()
        {
            await Guard();var repo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(GLAccountMapping),connection,TableName);
            var rows=(await repo.GetAsync([new(){FieldName="GL_ACCOUNT_MAPPING_ID",Operator="=",FilterValue=id}]))?.ToArray();await Guard();
            if(rows is null || rows.Length>1 || rows.Any(row=>row is not GLAccountMapping))throw new InvalidOperationException("The original mapping outcome is unavailable or ambiguous.");
            var row=rows.SingleOrDefault() as GLAccountMapping;if(row is null)return null;
            if(row.OWNER_TENANT_ID!=company.TenantId || row.OWNER_COMPANY_BA_ID!=company.BusinessAssociateId || row.BOOK_ID!=book || row.CURRENCY_CODE!=currency || row.ROW_CREATED_BY!=actor)throw new UnauthorizedAccessException("Only the original mapping producer can recover this request.");
            var receipt=CompanyGLMappingConfigurationProof.Require(row);if(receipt.Command.ActorUserId!=actor || receipt.Command.MappingId!=id)throw new UnauthorizedAccessException();return CompanyInventorySourceReview.Copy(row);
        }
        var initial=await Capture();var closing=await Capture();await Guard();if(JsonSerializer.Serialize(initial)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The original configured mapping changed during recovery.");return closing is null?null:CompanyInventorySourceReview.Copy(CompanyGLMappingConfigurationProof.Require(closing).Command);
    }
    private sealed class MappingConfigurationMutation(CompanyGLMappingConfigurationCommand command,Func<Task> guard):IPPDMRowAccessPolicy
    {
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {await guard();if(type!=typeof(GLAccountMapping) || table!=TableName || entity is not GLAccountMapping row || previous is not null || operation!=PPDMRowMutation.Insert || actor!=command.ActorUserId)throw new InvalidOperationException("Only the exact original mapping insertion is authorized.");row.CONFIGURATION_COMMAND_JSON=CompanyGLMappingConfigurationProof.Create(command,row);_=CompanyGLMappingConfigurationProof.Require(row);}
        public Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)=>throw new InvalidOperationException("Mapping insertion cannot grant reads.");
        public Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table)=>throw new InvalidOperationException("Mapping insertion cannot grant reads.");
        public Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)=>throw new InvalidOperationException("Mapping insertion cannot grant reads.");
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("Mapping insertion cannot grant reads.");
    }
}
