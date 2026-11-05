using System.Security.Claims;
using System.Text.Json;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Services;
public sealed partial class CompanyAccountingProcessReads
{
    private async Task<Func<Task>> DeliveryCorrectionAuthority(ClaimsPrincipal user,CompanyDeliveryCorrectionReview review)
    {
        var original=await context.CaptureProcessAsync(user);var roles=original.Access.Roles.Order(StringComparer.Ordinal).ToArray();var grants=original.Access.Permissions.Order(StringComparer.Ordinal).ToArray();
        async Task Guard()
        {
            var current=await context.CaptureProcessAsync(user);
            if(user.Identity?.IsAuthenticated!=true || current.Access.Scope!=review.Company || current.Actor!=review.ActorUserId || current.Binding!=review.BindingRevision || !ReferenceEquals(current.Process,original.Process) ||
                current.Access.Roles is null || current.Access.Permissions is null || !current.Access.Roles.Any(user.IsInRole) || !current.Access.Roles.Order(StringComparer.Ordinal).SequenceEqual(roles) || !current.Access.Permissions.Order(StringComparer.Ordinal).SequenceEqual(grants) ||
                !current.Access.Permissions.Contains("Accounting.View",StringComparer.Ordinal) || !current.Access.Permissions.Contains("ProductionAccounting.Adjust",StringComparer.Ordinal))throw new UnauthorizedAccessException("The original receiving correction authority changed.");
        }
        await Guard();return Guard;
    }
    private Task<Beep.OilandGas.Models.Processes.ProcessInstance> RequireDeliveryCorrectionProcess(ClaimsPrincipal user,CompanyDeliveryCorrectionReview review)
        =>ClosedDeliveryProcess(user,review.InstanceId,review.Company,review.ActorUserId,review.BindingRevision,review.ProcessRevision,review.Previous);
    public async Task<CompanyDeliveryCorrectionReview> ReviewDeliveryCorrectionAsync(ClaimsPrincipal user,string id)
    {
        if(commercial is null)throw new InvalidOperationException("The canonical receiving reader is required.");
        var original=await context.CaptureProcessAsync(user);var process=await ClosedDeliveryProcess(user,id,original.Access.Scope,original.Actor,original.Binding);var start=NativeDecisionStart(process,"RUN_TICKET");
        var current=await commercial.ReadProcessDeliveryAsync(user,process.EntityId,start.SourceRevision);
        if(current.IsHistorical || current.Recorded is null)throw new InvalidOperationException("The exact current rejected receiving version is required. Recover an existing original correction before another.");
        var review=new CompanyDeliveryCorrectionReview(original.Access.Scope,original.Actor,id,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(process)),original.Binding,
            CompanyInventorySourceReview.Copy(current.Source.Ticket),CompanyInventorySourceReview.Copy(current.Recorded),RevenueDeliveryAcceptanceCorrection.NormalizeSource(current.Source,current.Recorded));review.Require(original.Access.Scope,original.Actor,id);
        var authority=await DeliveryCorrectionAuthority(user,review);await RequireDeliveryCorrectionProcess(user,review);
        var closing=await commercial.ReadProcessDeliveryAsync(user,process.EntityId,start.SourceRevision);await authority();
        if(JsonSerializer.Serialize(closing)!=JsonSerializer.Serialize(current))throw new InvalidOperationException("The complete original rejected receiving source changed during correction review.");return CompanyInventorySourceReview.Copy(review);
    }
    public async Task<DELIVERY_ACCEPTANCE_INTENT> SaveDeliveryCorrectionIntentAsync(ClaimsPrincipal user,string id,CompanyDeliveryCorrectionSubmission command)
    {
        command=CompanyInventorySourceReview.Copy(command);command.Require();var review=command.Review;
        if(id!=review.InstanceId)throw new ArgumentException("The correction route differs from its original rejected process.");if(commercial is null)throw new InvalidOperationException("The canonical receiving owner is required.");
        var authority=await DeliveryCorrectionAuthority(user,review);await RequireDeliveryCorrectionProcess(user,review);
        // Full LIFECYCLE reads stay outside owned PRODUCTION transactions; only pure authority/configuration guards enter them.
        var saved=await commercial.SaveDeliveryCorrectionIntentAsync(user,command,authority);await RequireDeliveryCorrectionProcess(user,review);await authority();
        if(JsonSerializer.Serialize(saved.Restore(review.Company,review.ActorUserId,saved.SCOPE_KEY).Correction)!=JsonSerializer.Serialize(command))throw new InvalidOperationException("The exact original receiving correction is not durably saved.");return CompanyInventorySourceReview.Copy(saved);
    }
    public async Task<RevenueDeliveryAcceptanceRecord> SubmitDeliveryCorrectionAsync(ClaimsPrincipal user,string id,CompanyDeliveryCorrectionSubmission command)
    {
        command=CompanyInventorySourceReview.Copy(command);command.Require();var review=command.Review;if(id!=review.InstanceId)throw new ArgumentException("The correction route differs from its original rejected process.");
        if(commercial is null)throw new InvalidOperationException("The canonical receiving owner is required.");var authority=await DeliveryCorrectionAuthority(user,review);
        await SaveDeliveryCorrectionIntentAsync(user,id,command);await RequireDeliveryCorrectionProcess(user,review);await authority();
        var result=await commercial.CorrectDeliveryAcceptanceAsync(user,command,authority);await RequireDeliveryCorrectionProcess(user,review);await authority();
        result.Require();if(JsonSerializer.Serialize(result.Preparation)!=JsonSerializer.Serialize(command.Correction.Preparation))throw new InvalidOperationException("The recorded receiving correction differs from its original intent.");return CompanyInventorySourceReview.Copy(result);
    }
}
