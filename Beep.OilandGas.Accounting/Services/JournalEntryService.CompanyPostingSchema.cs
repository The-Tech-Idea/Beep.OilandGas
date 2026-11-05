using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Accounting.Services;
public partial class JournalEntryService
{
    internal static void RequireCompanyPostingRecordSchema(IDataSource source,string table,bool outcome)
    {
        var schema=source.GetEntityStructure(new EntityStructure{EntityName=table,DatasourceEntityName=table},true);var keys=schema?.Fields?.Where(value=>value.IsKey).ToArray();
        if(schema?.EntityName!=table || keys is not {Length:1} || keys[0].FieldName!=table+"_ID" || keys[0].AllowDBNull || keys[0].Fieldtype!=typeof(string).FullName)throw new InvalidOperationException("The original native posting record identity is unconfirmed. Review the PRODUCTION Beep migration.");
        var columns=new List<(string,int)>{(table+"_ID",32),("OWNER_TENANT_ID",128),("OWNER_COMPANY_BA_ID",128),("BOOK_ID",128),("CURRENCY_CODE",16),("FIELD_ID",128),("JOURNAL_ENTRY_ID",32),("SOURCE_AR_INVOICE_ID",32),("SOURCE_REVISION",64)};
        columns.AddRange(outcome?[("SALES_JOURNAL_POSTING_INTENT_ID",32),("RECORDED_REVISION",64),("OUTCOME_JSON",SALES_JOURNAL_POSTING_OUTCOME.MaximumOutcomeLength)]:[("COMMAND_JSON",SALES_JOURNAL_POSTING_INTENT.MaximumCommandLength)]);
        foreach(var (name,size) in columns){var fields=schema.Fields.Where(value=>value.FieldName==name).ToArray();if(fields is not {Length:1} || fields[0].AllowDBNull || fields[0].Fieldtype!=typeof(string).FullName || Math.Max(fields[0].Size,fields[0].Size1)<size)throw new InvalidOperationException("Complete bounded native posting record metadata is unconfirmed.");}
    }
    private static void RequireCompanyJournalPostingSchema(IDataSource source)
    {
        RequireCompanyJournalReviewSchema(source);RequireCompanyPostingRecordSchema(source,"SALES_JOURNAL_POSTING_INTENT",false);RequireCompanyPostingRecordSchema(source,"SALES_JOURNAL_POSTING_OUTCOME",true);
        var header=source.GetEntityStructure(new EntityStructure{EntityName="JOURNAL_ENTRY",DatasourceEntityName="JOURNAL_ENTRY"},true);
        foreach(var (name,size) in new[]{("POSTING_WORKFLOW_JSON",CompanySalesJournalPostingHeaderProof.MaximumLength),("SALES_JOURNAL_POSTING_INTENT_ID",32)}){var fields=header?.Fields?.Where(value=>value.FieldName==name).ToArray();if(fields is not {Length:1} || !fields[0].AllowDBNull || fields[0].Fieldtype!=typeof(string).FullName || Math.Max(fields[0].Size,fields[0].Size1)<size)throw new InvalidOperationException("Review the bounded nullable native posting header metadata through Beep migration.");}
        const string table="GL_ENTRY";var schema=source.GetEntityStructure(new EntityStructure{EntityName=table,DatasourceEntityName=table},true);var keys=schema?.Fields?.Where(value=>value.IsKey).ToArray();if(keys is not {Length:1} || keys[0].FieldName!="GL_ENTRY_ID" || keys[0].AllowDBNull || keys[0].Fieldtype!=typeof(string).FullName || Math.Max(keys[0].Size,keys[0].Size1)<128)throw new InvalidOperationException("The existing canonical native GL identity is unconfirmed.");
        foreach(var (name,size) in new[]{("JOURNAL_ENTRY_ID",128),("GL_ACCOUNT_ID",128),("DESCRIPTION",1000),("REFERENCE_NUMBER",128),("SOURCE_MODULE",128),("OWNER_TENANT_ID",128),("OWNER_COMPANY_BA_ID",128),("BOOK_ID",128),("FIELD_ID",128),("CURRENCY_CODE",16),("SOURCE_AR_INVOICE_ID",32),("COMPANY_LEDGER_PERIOD_ID",32),("JOURNAL_ENTRY_LINE_ID",32),("SALES_JOURNAL_POSTING_INTENT_ID",32),("MAPPING_KEY",128)}){var fields=schema?.Fields?.Where(value=>value.FieldName==name).ToArray();if(fields is not {Length:1} || fields[0].Fieldtype!=typeof(string).FullName || Math.Max(fields[0].Size,fields[0].Size1)<size)throw new InvalidOperationException("Complete bounded company GL correlation metadata is unconfirmed.");}
        foreach(var (name,type) in new[]{("ENTRY_DATE",typeof(DateTime)),("DEBIT_AMOUNT",typeof(decimal)),("CREDIT_AMOUNT",typeof(decimal))}){var fields=schema?.Fields?.Where(value=>value.FieldName==name).ToArray();if(fields is not {Length:1} || fields[0].Fieldtype!=type.FullName)throw new InvalidOperationException("Exact native GL calendar date/currency amount types are unconfirmed.");}
    }
}
