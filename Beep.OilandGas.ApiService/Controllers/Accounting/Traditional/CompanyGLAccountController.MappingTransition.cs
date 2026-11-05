using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Accounting.Services;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Controllers.Accounting.Traditional;
public sealed partial class CompanyGLAccountController
{
    [HttpPost("mappings/transitions")]
    public Task<ActionResult> TransitionMapping([FromBody]CompanyGLMappingTransitionSubmission submission,[FromServices]AccountMappingService mappings)
    {
        if(submission?.Command is null)return Task.FromResult<ActionResult>(BadRequest());return ConfigurationExecute(async()=>{submission=CompanyInventorySourceReview.Copy(submission);submission.Command.Require();var authority=await Authority(true,transition:submission.Command);var result=await mappings.TransitionCompanyMappingAsync(submission,authority.Guard);result.Require(submission.Command);await authority.Guard();return Ok(result);});
    }
    [HttpGet("mappings/transitions/{id}")]
    public Task<ActionResult> RecoverMappingTransition(string id,[FromQuery]string book,[FromQuery]string currency,[FromServices]AccountMappingService mappings)=>ConfigurationExecute(async()=>{
        var authority=await Authority();var result=await mappings.GetCompanyMappingTransitionAsync(authority.Company,authority.Actor,book,currency,id,authority.Guard);await authority.Guard();
        if(result is null)return NotFound();if(result.Command.BindingRevision!=authority.Binding)throw new InvalidOperationException("Return to the original transition binding to reconcile.");result.Require(result.Command);return Ok(result);
    });
}
