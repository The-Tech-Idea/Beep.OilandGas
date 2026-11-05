using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Data.Security;
namespace Beep.OilandGas.ApiService.Controllers.Accounting.Traditional;

[ApiController]
[Authorize]
[Route("api/accounting/traditional/gl/company-accounts")]
public sealed partial class CompanyGLAccountController(GLAccountService accounts,ICompanyAccountingReadContext context,ILogger<CompanyGLAccountController> logger):ControllerBase
{
    private async Task<(CompanyScope Company,string Actor,string Binding,Func<Task> Guard)> Authority(bool configure=false,CompanyGLAccountConfigurationCommand? command=null,CompanyGLMappingConfigurationCommand? mapping=null,CompanyLedgerPeriodCreation? period=null,CompanyGLMappingTransitionCommand? transition=null,CompanyLedgerBookConfigurationCommand? book=null,CompanyFxRateConfigurationCommand? fx=null)
    {
        var original=await context.CaptureAsync(User);
        if(!APInvoiceReadChecks.CanRead(User,original.Access.Scope,original.Access))throw new UnauthorizedAccessException();
        var roles=original.Access.Roles.Order(StringComparer.Ordinal).ToArray();var grants=original.Access.Permissions.Order(StringComparer.Ordinal).ToArray();
        async Task Guard()
        {
            var current=await context.CaptureAsync(User);
            if(!APInvoiceReadChecks.CanRead(User,original.Access.Scope,current.Access) || current.Actor!=original.Actor || current.Binding!=original.Binding ||
                configure && !current.Access.Permissions.Contains(PermissionConstants.Accounting.ConfigureLedger,StringComparer.Ordinal) ||
                command is not null && (command.Company!=current.Access.Scope || command.ActorUserId!=current.Actor || command.BindingRevision!=current.Binding) ||
                mapping is not null && (mapping.Company!=current.Access.Scope || mapping.ActorUserId!=current.Actor || mapping.BindingRevision!=current.Binding) ||
                period is not null && (period.Company!=current.Access.Scope || period.ActorUserId!=current.Actor || period.BindingRevision!=current.Binding) ||
                transition is not null && (transition.Company!=current.Access.Scope || transition.ActorUserId!=current.Actor || transition.BindingRevision!=current.Binding) ||
                book is not null && (book.Company!=current.Access.Scope || book.ActorUserId!=current.Actor || book.BindingRevision!=current.Binding) ||
                fx is not null && (fx.Company!=current.Access.Scope || fx.ActorUserId!=current.Actor || fx.BindingRevision!=current.Binding) ||
                !current.Access.Roles.Order(StringComparer.Ordinal).SequenceEqual(roles) || !current.Access.Permissions.Order(StringComparer.Ordinal).SequenceEqual(grants))
                throw new UnauthorizedAccessException("The original company ledger configuration authority changed.");
            if(!ProcessDefinitionRevision.IsValid(current.Binding))throw new InvalidOperationException("The production binding is unconfirmed.");
        }
        await Guard();return(original.Access.Scope,original.Actor,original.Binding,Guard);
    }
    [HttpGet("configuration-review")]
    public Task<ActionResult> ConfigurationReview([FromQuery]string book,[FromQuery]string currency)=>ConfigurationExecute(async()=>
    {
        var original=await Authority(true);var source=await accounts.GetCompanyAccountConfigurationReviewAsync(original.Company,original.Actor,book,currency,original.Guard);await original.Guard();
        var review=new CompanyGLAccountConfigurationReview(source,original.Binding);review.Require(original.Company,original.Actor,book,currency);return Ok(review);
    });
    [HttpGet("mappings/configuration-review")]
    public Task<ActionResult> MappingConfigurationReview([FromQuery]string book,[FromQuery]string currency,[FromQuery]string date,[FromServices]AccountMappingService mappings)
    {
        if(!DateTime.TryParseExact(date,"yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture,System.Globalization.DateTimeStyles.None,out var calendar))return Task.FromResult<ActionResult>(BadRequest());
        return ConfigurationExecute(async()=>{var original=await Authority(true);var history=await mappings.GetCompanyMappingConfigurationReviewAsync(original.Company,original.Actor,book,currency,calendar,original.Guard);await original.Guard();
            var review=new CompanyGLMappingConfigurationReview(history,original.Binding);review.Require(original.Company,original.Actor,book,currency,calendar);return Ok(review);});
    }
    [HttpPost("mappings/configuration")]
    public Task<ActionResult> ConfigureMapping([FromBody]CompanyGLMappingConfigurationSubmission submission,[FromServices]AccountMappingService mappings)
    {
        if(submission?.Command is null)return Task.FromResult<ActionResult>(BadRequest());return ConfigurationExecute(async()=>{
            submission=CompanyInventorySourceReview.Copy(submission);submission.Command.Require();var original=await Authority(true,mapping:submission.Command);
            var result=await mappings.ConfigureCompanyMappingAsync(submission,original.Guard);await original.Guard();
            if(CompanyGLMappingConfigurationProof.Require(result).Command!=submission.Command)throw new InvalidOperationException("The original mapping outcome is unconfirmed.");return Ok(result);});
    }
    [HttpGet("mappings/configuration/{id}/request")]
    public Task<ActionResult> RecoverMappingConfiguration(string id,[FromQuery]string book,[FromQuery]string currency,[FromServices]AccountMappingService mappings)=>ConfigurationExecute(async()=>{
        var original=await Authority(true);var command=await mappings.GetCompanyMappingConfigurationAsync(original.Company,original.Actor,book,currency,id,original.Guard);await original.Guard();
        if(command is null)return NotFound();if(command.BindingRevision!=original.Binding)throw new InvalidOperationException("Return to the original mapping binding to reconcile.");return Ok(command);
    });
    [HttpPost("configuration")]
    public Task<ActionResult> Configure([FromBody]CompanyGLAccountConfigurationSubmission submission)
    {
        if(submission?.Command is null)return Task.FromResult<ActionResult>(BadRequest());
        return ConfigurationExecute(async()=>{submission=CompanyInventorySourceReview.Copy(submission);submission.Command.Require();var original=await Authority(true,submission.Command);
            var result=await accounts.ConfigureCompanyAccountAsync(submission,original.Guard);await original.Guard();
            if(CompanyGLAccountConfigurationProof.Require(result).Command!=submission.Command)throw new InvalidOperationException("The original configuration outcome is unconfirmed.");return Ok(result);});
    }
    [HttpGet("configuration/{id}/request")]
    public Task<ActionResult> RecoverConfiguration(string id,[FromQuery]string book,[FromQuery]string currency)=>ConfigurationExecute(async()=>
    {
        var original=await Authority(true);var command=await accounts.GetCompanyAccountConfigurationAsync(original.Company,original.Actor,book,currency,id,original.Guard);await original.Guard();
        if(command is null)return NotFound();if(command.BindingRevision!=original.Binding)throw new InvalidOperationException("Return to the original configuration binding to reconcile.");return Ok(command);
    });
    private async Task<ActionResult> ConfigurationExecute(Func<Task<ActionResult>> action)
    {
        try{return await action();}catch(UnauthorizedAccessException){return Forbid();}
        catch(Beep.OilandGas.Models.Core.Refusals.RefusalException denied) when(denied.Kind==Beep.OilandGas.Models.Core.Refusals.RefusalKind.Forbidden){return Forbid();}
        catch(Exception unavailable) when(unavailable is InvalidOperationException or AggregateException or ArgumentException or System.Text.Json.JsonException)
        {logger.LogWarning(unavailable,"The original company ledger configuration request or outcome is unconfirmed.");return Conflict(new{error="Account configuration is unconfirmed. Retain the exact original request, return to its company/account/binding and reconcile before another configuration command."});}
    }
    [HttpGet("mappings")]
    public async Task<ActionResult<CompanyGLMappingReview>> ReadMappings([FromQuery]string book,[FromQuery]string currency,[FromQuery]string date,[FromServices]AccountMappingService mappings)
    {
        if(string.IsNullOrWhiteSpace(book) || string.IsNullOrWhiteSpace(currency) || !DateTime.TryParseExact(date,"yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture,System.Globalization.DateTimeStyles.None,out var asOf))return BadRequest();
        try
        {
            var original=await Authority();var review=await mappings.GetCompanyMappingsAsync(original.Company,original.Actor,book,currency,asOf,original.Guard);
            await original.Guard();review.Require(original.Company,original.Actor,book,currency,asOf);return Ok(review);
        }
        catch(UnauthorizedAccessException){return Forbid();}
        catch(Beep.OilandGas.Models.Core.Refusals.RefusalException denied) when(denied.Kind==Beep.OilandGas.Models.Core.Refusals.RefusalKind.Forbidden){return Forbid();}
        catch(Exception unavailable) when(unavailable is InvalidOperationException or AggregateException or ArgumentException)
        {logger.LogWarning(unavailable,"Company ledger account mapping evidence is unconfirmed.");return Conflict(new{error="Company mapping evidence is unavailable or changed. Review the original book, currency, company calendar date and exact configured accounts before journal preparation."});}
    }
    [HttpGet]
    public async Task<ActionResult<CompanyGLAccountReview>> Read([FromQuery]string book,[FromQuery]string currency)
    {
        if(string.IsNullOrWhiteSpace(book) || string.IsNullOrWhiteSpace(currency))return BadRequest();
        try
        {
            var original=await Authority();var review=await accounts.GetCompanyAccountsAsync(original.Company,original.Actor,book,currency,original.Guard);
            await original.Guard();review.Require(original.Company,original.Actor,book,currency);return Ok(review);
        }
        catch(UnauthorizedAccessException){return Forbid();}
        catch(Beep.OilandGas.Models.Core.Refusals.RefusalException denied) when(denied.Kind==Beep.OilandGas.Models.Core.Refusals.RefusalKind.Forbidden){return Forbid();}
        catch(Exception unavailable) when(unavailable is InvalidOperationException or AggregateException or ArgumentException)
        {logger.LogWarning(unavailable,"Company ledger account configuration is unconfirmed.");return Conflict(new{error="Company ledger account configuration is unavailable or changed. Review the original company, book, currency and configured accounts before posting."});}
    }
}
