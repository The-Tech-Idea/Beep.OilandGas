using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Accounting.Services;
public partial class JournalEntryService
{
    private async Task<CompanySalesJournalPostingOutcome> ConfirmNativeJournalPostingAsync(JOURNAL_ENTRY header,string connection,IDataSource source,Func<Task> guard,Func<string,Task> fieldRead)
    {
        await guard();await fieldRead(header.FIELD_ID!);RequireCompanyJournalPostingSchema(source);var transition=CompanySalesJournalPostingHeaderProof.Require(header);
        PPDMGenericRepository Repo(Type type,string table)=>new(_editor,_commonColumnHandler,_defaults,_metadata,type,connection,table);
        async Task<T[]> Rows<T>(string table,string key,string value,int maximum)
        {
            await guard();var rows=(await Repo(typeof(T),table).GetAsync([new(){FieldName=key,Operator="=",FilterValue=value}]))?.ToArray();await guard();if(rows is null || rows.Length>maximum || rows.Any(row=>row is not T))throw new InvalidOperationException("Complete actual native posting artifacts are unavailable or ambiguous.");return rows.Cast<T>().ToArray();
        }
        var intents=await Rows<SALES_JOURNAL_POSTING_INTENT>("SALES_JOURNAL_POSTING_INTENT","JOURNAL_ENTRY_ID",header.JOURNAL_ENTRY_ID,1);if(intents.Length!=1 || intents[0].SALES_JOURNAL_POSTING_INTENT_ID!=transition.IntentId)throw new InvalidOperationException("The posted native journal lacks its exact retained posting request.");var command=intents[0].Restore();
        var records=await Rows<SALES_JOURNAL_POSTING_OUTCOME>("SALES_JOURNAL_POSTING_OUTCOME","JOURNAL_ENTRY_ID",header.JOURNAL_ENTRY_ID,1);if(records.Length!=1)throw new InvalidOperationException("The posted journal lacks its durable native AR/GL correlation.");var stored=records[0].Restore();stored.Require(command);
        var currentHeaders=await Rows<JOURNAL_ENTRY>("JOURNAL_ENTRY","JOURNAL_ENTRY_ID",header.JOURNAL_ENTRY_ID,1);if(currentHeaders.Length!=1 || JsonSerializer.Serialize(currentHeaders[0])!=JsonSerializer.Serialize(header))throw new InvalidOperationException("The actual complete posted header changed.");
        var lines=await Rows<JOURNAL_ENTRY_LINE>("JOURNAL_ENTRY_LINE","JOURNAL_ENTRY_ID",header.JOURNAL_ENTRY_ID,4);var entries=await Rows<GL_ENTRY>("GL_ENTRY","JOURNAL_ENTRY_ID",header.JOURNAL_ENTRY_ID,4);if(entries.Length!=command.EntryIds.Count || entries.Select(row=>row.GL_ENTRY_ID).Distinct(StringComparer.Ordinal).Count()!=entries.Length)throw new InvalidOperationException("The exact complete posted GL cardinality/identities are unconfirmed.");
        var ordered=command.EntryIds.Select(id=>entries.SingleOrDefault(row=>row.GL_ENTRY_ID==id)??throw new InvalidOperationException("An original native GL entry is missing.")).ToArray();var actual=new CompanySalesJournalPostingOutcome(intents[0],new(header,lines.OrderBy(row=>row.LINE_NUMBER).ToArray()),ordered,stored.Receivable);actual.Require(command);
        if(JsonSerializer.Serialize(actual)!=JsonSerializer.Serialize(stored))throw new InvalidOperationException("The durable outcome does not match every actual native journal/GL/AR fact.");
        await RequireActualJournalReviewerAsync(header,connection,source,guard);await ARService.ConfirmNativePostingReceivableAsync(command.Source.Current.Company,stored.Receivable,source,Repo,guard,fieldRead);await guard();return CompanyInventorySourceReview.Copy(actual);
    }
    private async Task RequireActualJournalPostingAsync(JOURNAL_ENTRY header,string connection,IDataSource source,Func<Task> guard,Func<string,Task> fieldRead)
    {
        if(header.POSTING_WORKFLOW_JSON is null && header.SALES_JOURNAL_POSTING_INTENT_ID is null){if(header.STATUS=="POSTED")throw new InvalidOperationException("A company posted header requires its original native posting artifacts.");return;}
        _=await ConfirmNativeJournalPostingAsync(header,connection,source,guard,fieldRead);
    }
}
