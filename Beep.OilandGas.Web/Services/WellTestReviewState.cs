using Beep.OilandGas.Models.Data.Production;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Web.Services;
public sealed class WellTestReviewState:IDisposable
{
    private readonly CompanyClientContext company;
    public WellTestReviewState(CompanyClientContext company){this.company=company;company.Changed+=Clear;}
    private long version;
    public long Version=>version;
    public bool Confirmed { get; private set; }
    public DateTimeOffset? RetrievedAtUtc { get; private set; }
    public string? Field { get; private set; }
    public string? Well { get; private set; }
    public IReadOnlyList<WellTestResponse> Rows { get; private set; }=[];
    public WellTestCoverage Coverage { get; private set; }
    public WellTestResponse? Selected { get; private set; }
    public IReadOnlyList<WellTestResponse> VisibleRows=>Rows.Where(x=>Includes(x,Coverage)).ToArray();
    public int Count(WellTestCoverage coverage)=>Rows.Count(x=>Includes(x,coverage));
    public void SelectCoverage(WellTestCoverage coverage){if(!Enum.IsDefined(coverage))throw new ArgumentOutOfRangeException(nameof(coverage));Coverage=coverage;Selected=null;}
    public void SelectRecord(string id)=>Selected=Confirmed ? VisibleRows.SingleOrDefault(x=>x.TestId==id) : null;
    private static bool Includes(WellTestResponse row,WellTestCoverage coverage)=>coverage switch{WellTestCoverage.All=>true,WellTestCoverage.Oil=>row.OilFlowAmount.HasValue,WellTestCoverage.Gas=>row.GasFlowAmount.HasValue,WellTestCoverage.Water=>row.WaterFlowAmount.HasValue,_=>false};
    public void Clear(){++version;Coverage=WellTestCoverage.All;Selected=null;Confirmed=false;RetrievedAtUtc=null;Field=null;Well=null;Rows=[];}
    public async Task LoadAsync(string well,Func<Task<string?>> fieldLookup,Func<string,Task<WellTestReview>> read)
    {
        var scope=company.Snapshot;Clear();var generation=version;
        if(scope.Selection is null || string.IsNullOrWhiteSpace(well))return;
        var field=await fieldLookup();if(generation!=version || scope!=company.Snapshot)return;
        if(string.IsNullOrWhiteSpace(field))throw new InvalidOperationException("Confirm FIELD before reading well observations.");
        var review=await read(well);if(generation!=version || scope!=company.Snapshot)return;
        if(review is null || review.Company!=scope.Selection.Scope || review.Actor!=scope.Selection.UserId || review.Field!=field || review.Well!=well || review.RetrievedAtUtc==default || review.RetrievedAtUtc.Offset!=TimeSpan.Zero)
            throw new InvalidOperationException("Returned observation review differs from the original company, account, FIELD or well.");
        var rows=review.Rows;
        if(rows is null || rows.Any(x=>x is null || string.IsNullOrWhiteSpace(x.TestId) ||
            (x.Uwi!=well && x.WellId!=well)) ||
            rows.Select(x=>x.TestId).Distinct(StringComparer.Ordinal).Count()!=rows.Count)
            throw new InvalidOperationException("Returned observation identity does not match the reviewed well.");
        Rows=rows.ToArray();RetrievedAtUtc=review.RetrievedAtUtc;Field=field;Well=well;Confirmed=true;
    }
    public void Dispose(){company.Changed-=Clear;Clear();}
}

public enum WellTestCoverage { All,Oil,Gas,Water }
