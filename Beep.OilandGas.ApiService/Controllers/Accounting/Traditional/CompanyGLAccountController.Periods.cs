using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Accounting.Services;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Controllers.Accounting.Traditional;
public sealed partial class CompanyGLAccountController
{
    [HttpGet("periods")]
    public Task<ActionResult> Periods([FromQuery]string book,[FromQuery]string currency,[FromQuery]string calendar,[FromServices]PeriodClosingService periods)=>ConfigurationExecute(async()=>{
        var original=await Authority();var native=await periods.GetCompanyPeriodsAsync(original.Company,original.Actor,book,currency,calendar,original.Guard);await original.Guard();return Ok(CompanyLedgerPeriodSource.FromNative(native));
    });
    [HttpGet("periods/configuration-review")]
    public Task<ActionResult> PeriodConfigurationReview([FromQuery]string book,[FromQuery]string currency,[FromQuery]string calendar,[FromServices]PeriodClosingService periods)=>ConfigurationExecute(async()=>{
        var original=await Authority(true);var history=await periods.GetCompanyPeriodsAsync(original.Company,original.Actor,book,currency,calendar,original.Guard);await original.Guard();var review=new CompanyLedgerPeriodConfigurationReview(history,original.Binding);review.Require(original.Company,original.Actor,book,currency,calendar);return Ok(review);
    });
    [HttpPost("periods/configuration")]
    public Task<ActionResult> CreatePeriod([FromBody]CompanyLedgerPeriodCreationSubmission submission,[FromServices]PeriodClosingService periods)
    {
        if(submission?.Command is null)return Task.FromResult<ActionResult>(BadRequest());return ConfigurationExecute(async()=>{
            submission=CompanyInventorySourceReview.Copy(submission);submission.Command.Require();var original=await Authority(true,period:submission.Command);
            var result=await periods.CreateCompanyPeriodAsync(submission,original.Guard);await original.Guard();if(CompanyLedgerPeriodAuthority.Require(result).Command!=submission.Command)throw new InvalidOperationException("The original period outcome is unconfirmed.");return Ok(result);
        });
    }
    [HttpGet("periods/configuration/{id}/request")]
    public Task<ActionResult> RecoverPeriod(string id,[FromQuery]string book,[FromQuery]string currency,[FromQuery]string calendar,[FromServices]PeriodClosingService periods)=>ConfigurationExecute(async()=>{
        var original=await Authority(true);var command=await periods.GetCompanyPeriodCreationAsync(original.Company,original.Actor,book,currency,calendar,id,original.Guard);await original.Guard();
        if(command is null)return NotFound();if(command.BindingRevision!=original.Binding)throw new InvalidOperationException("Return to the original period binding to reconcile.");return Ok(command);
    });
}
