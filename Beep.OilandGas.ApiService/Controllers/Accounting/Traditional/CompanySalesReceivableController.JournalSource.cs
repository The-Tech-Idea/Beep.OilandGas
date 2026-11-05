using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Accounting.Services;
namespace Beep.OilandGas.ApiService.Controllers.Accounting.Traditional;
public sealed partial class CompanySalesReceivableController
{
    [HttpGet("journal-source/{receivableId}")]
    public Task<ActionResult> JournalSource(string receivableId,[FromQuery]string book,[FromQuery]string currency,[FromQuery]string calendar,[FromQuery]string date,
        [FromQuery]string receivableKey,[FromQuery]string revenueKey,[FromQuery]string? chargesKey,[FromQuery]string? taxKey,[FromServices]JournalEntryService journal,[FromServices]AccountMappingService mappings,[FromServices]PeriodClosingService periods)
    {
        if(!DateTime.TryParseExact(date,"yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture,System.Globalization.DateTimeStyles.None,out var postingDate))return Task.FromResult<ActionResult>(BadRequest());
        return Execute(async()=>Ok(await reads.ReadJournalSourceAsync(User,RecordedSourceRouteIds.Read(Request,nameof(receivableId)),book,currency,calendar,postingDate,new(receivableKey,revenueKey,chargesKey,taxKey),journal,mappings,periods,HttpContext.RequestAborted)));
    }
}
