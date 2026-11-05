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
    private static void RequireMappingTransitionSchema(IDataSource source)
    {
        RequireMappingConfigurationSchema(source);var schema=source.GetEntityStructure(new EntityStructure{EntityName=TableName,DatasourceEntityName=TableName},true);
        var fields=schema?.Fields?.Where(field=>field.FieldName=="CONFIGURATION_TRANSITION_JSON").ToArray();
        if(fields is not {Length:1} || fields[0].Fieldtype!=typeof(string).FullName || !fields[0].AllowDBNull || Math.Max(fields[0].Size,fields[0].Size1)<CompanyGLMappingTransitionProof.MaximumLength)throw new InvalidOperationException("Review the bounded native mapping transition receipt through the PRODUCTION Beep migration.");
    }
    public async Task<CompanyGLMappingTransitionOutcome> TransitionCompanyMappingAsync(CompanyGLMappingTransitionSubmission submission,Func<Task> requireAuthority)
    {
        ArgumentNullException.ThrowIfNull(submission);ArgumentNullException.ThrowIfNull(submission.Command);ArgumentNullException.ThrowIfNull(requireAuthority);submission=CompanyInventorySourceReview.Copy(submission);var command=submission.Command;command.Require();await requireAuthority();
        if(_resolveConnection is null || _accounts is null)throw new InvalidOperationException("The original request-bound mapping/account owner is required.");var connection=await _resolveConnection();var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("The original mapping datasource is unavailable.");
        async Task Guard(bool owned){await requireAuthority();if(await _resolveConnection()!=connection || !ReferenceEquals(_editor.GetDataSource(connection),source))throw new InvalidOperationException("The original mapping transition binding changed.");if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);RequireMappingTransitionSchema(source);}
        PPDMGenericRepository Repo(IPPDMRowAccessPolicy? policy=null)=>new(_editor,_commonColumnHandler,_defaults,_metadata,typeof(GLAccountMapping),connection,TableName,rowAccess:policy);
        async Task<GLAccountMapping?> Row(string id,bool owned)
        {
            await Guard(owned);var rows=(await Repo().GetAsync([new(){FieldName="GL_ACCOUNT_MAPPING_ID",Operator="=",FilterValue=id}]))?.ToArray();await Guard(owned);
            if(rows is null || rows.Length>1 || rows.Any(row=>row is not GLAccountMapping))throw new InvalidOperationException("The original mapping identity is unavailable or ambiguous.");var row=rows.SingleOrDefault() as GLAccountMapping;
            if(row is not null && (row.OWNER_TENANT_ID!=command.Company.TenantId || row.OWNER_COMPANY_BA_ID!=command.Company.BusinessAssociateId || row.BOOK_ID!=command.BookId || row.CURRENCY_CODE!=command.CurrencyCode))throw new InvalidOperationException("The original mapping identity belongs to another company book/currency.");return row is null?null:CompanyInventorySourceReview.Copy(row);
        }
        async Task<CompanyGLMappingTransitionOutcome?> Outcome(bool owned)
        {
            var old=await Row(command.MappingId,owned);if(old?.CONFIGURATION_TRANSITION_JSON is null)return null;var proof=CompanyGLMappingTransitionProof.Require(old);if(proof.Command!=command)throw new InvalidOperationException("Another original command owns this mapping transition.");
            var next=command.Replacement is {} replacement?await Row(replacement.MappingId,owned):null;if(next?.CONFIGURATION_TRANSITION_JSON is not null)next=CompanyInventorySourceReview.Copy(CompanyGLMappingTransitionProof.Require(next).Original);var result=new CompanyGLMappingTransitionOutcome(command,old,next);result.Require(command);await Guard(owned);return result;
        }
        CompanyGLMappingTransitionOutcome? confirmed=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Guard(true);var global=(await Repo().GetAsync([]))?.ToArray();await Guard(true);
            if(global is null || global.Length>4096 || global.Any(row=>row is not GLAccountMapping))throw new InvalidOperationException("Complete bounded mapping intent reservations are unavailable.");
            foreach(var row in global.Cast<GLAccountMapping>())
            {
                var ids=new[]{command.IntentId,command.Replacement?.IntentId}.Where(id=>id is not null).ToArray();
                if(ids.Any(id=>row.PPDM_GUID==Guid.ParseExact(id!,"N").ToString()) && (command.Replacement is null || row.GL_ACCOUNT_MAPPING_ID!=command.Replacement.MappingId))throw new InvalidOperationException("An original transition intent is reserved by another mapping creation.");
                if(row.CONFIGURATION_TRANSITION_JSON is not null){var previous=CompanyGLMappingTransitionProof.Require(row);if(ids.Contains(previous.Command.IntentId) && (row.GL_ACCOUNT_MAPPING_ID!=command.MappingId || previous.Command!=command))throw new InvalidOperationException("An original transition intent is reserved by another controlled mapping.");}
            }
            var existing=await Outcome(true);if(existing is not null){confirmed=existing;return;}
            if(submission.Review is null)throw new InvalidOperationException("A complete original mapping review is required for a new transition.");command.Require(submission.Review);
            var current=await GetCompanyMappingsAsync(command.Company,command.ActorUserId,command.BookId,command.CurrencyCode,command.ReviewDate,()=>Guard(true));command.Require(current);
            if(JsonSerializer.Serialize(current)!=JsonSerializer.Serialize(submission.Review))throw new InvalidOperationException("The complete account/mapping evidence changed before transition.");
            var original=await Row(command.MappingId,true) ??throw new InvalidOperationException("The native original mapping is unavailable.");_=CompanyGLMappingConfigurationProof.Require(original);if(original.CONFIGURATION_TRANSITION_JSON is not null)throw new InvalidOperationException("The original mapping already has a controlled transition.");
            var publicOriginal=CompanyInventorySourceReview.Copy(original);publicOriginal.CONFIGURATION_COMMAND_JSON=null;publicOriginal.CONFIGURATION_TRANSITION_JSON=null;
            if(JsonSerializer.Serialize(publicOriginal)!=JsonSerializer.Serialize(command.Require(current)))throw new InvalidOperationException("The actual mapping differs from its complete reviewed predecessor.");
            if(command.Replacement is {} nextCommand && await Row(nextCommand.MappingId,true) is not null)throw new InvalidOperationException("The replacement identity is already reserved.");
            var changed=CompanyInventorySourceReview.Copy(original);changed.EXPIRY_DATE=command.EffectiveThrough;
            await Repo(new MappingTransitionMutation(command,original,()=>Guard(true))).UpdateAsync(changed,command.ActorUserId);
            if(command.Replacement is {} next)await Repo(new MappingConfigurationMutation(next,()=>Guard(true))).InsertAsync(next.CreateRow(),command.ActorUserId);
            var stored=await Outcome(true) ??throw new InvalidOperationException("The exact native transition outcome is unavailable.");
            var closing=await GetCompanyMappingsAsync(command.Company,command.ActorUserId,command.BookId,command.CurrencyCode,command.ReviewDate,()=>Guard(true));
            var restored=CompanyGLMappingReview.Create(closing.Accounts,command.ReviewDate,closing.Records.Where(row=>command.Replacement is null || row.GL_ACCOUNT_MAPPING_ID!=command.Replacement.MappingId).Select(row=>row.GL_ACCOUNT_MAPPING_ID==command.MappingId?publicOriginal:row));
            if(JsonSerializer.Serialize(restored)!=JsonSerializer.Serialize(current))throw new InvalidOperationException("Other account/mapping facts changed during transition.");confirmed=stored;await Guard(true);
        });
        var initial=await Outcome(false);var closingOutcome=await Outcome(false);await Guard(false);
        if(initial is null || closingOutcome is null || JsonSerializer.Serialize(initial)!=JsonSerializer.Serialize(confirmed) || JsonSerializer.Serialize(initial)!=JsonSerializer.Serialize(closingOutcome))throw new InvalidOperationException("The durable mapping transition is unconfirmed. Reconcile its original request.");return CompanyInventorySourceReview.Copy(closingOutcome);
    }
    public async Task<CompanyGLMappingTransitionOutcome?> GetCompanyMappingTransitionAsync(CompanyScope company,string actor,string book,string currency,string id,Func<Task> requireAuthority)
    {
        if(!Guid.TryParseExact(id,"N",out var identity) || identity==Guid.Empty || identity.ToString("N")!=id)throw new ArgumentException("The original mapping identity is required.");_=CompanyGLAccountReview.Create(company,actor,book,currency,[]);await requireAuthority();if(_resolveConnection is null)throw new InvalidOperationException("The original mapping resolver is unavailable.");var connection=await _resolveConnection();var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("The original mapping datasource is unavailable.");
        async Task Guard(){await requireAuthority();if(await _resolveConnection()!=connection || !ReferenceEquals(_editor.GetDataSource(connection),source))throw new InvalidOperationException("The original transition binding changed.");RequireMappingTransitionSchema(source);}
        async Task<CompanyGLMappingTransitionOutcome?> Capture()
        {
            await Guard();
            var repo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(GLAccountMapping),connection,TableName);var rows=(await repo.GetAsync([new(){FieldName="GL_ACCOUNT_MAPPING_ID",Operator="=",FilterValue=id}]))?.ToArray();
            if(rows is null || rows.Length>1 || rows.Any(row=>row is not GLAccountMapping))throw new InvalidOperationException("The complete original transition is unavailable.");var row=rows.SingleOrDefault() as GLAccountMapping;if(row is null)return null;
            if(row.OWNER_TENANT_ID!=company.TenantId || row.OWNER_COMPANY_BA_ID!=company.BusinessAssociateId || row.BOOK_ID!=book || row.CURRENCY_CODE!=currency)throw new UnauthorizedAccessException();if(row.CONFIGURATION_TRANSITION_JSON is null)return null;
            var proof=CompanyGLMappingTransitionProof.Require(row);if(proof.Command.ActorUserId!=actor)throw new UnauthorizedAccessException("Only the original transition producer may recover its private command.");GLAccountMapping? next=null;
            if(proof.Command.Replacement is {} replacement){var records=(await repo.GetAsync([new(){FieldName="GL_ACCOUNT_MAPPING_ID",Operator="=",FilterValue=replacement.MappingId}]))?.ToArray();if(records is not {Length:1} || records[0] is not GLAccountMapping mapping)throw new InvalidOperationException("The exact native replacement is unavailable.");next=mapping.CONFIGURATION_TRANSITION_JSON is null?mapping:CompanyInventorySourceReview.Copy(CompanyGLMappingTransitionProof.Require(mapping).Original);}
            var result=new CompanyGLMappingTransitionOutcome(proof.Command,row,next);result.Require(proof.Command);await Guard();return CompanyInventorySourceReview.Copy(result);
        }
        var initial=await Capture();var closing=await Capture();if(JsonSerializer.Serialize(initial)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The original transition changed during recovery.");await Guard();return closing;
    }
    private sealed class MappingTransitionMutation(CompanyGLMappingTransitionCommand command,GLAccountMapping original,Func<Task> guard):IPPDMRowAccessPolicy
    {
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {await guard();if(type!=typeof(GLAccountMapping) || table!=TableName || entity is not GLAccountMapping row || previous is not GLAccountMapping before || operation!=PPDMRowMutation.Update || actor!=command.ActorUserId || JsonSerializer.Serialize(before)!=JsonSerializer.Serialize(original) || row.ROW_CHANGED_BY!=actor || row.ROW_CHANGED_DATE is not {} changed)throw new InvalidOperationException("Only the exact original mapping expiry and transition audit update are authorized.");var proof=CompanyGLMappingTransitionProof.Create(command,original,changed);row.CONFIGURATION_TRANSITION_JSON=proof.Serialize();_=CompanyGLMappingTransitionProof.Require(row);}
        public async Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)=>[await GetReadConstraintsAsync(type,table)];
        public async Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table){await guard();if(type!=typeof(GLAccountMapping) || table!=TableName)throw new InvalidOperationException("The transition grants only its exact predecessor read.");return [new(){FieldName="GL_ACCOUNT_MAPPING_ID",Operator="=",FilterValue=command.MappingId},new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=command.Company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=command.Company.BusinessAssociateId},new(){FieldName="BOOK_ID",Operator="=",FilterValue=command.BookId},new(){FieldName="CURRENCY_CODE",Operator="=",FilterValue=command.CurrencyCode}];}
        public async Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows){await guard();if(type!=typeof(GLAccountMapping) || table!=TableName || rows.Count!=1 || JsonSerializer.Serialize(rows[0])!=JsonSerializer.Serialize(original))throw new InvalidOperationException("The exact original mapping predecessor is required.");}
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException();
    }
}
