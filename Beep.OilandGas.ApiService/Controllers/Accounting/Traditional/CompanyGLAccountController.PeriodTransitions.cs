using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Accounting.Services;
namespace Beep.OilandGas.ApiService.Controllers.Accounting.Traditional;
public sealed partial class CompanyGLAccountController
{
    private async Task<(CompanyScope Company,string Actor,string Binding,Func<Task> Guard)> PeriodStateAuthority(CompanyLedgerPeriodTransitionCommand? command=null)
    {
        var original=await Authority();async Task Guard(){await original.Guard();var current=await context.CaptureAsync(User);if(!current.Access.Permissions.Contains("Accounting.ManagePeriods",StringComparer.Ordinal) || command?.ToState=="OPEN" && !current.Access.Permissions.Contains("Accounting.ReopenPeriods",StringComparer.Ordinal) || command is not null && (command.Company!=original.Company || command.ActorUserId!=original.Actor || command.BindingRevision!=original.Binding))throw new UnauthorizedAccessException("Current owned period-management and separate reopening authority are required.");await original.Guard();}await Guard();return(original.Company,original.Actor,original.Binding,Guard);
    }
    [HttpGet("periods/transition-review")]
    public Task<ActionResult> PeriodStateReview([FromQuery]string book,[FromQuery]string currency,[FromQuery]string calendar,[FromServices]PeriodClosingService periods)=>ConfigurationExecute(async()=>{
        var authority=await PeriodStateAuthority();var history=await periods.GetCompanyPeriodsAsync(authority.Company,authority.Actor,book,currency,calendar,authority.Guard);await authority.Guard();var review=new CompanyLedgerPeriodConfigurationReview(history,authority.Binding);review.Require(authority.Company,authority.Actor,book,currency,calendar);return Ok(review);
    });
    [HttpPost("periods/transitions")]
    public Task<ActionResult> ChangePeriodState([FromBody]CompanyLedgerPeriodTransitionSubmission submission,[FromServices]PeriodClosingService periods)
    {
        if(submission?.Command is null)return Task.FromResult<ActionResult>(BadRequest());return ConfigurationExecute(async()=>{submission=CompanyInventorySourceReview.Copy(submission);submission.Command.Require();var authority=await PeriodStateAuthority(submission.Command);var result=await periods.TransitionCompanyPeriodAsync(submission,authority.Guard);result.Require(submission.Command);await authority.Guard();return Ok(result);});
    }
    [HttpGet("periods/transitions/{periodId}/{intentId}")]
    public Task<ActionResult> PeriodStateRecovery(string periodId,string intentId,[FromQuery]string book,[FromQuery]string currency,[FromQuery]string calendar,[FromServices]PeriodClosingService periods)=>ConfigurationExecute(async()=>{
        var authority=await Authority();var result=await periods.GetCompanyPeriodTransitionAsync(authority.Company,authority.Actor,book,currency,calendar,periodId,intentId,authority.Guard);await authority.Guard();if(result is null)return NotFound();if(result.Command.BindingRevision!=authority.Binding)throw new InvalidOperationException("Return to the original period transition binding.");result.Require(result.Command);return Ok(result);
    });
}
