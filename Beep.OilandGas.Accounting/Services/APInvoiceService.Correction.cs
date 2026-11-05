using System.Data;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Accounting.Services;
public partial class APInvoiceService
{
    public async Task<APInvoiceCorrectionResult> RecordCorrectionAsync(APInvoiceCorrectionCommand command,APInvoiceCorrectionReview displayed,
        Func<Task> requireAuthority,Func<Task<APInvoiceFinancialReview>> readFinancial)
    {
        ArgumentNullException.ThrowIfNull(command);ArgumentNullException.ThrowIfNull(displayed);ArgumentNullException.ThrowIfNull(requireAuthority);ArgumentNullException.ThrowIfNull(readFinancial);
        command=PurchaseReceiptDecisionProof.Copy(command);displayed=PurchaseReceiptDecisionProof.Copy(displayed);command.Require(displayed);await requireAuthority();
        if(_resolveConnection is null)throw new InvalidOperationException("A request-bound PRODUCTION resolver is required.");
        var connection=await _resolveConnection();if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("The original correction production connection is unavailable.");
        var source=_editor.GetDataSource(connection)??throw new InvalidOperationException("The original correction datasource is unavailable.");
        static string Json<T>(T value)=>JsonSerializer.Serialize(value);
        async Task Guard(bool owned)
        {
            await requireAuthority();if(await _resolveConnection()!=connection || !ReferenceEquals(_editor.GetDataSource(connection),source))throw new InvalidOperationException("The original correction production binding changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);
        }
        async Task<APInvoiceSourceReview> Raw(bool owned)
        {
            await Guard(owned);var headers=await GetRepoAsync<AP_INVOICE>("AP_INVOICE");var lineRepo=await GetRepoAsync<AP_INVOICE_LINE>("AP_INVOICE_LINE");
            var rows=(await headers.GetAsync([new(){FieldName="AP_INVOICE_ID",Operator="=",FilterValue=command.InvoiceId}]))?.ToArray();
            if(rows is not {Length:1} || rows[0] is not AP_INVOICE header)throw new InvalidOperationException("The globally identified correction invoice is ambiguous or unavailable.");
            var items=(await lineRepo.GetAsync([new(){FieldName="AP_INVOICE_ID",Operator="=",FilterValue=command.InvoiceId}]))?.ToArray();
            if(items is null || items.Any(item=>item is not AP_INVOICE_LINE))throw new InvalidOperationException("The complete correction line set is unavailable.");
            var lines=items.Cast<AP_INVOICE_LINE>().OrderBy(line=>line.AP_INVOICE_LINE_ID,StringComparer.Ordinal).ToArray();APInvoiceOwnership.RequireLines(command.InvoiceId,lines);
            foreach(var line in lines)
            {
                var global=(await lineRepo.GetAsync([new(){FieldName="AP_INVOICE_LINE_ID",Operator="=",FilterValue=line.AP_INVOICE_LINE_ID}]))?.ToArray();
                if(global is not {Length:1} || Json(global[0])!=Json(line))throw new InvalidOperationException("A globally identified correction line is ambiguous or changed.");
            }
            await Guard(owned);return APInvoiceSourceReview.Create(command.Company,command.ActorUserId,header,lines);
        }
        async Task<APInvoiceSourceReview> Review(bool owned)
        {
            await Guard(owned);var current=PurchaseReceiptDecisionProof.Copy(await readFinancial());current.Require(command.Company,command.ActorUserId,command.InvoiceId,current.Invoice.Revision);
            var raw=await Raw(owned);
            if(Json(raw.Invoice)!=Json(current.Invoice.Invoice) || Json(raw.Lines)!=Json(current.Invoice.Lines))throw new InvalidOperationException("The correction source differs from its complete owned financial capture.");
            var original=current;
            if(raw.Invoice.CORRECTION_HISTORY_JSON is not null && APInvoiceCorrectionHistory.Require(command.Company,raw.Invoice,raw.Lines).Versions[^1].Command.Same(command))original=current.OriginalCorrectionEvidence(command);
            command.Require(APInvoiceCorrectionReview.Create(original,command.Draft,command.BindingRevision));await Guard(owned);return raw;
        }
        APInvoiceCorrectionResult? confirmed=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Guard(true);
            foreach(var (table,fields) in new[]{("AP_INVOICE",new[]{("CORRECTION_HISTORY_JSON",typeof(string)),("TOTAL_AMOUNT",typeof(decimal)),("ROW_CHANGED_BY",typeof(string)),("ROW_CHANGED_DATE",typeof(DateTime))}),
                ("AP_INVOICE_LINE",new[]{("QUANTITY",typeof(decimal)),("UNIT_PRICE",typeof(decimal)),("LINE_TOTAL",typeof(decimal)),("ITEM_DESCRIPTION",typeof(string)),("ROW_CHANGED_BY",typeof(string)),("ROW_CHANGED_DATE",typeof(DateTime))})})
            {
                var schema=source.GetEntityStructure(new EntityStructure{EntityName=table,DatasourceEntityName=table},true);
                foreach(var (name,type) in fields)
                {
                    var found=schema?.Fields?.Where(field=>field.FieldName==name).ToArray();
                    if(found is not {Length:1} || found[0].Fieldtype!=type.FullName || name=="CORRECTION_HISTORY_JSON" && (!found[0].AllowDBNull || found[0].Size<APInvoiceCorrectionHistory.MaximumLength))
                        throw new InvalidOperationException("Review typed correction fields and the nullable bounded history column in the PRODUCTION Beep migration.");
                }
            }
            var before=await Review(true);_ = await Review(true);
            var retry=before.Invoice.CORRECTION_HISTORY_JSON is not null && APInvoiceCorrectionHistory.Require(command.Company,before.Invoice,before.Lines).Versions[^1].Command.Same(command);
            if(!retry)
            {
                var changed=DateTime.UtcNow;
                if(before.Invoice.ROW_CHANGED_DATE is {} prior && changed<prior)throw new InvalidOperationException("Current correction audit precedes its recorded source.");
                var header=PurchaseReceiptDecisionProof.Copy(before.Invoice);var lines=PurchaseReceiptDecisionProof.Copy(before.Lines.ToArray());
                header.TOTAL_AMOUNT=command.Draft.TotalAmount;header.ROW_CHANGED_BY=command.ActorUserId;header.ROW_CHANGED_DATE=changed;
                foreach(var line in lines)
                {
                    var edit=command.Draft.Lines.Single(value=>value.LineId==line.AP_INVOICE_LINE_ID);line.QUANTITY=edit.Quantity;line.UNIT_PRICE=edit.UnitPrice;line.LINE_TOTAL=edit.LineTotal;line.ITEM_DESCRIPTION=edit.Description;line.ROW_CHANGED_BY=command.ActorUserId;line.ROW_CHANGED_DATE=changed;
                }
                header.CORRECTION_HISTORY_JSON=APInvoiceCorrectionHistory.Create(command,displayed,header,lines);
                foreach(var line in lines)
                {
                    var original=before.Lines.Single(value=>value.AP_INVOICE_LINE_ID==line.AP_INVOICE_LINE_ID);
                    var writer=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(AP_INVOICE_LINE),connection,"AP_INVOICE_LINE",
                        rowAccess:new CorrectionMutation(command,"AP_INVOICE_LINE",original,line,changed,()=>Guard(true)));
                    await writer.UpdateAsync(line,command.ActorUserId);
                }
                var headerWriter=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(AP_INVOICE),connection,"AP_INVOICE",
                    rowAccess:new CorrectionMutation(command,"AP_INVOICE",before.Invoice,header,changed,()=>Guard(true)));
                await headerWriter.UpdateAsync(header,command.ActorUserId);
                var stored=await Review(true);
                if(Json(stored.Invoice)!=Json(header) || Json(stored.Lines)!=Json(lines))throw new InvalidOperationException("The stored correction differs from its exact reviewed write.");
            }
            var final=await Review(true);confirmed=new(command,final.Invoice,final.Lines);confirmed.Require(new(command,displayed));await Guard(true);
        });
        var durable=await Review(false);var result=new APInvoiceCorrectionResult(command,durable.Invoice,durable.Lines);result.Require(new(command,displayed));
        if(Json(result)!=Json(confirmed))throw new InvalidOperationException("Committed correction is unconfirmed. Reconcile the original intent.");
        await Guard(false);return PurchaseReceiptDecisionProof.Copy(result);
    }
    private sealed class CorrectionMutation(APInvoiceCorrectionCommand command,string table,object original,object intended,DateTime changed,Func<Task> guard):IPPDMRowAccessPolicy
    {
        private readonly string originalJson=JsonSerializer.Serialize(original),intendedJson=JsonSerializer.Serialize(intended);
        private Type RowType=>table=="AP_INVOICE"?typeof(AP_INVOICE):typeof(AP_INVOICE_LINE);
        private string Id=>table=="AP_INVOICE"?command.InvoiceId:((AP_INVOICE_LINE)original).AP_INVOICE_LINE_ID;
        private async Task Table(Type type,string name){await guard();if(type!=RowType || name!=table)throw new InvalidOperationException("Correction mutation is restricted to its reviewed invoice and lines.");}
        public async Task ValidateMutationAsync(Type type,string name,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {
            await Table(type,name);
            if(actor!=command.ActorUserId || operation!=PPDMRowMutation.Update || JsonSerializer.Serialize(previous)!=originalJson)throw new InvalidOperationException("Only the reviewed original correction update is authorized.");
            if(entity is AP_INVOICE header){header.ROW_CHANGED_BY=command.ActorUserId;header.ROW_CHANGED_DATE=changed;}
            else if(entity is AP_INVOICE_LINE line){line.ROW_CHANGED_BY=command.ActorUserId;line.ROW_CHANGED_DATE=changed;}
            else throw new InvalidOperationException("Correction row type changed.");
            if(JsonSerializer.Serialize(entity)!=intendedJson)throw new InvalidOperationException("Correction mutation changed fields outside the exact reviewed write.");
        }
        public async Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string name)
        {await Table(type,name);return new List<IReadOnlyList<AppFilter>>{new List<AppFilter>{new(){FieldName=table+"_ID",Operator="=",FilterValue=Id}}};}
        public async Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string name)
        {await Table(type,name);return table=="AP_INVOICE"?new List<AppFilter>{new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=command.Company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=command.Company.BusinessAssociateId}}:new List<AppFilter>{new(){FieldName="AP_INVOICE_ID",Operator="=",FilterValue=command.InvoiceId}};}
        public async Task ValidateRowsAsync(Type type,string name,IReadOnlyList<object> rows)
        {await Table(type,name);if(rows.Count!=1 || JsonSerializer.Serialize(rows[0])!=originalJson)throw new InvalidOperationException("The original correction row changed before mutation.");}
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string name,object row,bool inherited=true)=>throw new InvalidOperationException("Correction mutation cannot grant reads.");
    }
}
