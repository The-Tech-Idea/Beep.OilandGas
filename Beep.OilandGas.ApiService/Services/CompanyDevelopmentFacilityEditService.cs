using System.Security.Claims;
using Beep.OilandGas.Models.Core.Refusals;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Services;

/// <summary>Admits the original facility command through current app authority and its native owner.</summary>
public sealed class CompanyDevelopmentFacilityEditService(DevelopmentFacilityReadAuthority authority,ModuleConnectionResolver modules,Func<FieldSelectionReview,ICompanyFacilityEditOwner> ownerFactory)
{
    public async Task<DevelopmentFacilityEditReceipt> ExecuteAsync(ClaimsPrincipal user,DevelopmentFacilityEdit input,bool recover=false)
    {
        ArgumentNullException.ThrowIfNull(input);var original=CompanyInventorySourceReview.Copy(input);original.Require();var reviewed=original.Review;
        var scope=recover?await authority.CaptureRecoveryAsync(user,reviewed.Original.FACILITY_ID,reviewed.Original.FACILITY_TYPE):
            await authority.CaptureEditAsync(user,reviewed.Original.FACILITY_ID,reviewed.Original.FACILITY_TYPE,reviewed.EditorRole);
        var current=scope.Context;var prior=reviewed.Context;
        if(current.Company!=prior.Company || current.ActorUserId!=prior.ActorUserId || current.Database!=prior.Database || current.FieldId!=prior.FieldId ||
            !recover && (current!=prior || scope.AuthorityRevision!=reviewed.AuthorityRevision))
            throw RefusalException.Forbidden("Return to the original company, account and FIELD before this facility edit.");
        var core=await modules.ResolveAsync("PPDM_CORE");var facility=await modules.ResolveAsync("FACILITY");
        if(core!=current.Database.ConnectionName || facility!=core)throw RefusalException.Conflict("The facility edit and CORE must use the same reviewed datasource.");
        var binding=await modules.GetMigrationBindingFingerprintAsync(["PPDM_CORE","FACILITY"],core);
        async Task Guard()
        {
            await scope.CheckCurrentAsync();
            if(await modules.ResolveAsync("PPDM_CORE")!=core || await modules.ResolveAsync("FACILITY")!=core || await modules.GetMigrationBindingFingerprintAsync(["PPDM_CORE","FACILITY"],core)!=binding)
                throw RefusalException.Conflict("The original facility datasource binding changed.");
        }
        await scope.ConfirmAsync();await Guard();var owner=ownerFactory(current)??throw new InvalidOperationException("The native facility edit owner is unavailable.");
        var entry=recover?await owner.RecoverFacilityEditAsync(original,Guard):await owner.RecordFacilityEditAsync(original,Guard);
        var saved=entry.Restore(prior.Company,prior.ActorUserId);
        if(saved.Command.Revision()!=original.Revision())throw new InvalidOperationException("The original facility edit outcome differs from the retained input.");
        await scope.ConfirmAsync();await Guard();var result=new DevelopmentFacilityEditReceipt(original.Revision(),saved.Result,DateTimeOffset.UtcNow);result.Require(original);return result;
    }
}

public sealed class CompanyDevelopmentFacilityEquipmentService(DevelopmentFacilityReadAuthority authority,ModuleConnectionResolver modules,Func<FieldSelectionReview,ICompanyFacilityEquipmentReader> readerFactory)
{
    public async Task<IReadOnlyList<DevelopmentFacilityEquipmentItem>> ReadAsync(ClaimsPrincipal user,string facilityId,string facilityType)
    {
        var scope=await authority.CaptureRecoveryAsync(user,facilityId,facilityType);var context=scope.Context;
        var core=await modules.ResolveAsync("PPDM_CORE");var facility=await modules.ResolveAsync("FACILITY");
        if(core!=context.Database.ConnectionName || facility!=core)throw RefusalException.Conflict("The facility and CORE must use the same reviewed datasource.");
        var binding=await modules.GetMigrationBindingFingerprintAsync(["PPDM_CORE","FACILITY"],core);
        async Task Guard()
        {
            await scope.ConfirmAsync();
            if(await modules.ResolveAsync("PPDM_CORE")!=core || await modules.ResolveAsync("FACILITY")!=core || await modules.GetMigrationBindingFingerprintAsync(["PPDM_CORE","FACILITY"],core)!=binding)throw RefusalException.Conflict("The original facility datasource binding changed.");
        }
        var reader=readerFactory(context)??throw new InvalidOperationException("The native facility equipment reader is unavailable.");
        try
        {
            await Guard();var rows=await reader.ReadDevelopmentFacilityEquipmentAsync(context,facilityId,facilityType,Guard);await Guard();
            return rows.Select(row=>new DevelopmentFacilityEquipmentItem(row.EQUIPMENT_ID,row.INSTALL_OBS_NO,row.SOURCE)).ToArray();
        }
        catch(UnauthorizedAccessException){throw RefusalException.Forbidden("Current facility and equipment access is required.");}
    }
}

public sealed class CompanyEquipmentInstallationReviewService(DevelopmentFacilityReadAuthority authority,ModuleConnectionResolver modules,Func<FieldSelectionReview,ICompanyEquipmentInstallationReader> readerFactory)
{
    public async Task<DevelopmentEquipmentInstallationReview> ReviewAsync(ClaimsPrincipal user,DevelopmentEquipmentInstallationProposal proposal)
    {
        try{if(proposal is null)throw new InvalidOperationException();proposal.Require();}
        catch(InvalidOperationException){throw RefusalException.Invalid("A valid equipment installation proposal is required.");}
        var scope=await authority.CaptureEditAsync(user,proposal.FacilityId,proposal.FacilityType,proposal.EditorRole);
        if(scope.Context!=proposal.Context)throw RefusalException.Forbidden("Return to the original company and FIELD for this proposal.");
        var core=await modules.ResolveAsync("PPDM_CORE");var facility=await modules.ResolveAsync("FACILITY");
        if(core!=scope.Context.Database.ConnectionName || facility!=core)throw RefusalException.Conflict("Facility and CORE must use the same reviewed datasource.");
        var binding=await modules.GetMigrationBindingFingerprintAsync(["PPDM_CORE","FACILITY"],core);
        async Task Guard(){await scope.ConfirmAsync();if(await modules.ResolveAsync("PPDM_CORE")!=core || await modules.ResolveAsync("FACILITY")!=core || await modules.GetMigrationBindingFingerprintAsync(["PPDM_CORE","FACILITY"],core)!=binding)throw RefusalException.Conflict("The original facility datasource binding changed.");}
        try
        {
            var reader=readerFactory(scope.Context)??throw new InvalidOperationException("The native equipment installation reader is unavailable.");await Guard();var source=await reader.ReviewDevelopmentEquipmentInstallationAsync(proposal,Guard);await Guard();
            var result=new DevelopmentEquipmentInstallationReview(proposal,source,scope.AuthorityRevision,DateTimeOffset.UtcNow);result.Require();return result;
        }
        catch(UnauthorizedAccessException){throw RefusalException.Forbidden("Current facility and equipment WRITE access is required.");}
    }
}

public sealed class CompanyEquipmentInstallationCommandService(DevelopmentFacilityReadAuthority authority,ModuleConnectionResolver modules,Func<FieldSelectionReview,ICompanyEquipmentInstallationOwner> ownerFactory)
{
    public async Task<DevelopmentEquipmentInstallationReceipt> ExecuteAsync(ClaimsPrincipal user,DevelopmentEquipmentInstallation input,bool recover=false)
    {
        var original=CompanyInventorySourceReview.Copy(input);original.Require();var proposal=original.Review.Proposal;var prior=proposal.Context;
        var scope=recover?await authority.CaptureRecoveryAsync(user,proposal.FacilityId,proposal.FacilityType):await authority.CaptureEditAsync(user,proposal.FacilityId,proposal.FacilityType,proposal.EditorRole);var current=scope.Context;
        if(current.Company!=prior.Company || current.ActorUserId!=prior.ActorUserId || current.Database!=prior.Database || current.FieldId!=prior.FieldId || !recover && (current!=prior || scope.AuthorityRevision!=original.Review.AuthorityRevision))throw RefusalException.Forbidden("Return to the original company, account and FIELD before this equipment installation.");
        var core=await modules.ResolveAsync("PPDM_CORE");var facility=await modules.ResolveAsync("FACILITY");if(core!=current.Database.ConnectionName || facility!=core)throw RefusalException.Conflict("Facility and CORE must use the same reviewed datasource.");
        var binding=await modules.GetMigrationBindingFingerprintAsync(["PPDM_CORE","FACILITY"],core);
        async Task Guard(){await scope.ConfirmAsync();if(await modules.ResolveAsync("PPDM_CORE")!=core || await modules.ResolveAsync("FACILITY")!=core || await modules.GetMigrationBindingFingerprintAsync(["PPDM_CORE","FACILITY"],core)!=binding)throw RefusalException.Conflict("The original installation datasource binding changed.");}
        try
        {
            await Guard();var owner=ownerFactory(current)??throw new InvalidOperationException("The installation owner is unavailable.");var entry=recover?await owner.RecoverEquipmentInstallationAsync(original,Guard):await owner.RecordEquipmentInstallationAsync(original,Guard);var saved=entry.Restore(prior.Company,prior.ActorUserId);
            if(saved.Command.Revision()!=original.Revision())throw new InvalidOperationException("The installation outcome differs from the retained original.");await Guard();var receipt=new DevelopmentEquipmentInstallationReceipt(original.Revision(),saved.Installation,DateTimeOffset.UtcNow);receipt.Require(original);return receipt;
        }
        catch(UnauthorizedAccessException){throw RefusalException.Forbidden("Current original facility and equipment access is required.");}
    }
}
