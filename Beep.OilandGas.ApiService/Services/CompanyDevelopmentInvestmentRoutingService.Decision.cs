using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DevelopmentPlanning.Models;
using System.Security.Claims;
using System.Text.Json;

namespace Beep.OilandGas.ApiService.Services;

public sealed partial class CompanyDevelopmentInvestmentRoutingService
{
    private async Task<string> NextInvestmentRecipientsAsync(InvestmentAccess access,DevelopmentInvestmentStageDecision command)
    {
        await access.Check();if(command.Terminal())return "terminal";
        var next=access.Original.Option().Stages[command.Review.StageNumber];var scope=command.Scope();
        var candidates=await accounts.GetCompanyRoleRecipientsAsync(scope.Company,next.AssignedRole)??throw new InvalidOperationException("The current next investment recipients are unavailable.");
        if(candidates.Length>256 || candidates.Any(candidate=>candidate is null) || candidates.Select(candidate=>candidate.UserId).Distinct(StringComparer.Ordinal).Count()!=candidates.Length)
            throw new InvalidOperationException("The bounded current next-recipient population is ambiguous.");
        candidates=candidates.Where(candidate=>next.Recipients.Any(original=>original==candidate)).OrderBy(candidate=>candidate.UserId,StringComparer.Ordinal).ToArray();
        IReadOnlyList<AppUserAssetAccess> grants=candidates.Length==0?[]:await assets.ReadInvestmentReviewRecipientFieldGrantsAsync(scope,candidates.Select(candidate=>candidate.UserId).ToArray());
        if(grants is null || grants.Any(row=>row is null) || grants.Select(row=>row.Id).Distinct(StringComparer.Ordinal).Count()!=grants.Count)
            throw new InvalidOperationException("The original next-stage FIELD authority is ambiguous.");
        var snapshots=new List<object>();var eligible=new List<RepositoryCompanyRoleRecipient>();
        foreach(var candidate in candidates)
        {
            var current=await accounts.GetCompanyAccessAsync(candidate.UserId,scope.Company);
            if(current?.Scope!=scope.Company || !current.Roles.Contains(next.AssignedRole,StringComparer.Ordinal) || !RecipientPermissions.All(permission=>current.Permissions.Contains(permission,StringComparer.Ordinal)))continue;
            var workspace=await workspaces.ReviewWorkspacesAsync(candidate.UserId,scope.Company,accounts);var profile=await workspaces.GetAsync(candidate.UserId,scope.Company);
            var rights=grants.Where(row=>row.UserId==candidate.UserId && row.DatabaseScope==scope.CoreDatabase.Fingerprint && row.OrganizationId==scope.Company.BusinessAssociateId &&
                row.AssetType=="FIELD" && row.AssetId==scope.FieldId && row.FacilityType is null && row.IsActive && row.AccessLevel is "WRITE" or "DELETE").OrderBy(row=>row.Id,StringComparer.Ordinal).ToArray();
            if(profile?.PersonaCode!=candidate.PersonaCode || rights.Length==0 || !workspace.Personas.Any(persona=>persona.Code==candidate.PersonaCode))continue;
            eligible.Add(candidate);snapshots.Add(new{candidate,Roles=current.Roles.Order(StringComparer.Ordinal),Permissions=current.Permissions.Order(StringComparer.Ordinal),workspace.Revision,Profile=profile,Grants=rights});
        }
        if(access.Original.Workflow.Steps[command.Review.StageNumber].PersonaCodes.Any(persona=>!eligible.Any(candidate=>candidate.PersonaCode==persona)))
            throw new InvalidOperationException("Every original next-stage workspace needs a current approved independent role recipient and exact FIELD write authority before this stage can advance.");
        await access.Check();return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(snapshots));
    }
    public async Task<DevelopmentInvestmentStageRecoveryInput> ReadOwnStageOriginalAsync(ClaimsPrincipal user,string instanceId)
    {
        var access=await CaptureInvestmentAccessAsync(user,instanceId,false);var scope=access.Original.Scope();
        string? nativeRevision=null;
        async Task<DevelopmentInvestmentStageDecision?> Read()
        {
            await access.Check();var native=await access.Native.ReadRecordedInvestmentHandoffAsync(access.Company,scope.FieldId,access.Original.Review.Source.AfeId,access.Check)
                ??throw new InvalidOperationException("The complete original investment assignment is unavailable.");
            if(native.Original.Revision()!=access.Original.Revision())throw new InvalidOperationException("The original assignment changed during recovery-input retrieval.");
            var revision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(native.Process));
            if(nativeRevision is not null && nativeRevision!=revision)throw new InvalidOperationException("The native recovery graph changed during retrieval.");nativeRevision=revision;
            return Beep.OilandGas.LifeCycle.Services.Processes.PPDMProcessService.OwnInvestmentStageOriginal(native.Process,access.Actor);
        }
        var original=await Read();
        if(original is not null)
        {
            original.Require();await DecideAssignedStageAsync(user,original,true);
            if((await Read())?.Revision()!=original.Revision())throw new InvalidOperationException("The original recovery input changed during retrieval.");
        }
        else
        {
            var source=await access.ReadSource(access.Original.Review.Source.AfeId);access.Original.RequireSource(source,false);
            await access.ReadFunding(source);if(await Read() is not null)throw new InvalidOperationException("An original decision appeared during retrieval. Reload its outcome.");
        }
        await access.Confirm();await access.Check();var result=new DevelopmentInvestmentStageRecoveryInput(access.Company,access.Actor,instanceId,original,nativeRevision!);result.Require(access.Company,access.Actor,instanceId);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<DevelopmentInvestmentStageReceipt> DecideAssignedStageAsync(ClaimsPrincipal user,DevelopmentInvestmentStageDecision input,bool recover=false)
    {
        input.Require();var command=CompanyInventorySourceReview.Copy(input);var access=await CaptureInvestmentAccessAsync(user,command.Review.InstanceId,!recover,command);
        var processId=command.Review.Source.Current.Source.Review.InstanceId;
        async Task<DevelopmentInvestmentReview> Source()
        {
            await access.Check();var source=await access.ReadSource(command.Review.Source.AfeId);access.Original.RequireSource(source,false);
            if(await access.ReadFunding(source)!=command.Review.FundingRationale)throw new InvalidOperationException("The original funding rationale differs from the retained assigned decision.");
            await access.Check();return source;
        }
        if(recover)return await access.Native.RecoverInvestmentStageAsync(command,access.Check,Source);
        if(access.Process.Status!=Beep.OilandGas.Models.Processes.ProcessStatus.IN_PROGRESS || access.Process.CurrentStepId!=command.Review.StepId)
        {
            var recorded=await access.Native.RecoverInvestmentStageAsync(command,access.Check,Source);
            if(recorded.Status=="RECORDED")return recorded;
            throw new InvalidOperationException("The original stage is no longer current. Recover its retained outcome before continuing.");
        }
        // Reservation is also exact original readback for a current stage. A preceding recovery
        // transaction would duplicate its complete native/source admission without owning a write.
        var next=await NextInvestmentRecipientsAsync(access,command);
        async Task WriteGuard()
        {
            if(await NextInvestmentRecipientsAsync(access,command)!=next)throw new UnauthorizedAccessException("The current original next-stage receiving authority changed.");
        }
        var reserved=await access.Native.ReserveInvestmentStageAsync(command,WriteGuard,Source);reserved.Require(command);
        if(reserved.Status=="RECORDED")return reserved;
        if(reserved.Status is not ("RESERVED" or "FINANCIAL_RECORDED"))throw new InvalidOperationException("The exact original lifecycle reservation was not confirmed. Recover its retained input.");
        if(command.Terminal())
        {
            await WriteGuard();var financial=await finance.DecideInvestmentAsync(user,command.FinancialCommand());financial.Require(command.FinancialCommand());
            if(financial.Recorded is null)throw new InvalidOperationException("The exact final financial outcome is unconfirmed. Recover the original reserved stage.");
            command.RequireFinancial(DevelopmentInvestmentCommand.Facts(financial.Recorded));await WriteGuard();
        }
        var receipt=await access.Native.CompleteInvestmentStageAsync(command,WriteGuard,Source);receipt.Require(command);
        if(receipt.Status!="RECORDED")throw new InvalidOperationException("The original financial/stage outcome is incomplete. Recover its retained input before continuing.");
        await access.Confirm();await access.Check();return receipt;
    }
}
