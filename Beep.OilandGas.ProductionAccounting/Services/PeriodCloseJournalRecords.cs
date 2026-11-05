using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.ProductionAccounting.Constants;
using TheTechIdea.Beep.Report;
namespace Beep.OilandGas.ProductionAccounting.Services;
/// <summary>Recorded closing journal identity/status, not an accounting-period lock or ledger receipt.</summary>
public static class PeriodCloseJournalRecords
{
    public static string Reference(string field,DateTime end)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(field);if(end==default)throw new ArgumentException("A period end is required.");
        return "CLOSE-"+field+"-"+end.ToString("yyyyMM",System.Globalization.CultureInfo.InvariantCulture);
    }
    public static List<AppFilter> Filters(string field,DateTime end,string active)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(active);
        return [
        new(){FieldName="REFERENCE_NUMBER",Operator="=",FilterValue=Reference(field,end)},
        new(){FieldName="SOURCE_MODULE",Operator="=",FilterValue=AccountingSourceModuleCodes.PeriodClosing},
        new(){FieldName="ACTIVE_IND",Operator="=",FilterValue=active}];
    }
    public static string Status(IReadOnlyList<JOURNAL_ENTRY>? entries,string field,DateTime end,string active)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(active);
        var reference=Reference(field,end);
        if(entries is null || entries.Count>1 || entries.Any(row=>row is null || string.IsNullOrWhiteSpace(row.JOURNAL_ENTRY_ID) ||
            row.REFERENCE_NUMBER!=reference || row.SOURCE_MODULE!=AccountingSourceModuleCodes.PeriodClosing || row.ACTIVE_IND!=active || row.ENTRY_DATE?.Date!=end.Date))
            throw new InvalidOperationException("The recorded period-close journal cannot be confirmed.");
        if(entries.Count==0)return PeriodCloseStatusCodes.Open;
        return entries[0].STATUS switch {"POSTED"=>PeriodCloseStatusCodes.Closed,"DRAFT"=>PeriodCloseStatusCodes.Closing,
            _=>throw new InvalidOperationException("The recorded closing journal status is unconfirmed.")};
    }
}
