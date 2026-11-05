using System.Security.Claims;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

public sealed class CompanyDevelopmentFacilityCreationService(DevelopmentFacilityReadAuthority authority,ModuleConnectionResolver modules,Func<FieldSelectionReview,ICompanyFacilityCreationReader> readerFactory,Func<FieldSelectionReview,ICompanyFacilityCreationOwner> ownerFactory,IUserAssetAccessStore grants)
{
    public async Task<DevelopmentFacilityCreationReceipt> ExecuteAsync(ClaimsPrincipal user,DevelopmentFacilityCreation input,bool recover=false)
    {
        ArgumentNullException.ThrowIfNull(input);var original=CompanyInventorySourceReview.Copy(input);original.Require();var prior=original.Review.Draft.Context;
        var scope=recover?await authority.CaptureAsync(user):await authority.CaptureCreateAsync(user,original.Review.Draft.CreatorRole);var current=scope.Context;
        if(current.Company!=prior.Company || current.ActorUserId!=prior.ActorUserId || current.Database!=prior.Database || current.FieldId!=prior.FieldId || !recover && (current!=prior || scope.AuthorityRevision!=original.Review.AuthorityRevision))
            throw RefusalException.Forbidden("Return to the original company, account and FIELD before this facility creation.");
        var core=await modules.ResolveAsync("PPDM_CORE");var facility=await modules.ResolveAsync("FACILITY");
        if(core!=current.Database.ConnectionName || facility!=core)throw RefusalException.Conflict("The facility and CORE must use the same reviewed datasource.");
        var binding=await modules.GetMigrationBindingFingerprintAsync(["PPDM_CORE","FACILITY"],core);
        async Task Guard()
        {
            await scope.CheckCurrentAsync();
            if(await modules.ResolveAsync("PPDM_CORE")!=core || await modules.ResolveAsync("FACILITY")!=core || await modules.GetMigrationBindingFingerprintAsync(["PPDM_CORE","FACILITY"],core)!=binding)throw RefusalException.Conflict("The original facility datasource binding changed.");
            if(!recover)
            {
                var rows=await grants.ReadAsync(current.ActorUserId,current.Database.Fingerprint,current.Company.BusinessAssociateId);
                if(!rows.Any(row=>row.UserId==current.ActorUserId && row.DatabaseScope==current.Database.Fingerprint && row.OrganizationId==current.Company.BusinessAssociateId && row.AssetType=="FIELD" && row.AssetId==current.FieldId && row.FacilityType is null && row.Inherit && row.IsActive && row.AccessLevel is "WRITE" or "DELETE"))
                    throw RefusalException.Forbidden("An inherited FIELD write grant is required to retain access to the new facility.");
            }
        }
        await scope.ConfirmAsync();await Guard();var owner=ownerFactory(current)??throw new InvalidOperationException("The facility creation owner is unavailable.");
        var entry=recover?await owner.RecoverFacilityCreationAsync(original,Guard):await owner.RecordFacilityCreationAsync(original,Guard);var saved=entry.Restore(prior.Company,prior.ActorUserId);
        if(saved.Command.Revision()!=original.Revision())throw new InvalidOperationException("The recorded facility creation differs from the original.");
        await scope.ConfirmAsync();await Guard();var receipt=new DevelopmentFacilityCreationReceipt(original.Revision(),saved.Facility,saved.Link,DateTimeOffset.UtcNow);receipt.Require(original);return receipt;
    }
    public async Task<DevelopmentFacilityCreationReview> ReviewAsync(ClaimsPrincipal user,DevelopmentFacilityCreationDraft input)
    {
        ArgumentNullException.ThrowIfNull(input);var draft=CompanyInventorySourceReview.Copy(input);draft.Require();
        var scope=await authority.CaptureCreateAsync(user,draft.CreatorRole);
        if(scope.Context!=draft.Context)throw RefusalException.Forbidden("Return to the original company and FIELD before reviewing this proposal.");
        var core=await modules.ResolveAsync("PPDM_CORE");var facility=await modules.ResolveAsync("FACILITY");
        if(core!=scope.Context.Database.ConnectionName || facility!=core)throw RefusalException.Conflict("The facility and CORE must use the same reviewed datasource.");
        var binding=await modules.GetMigrationBindingFingerprintAsync(["PPDM_CORE","FACILITY"],core);
        async Task Guard()
        {
            await scope.CheckCurrentAsync();
            if(await modules.ResolveAsync("PPDM_CORE")!=core || await modules.ResolveAsync("FACILITY")!=core || await modules.GetMigrationBindingFingerprintAsync(["PPDM_CORE","FACILITY"],core)!=binding)
                throw RefusalException.Conflict("The original facility datasource binding changed.");
        }
        await scope.ConfirmAsync();await Guard();var reader=readerFactory(scope.Context)??throw new InvalidOperationException("The facility creation source reader is unavailable.");
        var source=await reader.ReadFacilityCreationSourceAsync(draft,Guard);await scope.ConfirmAsync();await Guard();
        var review=new DevelopmentFacilityCreationReview(draft,source.TypeSource,source.OperatorRevision,scope.AuthorityRevision,"",DateTimeOffset.UtcNow);review=review with{Revision=review.ContentRevision()};review.Require();return review;
    }
}
