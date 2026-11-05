using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.ProductionAccounting.Services;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class PeriodCloseJournalRecordsTests
{
    private static readonly DateTime End=new(2026,9,30);
    private static JOURNAL_ENTRY Entry()=>new(){JOURNAL_ENTRY_ID="journal",REFERENCE_NUMBER="CLOSE-field-202609",SOURCE_MODULE="PERIOD_CLOSING",ACTIVE_IND="Y",ENTRY_DATE=End,STATUS="POSTED"};
    [Fact] public void QueryUsesActualWriterReferenceAndSourceRatherThanAnUnwrittenEntryNumber()
    {
        var filters=PeriodCloseJournalRecords.Filters("field",End,"Y");
        Assert.Equal(new[]{"REFERENCE_NUMBER","SOURCE_MODULE","ACTIVE_IND"},filters.Select(row=>row.FieldName));
        Assert.Equal(PeriodCloseJournalRecords.Reference("field",End),filters[0].FilterValue);Assert.Equal("PERIOD_CLOSING",filters[1].FilterValue);
    }
    [Fact] public void EmptyRecordedResultIsOpen(){Assert.Equal("OPEN",PeriodCloseJournalRecords.Status([],"field",End,"Y"));}
    [Theory][InlineData("POSTED","CLOSED")][InlineData("DRAFT","CLOSING")]
    public void OnlyRecordedPostedJournalReportsClosed(string journalStatus,string expected)
    {
        var row=Entry();row.STATUS=journalStatus;Assert.Equal(expected,PeriodCloseJournalRecords.Status([row],"field",End,"Y"));
    }
    [Theory][InlineData("missing")][InlineData("duplicate")][InlineData("reference")][InlineData("module")][InlineData("date")][InlineData("inactive")][InlineData("identity")][InlineData("status")]
    public void UnconfirmedOrAmbiguousJournalCannotReportClosed(string fault)
    {
        var row=Entry();IReadOnlyList<JOURNAL_ENTRY>? rows=[row];
        switch(fault){case "missing":rows=null;break;case "duplicate":rows=[row,row];break;case "reference":row.REFERENCE_NUMBER="CLOSE-other-202609";break;
            case "module":row.SOURCE_MODULE="MANUAL";break;case "date":row.ENTRY_DATE=End.AddDays(-1);break;case "inactive":row.ACTIVE_IND="N";break;case "identity":row.JOURNAL_ENTRY_ID="";break;default:row.STATUS="UNKNOWN";break;}
        Assert.Throws<InvalidOperationException>(()=>PeriodCloseJournalRecords.Status(rows,"field",End,"Y"));
    }
    [Theory][InlineData("ar-SA")][InlineData("th-TH")][InlineData("en-US")]
    public void ClosingReferenceIsIndependentOfUserCalendar(string culture)
    {
        var previous=System.Globalization.CultureInfo.CurrentCulture;
        try{System.Globalization.CultureInfo.CurrentCulture=new(culture);Assert.Equal("CLOSE-field-202609",PeriodCloseJournalRecords.Reference("field",End));}
        finally{System.Globalization.CultureInfo.CurrentCulture=previous;}
    }
    [Fact] public void MissingActiveIndicatorCannotBecomeAnUnfilteredRecordQuery()
    {
        Assert.Throws<ArgumentException>(()=>PeriodCloseJournalRecords.Filters("field",End,""));
        Assert.Throws<ArgumentException>(()=>PeriodCloseJournalRecords.Status([],"field",End,""));
    }
}
