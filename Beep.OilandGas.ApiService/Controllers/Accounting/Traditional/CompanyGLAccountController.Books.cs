using Microsoft.AspNetCore.Mvc;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Controllers.Accounting.Traditional;
public sealed partial class CompanyGLAccountController
{
    [HttpGet("books")]
    public Task<ActionResult> Books()=>ConfigurationExecute(async()=>{var authority=await Authority();var result=await accounts.GetCompanyBooksAsync(authority.Company,authority.Actor,authority.Guard);result.Require(authority.Company,authority.Actor);await authority.Guard();return Ok(result);});
    [HttpGet("books/configuration-review")]
    public Task<ActionResult> BookReview()=>ConfigurationExecute(async()=>{var authority=await Authority(true);var result=new CompanyLedgerBookConfigurationReview(await accounts.GetCompanyBooksAsync(authority.Company,authority.Actor,authority.Guard),authority.Binding);result.Require(authority.Company,authority.Actor);await authority.Guard();return Ok(result);});
    [HttpPost("books/configuration")]
    public Task<ActionResult> ConfigureBook([FromBody]CompanyLedgerBookConfigurationSubmission submission)
    {
        if(submission?.Command is null)return Task.FromResult<ActionResult>(BadRequest());return ConfigurationExecute(async()=>{submission=CompanyInventorySourceReview.Copy(submission);submission.Command.Require();var authority=await Authority(true,book:submission.Command);var result=await accounts.ConfigureCompanyBookAsync(submission,authority.Guard);if(CompanyLedgerBookConfigurationProof.Require(result).Command!=submission.Command)throw new InvalidOperationException("The original native book configuration is unconfirmed.");await authority.Guard();return Ok(result);});
    }
    [HttpGet("books/configuration/{id}")]
    public Task<ActionResult> RecoverBook(string id)=>ConfigurationExecute(async()=>{var authority=await Authority();var result=await accounts.GetCompanyBookConfigurationAsync(authority.Company,authority.Actor,id,authority.Guard);await authority.Guard();if(result is null)return NotFound();if(CompanyLedgerBookConfigurationProof.Require(result).Command.BindingRevision!=authority.Binding)throw new InvalidOperationException("Return to the original book configuration binding.");return Ok(result);});
}
