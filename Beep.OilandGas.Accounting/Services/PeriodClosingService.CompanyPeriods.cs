using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Accounting.Services;

public partial class PeriodClosingService
{
    private const string CompanyPeriodTable="COMPANY_LEDGER_PERIOD";
    private static void RequireCompanyPeriodSchema(IDataSource source)
    {
        var schema=source.GetEntityStructure(new EntityStructure{EntityName=CompanyPeriodTable,DatasourceEntityName=CompanyPeriodTable},true);
        var keys=schema?.Fields?.Where(field=>field.IsKey).ToArray();
        if(schema?.EntityName!=CompanyPeriodTable || keys is not {Length:1} || keys[0].FieldName!=CompanyPeriodTable+"_ID" || keys[0].Fieldtype!=typeof(string).FullName || keys[0].AllowDBNull || Math.Max(keys[0].Size,keys[0].Size1)<32)
            throw new InvalidOperationException("Review the canonical company period identity through the PRODUCTION Beep migration.");
        foreach(var (name,size) in new[]{("OWNER_TENANT_ID",128),("OWNER_COMPANY_BA_ID",128),("BOOK_ID",128),("CURRENCY_CODE",16),("CALENDAR_ID",128),("PERIOD_CODE",128),("STATUS",16),("AUTHORITY_JSON",CompanyLedgerPeriodAuthority.MaximumLength)})
        {
            var fields=schema?.Fields?.Where(field=>field.FieldName==name).ToArray();if(fields is not {Length:1} || fields[0].Fieldtype!=typeof(string).FullName || fields[0].AllowDBNull || Math.Max(fields[0].Size,fields[0].Size1)<size)
                throw new InvalidOperationException("The complete bounded company/book/calendar period state and authority metadata is unconfirmed.");
        }
        foreach(var name in new[]{"START_DATE","END_DATE"}){var fields=schema?.Fields?.Where(field=>field.FieldName==name).ToArray();if(fields is not {Length:1} || fields[0].Fieldtype!=typeof(DateTime).FullName || fields[0].AllowDBNull)throw new InvalidOperationException("The required company calendar period dates are unconfirmed.");}
    }
    public async Task<CompanyLedgerPeriodReview> GetCompanyPeriodsAsync(CompanyScope company,string actor,string book,string currency,string calendar,Func<Task> requireAuthority)
    {
        ArgumentNullException.ThrowIfNull(requireAuthority);_=CompanyLedgerPeriodReview.Create(company,actor,book,currency,calendar,[]);await requireAuthority();
        if(_resolveConnection is null)throw new InvalidOperationException("A request-bound original PRODUCTION period resolver is required.");var connection=await _resolveConnection();if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("The original period binding is unavailable.");
        var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("The original period datasource is unavailable.");
        async Task Guard(){await requireAuthority();if(await _resolveConnection()!=connection || !ReferenceEquals(_editor.GetDataSource(connection),source))throw new InvalidOperationException("The original period binding changed.");}
        async Task<CompanyLedgerPeriodReview> Capture()
        {
            await Guard();RequireCompanyPeriodSchema(source);var repo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(COMPANY_LEDGER_PERIOD),connection,CompanyPeriodTable);
            // Read all active calendars for this book/currency; a caller cannot choose a second calendar to bypass its recorded period authority.
            var rows=(await repo.GetAsync([new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=company.BusinessAssociateId},new(){FieldName="BOOK_ID",Operator="=",FilterValue=book},new(){FieldName="CURRENCY_CODE",Operator="=",FilterValue=currency},new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}]))?.ToArray();await Guard();
            if(rows is null || rows.Length>CompanyLedgerPeriodReview.MaximumPeriods || rows.Any(row=>row is not COMPANY_LEDGER_PERIOD))throw new InvalidOperationException("Complete bounded company period records are unavailable.");
            var result=CompanyLedgerPeriodReview.Create(company,actor,book,currency,calendar,rows.Cast<COMPANY_LEDGER_PERIOD>());await Guard();return result;
        }
        var initial=await Capture();var closing=await Capture();await Guard();if(initial.Revision!=closing.Revision || JsonSerializer.Serialize(initial)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The complete original company period authority changed during review.");return CompanyInventorySourceReview.Copy(closing);
    }
}
