using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
using System.Data;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep.DataBase;
namespace Beep.OilandGas.Accounting.Services;

public partial class GLAccountService
{
    public async Task<CompanyGLAccountReview> GetCompanyAccountConfigurationReviewAsync(CompanyScope company,string actor,string book,string currency,Func<Task> requireAuthority)
    {
        ArgumentNullException.ThrowIfNull(requireAuthority);await requireAuthority();
        if(_resolveConnection is null)throw new InvalidOperationException("A request-bound PRODUCTION configuration owner is required.");
        var connection=await _resolveConnection();if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("The configuration binding is unavailable.");
        var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("The configuration datasource is unavailable.");
        async Task Guard(){await requireAuthority();if(await _resolveConnection()!=connection || !ReferenceEquals(_editor.GetDataSource(connection),source))throw new InvalidOperationException("The original configuration binding changed.");RequireCompanyAccountConfigurationSchema(source);}
        await Guard();var review=await GetCompanyAccountsForBindingAsync(company,actor,book,currency,Guard,connection,source);await Guard();return review;
    }
    public async Task<CompanyGLAccountConfigurationCommand?> GetCompanyAccountConfigurationAsync(CompanyScope company,string actor,string book,string currency,string accountId,Func<Task> requireAuthority)
    {
        ArgumentNullException.ThrowIfNull(requireAuthority);_=CompanyGLAccountReview.Create(company,actor,book,currency,[]);
        if(!Guid.TryParseExact(accountId,"N",out var identity) || identity==Guid.Empty || identity.ToString("N")!=accountId)throw new InvalidOperationException("The original native account identity is required.");
        await requireAuthority();if(_resolveConnection is null)throw new InvalidOperationException("A request-bound PRODUCTION configuration owner is required.");
        var connection=await _resolveConnection();if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("The original configuration binding is unavailable.");
        var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("The original configuration datasource is unavailable.");
        async Task Guard(){await requireAuthority();if(await _resolveConnection()!=connection || !ReferenceEquals(_editor.GetDataSource(connection),source))throw new InvalidOperationException("The original configuration binding changed during recovery.");}
        async Task<GL_ACCOUNT?> Capture()
        {
            await Guard();RequireCompanyAccountConfigurationSchema(source);var repo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(GL_ACCOUNT),connection,"GL_ACCOUNT");
            var rows=(await repo.GetAsync([new(){FieldName="GL_ACCOUNT_ID",Operator="=",FilterValue=accountId}]))?.ToArray();await Guard();
            if(rows is null || rows.Length>1 || rows.Any(row=>row is not GL_ACCOUNT))throw new InvalidOperationException("The original native configuration outcome is unavailable or ambiguous.");
            var row=rows.SingleOrDefault() as GL_ACCOUNT;if(row is null)return null;
            if(row.OWNER_TENANT_ID!=company.TenantId || row.OWNER_COMPANY_BA_ID!=company.BusinessAssociateId || row.BOOK_ID!=book || row.CURRENCY_CODE!=currency || row.ROW_CREATED_BY!=actor)
                throw new UnauthorizedAccessException("Only the original account configuration actor can recover this request in its original company book/currency.");
            var receipt=CompanyGLAccountConfigurationProof.Require(row);if(receipt.Command.ActorUserId!=actor || receipt.Command.AccountId!=accountId)throw new UnauthorizedAccessException();
            return CompanyInventorySourceReview.Copy(row);
        }
        var initial=await Capture();var closing=await Capture();await Guard();
        if(JsonSerializer.Serialize(initial)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The original account configuration changed during recovery.");
        return closing is null?null:CompanyInventorySourceReview.Copy(CompanyGLAccountConfigurationProof.Require(closing).Command);
    }
    private static void RequireCompanyAccountConfigurationSchema(TheTechIdea.Beep.IDataSource source)
    {
        RequireCompanyAccountSchema(source);var schema=source.GetEntityStructure(new EntityStructure{EntityName="GL_ACCOUNT",DatasourceEntityName="GL_ACCOUNT"},true);
        var proof=schema?.Fields?.Where(field=>field.FieldName=="CONFIGURATION_COMMAND_JSON").ToArray();
        if(proof is not {Length:1} || proof[0].Fieldtype!=typeof(string).FullName || !proof[0].AllowDBNull || Math.Max(proof[0].Size,proof[0].Size1)<CompanyGLAccountConfigurationProof.MaximumLength)
            throw new InvalidOperationException("Review the original account configuration receipt column through the PRODUCTION Beep migration.");
        foreach(var (name,size) in new[]{("PARENT_ACCOUNT_ID",128),("DESCRIPTION",1024)})
        {
            var fields=schema?.Fields?.Where(field=>field.FieldName==name).ToArray();
            if(fields is not {Length:1} || fields[0].Fieldtype!=typeof(string).FullName || !fields[0].AllowDBNull || Math.Max(fields[0].Size,fields[0].Size1)<size)
                throw new InvalidOperationException("The bounded optional native parent/description metadata is unconfirmed.");
        }
    }
    public async Task<GL_ACCOUNT> ConfigureCompanyAccountAsync(CompanyGLAccountConfigurationSubmission submission,Func<Task> requireAuthority)
    {
        ArgumentNullException.ThrowIfNull(submission);ArgumentNullException.ThrowIfNull(submission.Command);ArgumentNullException.ThrowIfNull(requireAuthority);
        submission=CompanyInventorySourceReview.Copy(submission);var command=submission.Command;command.Require();await requireAuthority();
        if(_resolveConnection is null)throw new InvalidOperationException("A request-bound original PRODUCTION account owner is required.");
        var connection=await _resolveConnection();if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("The account configuration binding is unavailable.");
        var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("The original account configuration datasource is unavailable.");
        async Task Guard(bool owned){await requireAuthority();if(await _resolveConnection()!=connection || !ReferenceEquals(_editor.GetDataSource(connection),source))throw new InvalidOperationException("The original configuration binding changed.");if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);}
        async Task<GL_ACCOUNT?> Read(bool owned)
        {
            await Guard(owned);RequireCompanyAccountConfigurationSchema(source);
            var repo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(GL_ACCOUNT),connection,"GL_ACCOUNT");
            var rows=(await repo.GetAsync([new(){FieldName="GL_ACCOUNT_ID",Operator="=",FilterValue=command.AccountId}]))?.ToArray();await Guard(owned);
            if(rows is null || rows.Length>1 || rows.Any(row=>row is not GL_ACCOUNT))throw new InvalidOperationException("The original account identity is unavailable or ambiguous.");
            var row=rows.SingleOrDefault() as GL_ACCOUNT;
            if(row is not null && (row.OWNER_TENANT_ID!=command.Company.TenantId || row.OWNER_COMPANY_BA_ID!=command.Company.BusinessAssociateId || row.BOOK_ID!=command.BookId || row.CURRENCY_CODE!=command.CurrencyCode))
                throw new InvalidOperationException("The account identity is reserved by another original company book/currency.");
            if(row is not null)_=CompanyGLAccountConfigurationProof.Require(row);
            return row is null?null:CompanyInventorySourceReview.Copy(row);
        }
        GL_ACCOUNT? confirmed=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Guard(true);RequireCompanyAccountConfigurationSchema(source);
            var reservationRepo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(GL_ACCOUNT),connection,"GL_ACCOUNT");
            var reservations=(await reservationRepo.GetAsync([new(){FieldName="PPDM_GUID",Operator="=",FilterValue=Guid.ParseExact(command.IntentId,"N").ToString()}]))?.ToArray();await Guard(true);
            if(reservations is null || reservations.Length>1 || reservations.Any(row=>row is not GL_ACCOUNT account || account.GL_ACCOUNT_ID!=command.AccountId))
                throw new InvalidOperationException("The original account configuration intent is unavailable, ambiguous or already reserved by another account.");
            var existing=await Read(true);
            if((reservations.Length==1)!=(existing is not null))throw new InvalidOperationException("The native account identity and original intent reservation are inconsistent.");
            if(existing is not null)
            {
                if(CompanyGLAccountConfigurationProof.Require(existing).Command!=command)throw new InvalidOperationException("Another original account configuration command owns this identity.");
                confirmed=existing;await Guard(true);return;
            }
            if(submission.Review is null)throw new InvalidOperationException("A complete original account review is required before a new configuration write.");
            command.Require(submission.Review);var current=await GetCompanyAccountsForBindingAsync(command.Company,command.ActorUserId,command.BookId,command.CurrencyCode,()=>Guard(true),connection,source);
            if(JsonSerializer.Serialize(current)!=JsonSerializer.Serialize(submission.Review))throw new InvalidOperationException("The complete original account source changed before configuration.");
            command.Require(current);var row=command.CreateRow();var writer=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(GL_ACCOUNT),connection,"GL_ACCOUNT",
                rowAccess:new CompanyAccountConfigurationMutation(command,()=>Guard(true)));
            await writer.InsertAsync(row,command.ActorUserId);var stored=await Read(true);
            if(stored is null || JsonSerializer.Serialize(stored)!=JsonSerializer.Serialize(row) || CompanyGLAccountConfigurationProof.Require(stored).Command!=command)
                throw new InvalidOperationException("The recorded account differs from the original authorized configuration.");
            var closing=await GetCompanyAccountsForBindingAsync(command.Company,command.ActorUserId,command.BookId,command.CurrencyCode,()=>Guard(true),connection,source);
            var before=CompanyGLAccountReview.Create(command.Company,command.ActorUserId,command.BookId,command.CurrencyCode,closing.Accounts.Where(account=>account.GL_ACCOUNT_ID!=command.AccountId));
            if(JsonSerializer.Serialize(before)!=JsonSerializer.Serialize(current))throw new InvalidOperationException("Other recorded accounts changed during the original configuration write.");
            confirmed=stored;await Guard(true);
        });
        var durable=await Read(false);if(durable is null || JsonSerializer.Serialize(durable)!=JsonSerializer.Serialize(confirmed) || CompanyGLAccountConfigurationProof.Require(durable).Command!=command)
            throw new InvalidOperationException("The committed account configuration is unconfirmed. Retain and reconcile the original request.");
        await Guard(false);return CompanyInventorySourceReview.Copy(durable);
    }
    private sealed class CompanyAccountConfigurationMutation(CompanyGLAccountConfigurationCommand command,Func<Task> guard):IPPDMRowAccessPolicy
    {
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {
            await guard();if(type!=typeof(GL_ACCOUNT) || table!="GL_ACCOUNT" || entity is not GL_ACCOUNT row || previous is not null || operation!=PPDMRowMutation.Insert || actor!=command.ActorUserId)
                throw new InvalidOperationException("Only the original configured account insertion is authorized.");
            row.CONFIGURATION_COMMAND_JSON=CompanyGLAccountConfigurationProof.Create(command,row);_=CompanyGLAccountConfigurationProof.Require(row);
        }
        public Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)=>throw new InvalidOperationException("An account configuration insertion cannot grant reads.");
        public Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table)=>throw new InvalidOperationException("An account configuration insertion cannot grant reads.");
        public Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)=>throw new InvalidOperationException("An account configuration insertion cannot grant reads.");
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("An account configuration insertion cannot grant reads.");
    }
}
