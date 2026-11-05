using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Repositories.WELL;
using Microsoft.Extensions.Options;
using TheTechIdea.Beep.Editor;
namespace Beep.OilandGas.ApiService.Services;

/// <summary>Binds the existing native well owner to the already authorized CORE and configured company date zone.</summary>
public sealed class CompanyMaintenanceWellEvidenceReader(IDMEEditor editor,ICommonColumnHandler columns,IPPDM39DefaultsRepository defaults,IPPDMMetadataRepository metadata,CompanyPPDMRowAccessPolicy policy,IOptions<PPDMFieldEligibilityConfiguration> configuration,ICompanyMaintenanceQualifiedAssetEvidenceOwner? qualifiedAssets=null):ICompanyMaintenanceAssetReturnOwner
{
    private string Zone(FieldSelectionReview context)
    {
        var rules=configuration.Value.Companies.Where(row=>row.TenantId==context.Company.TenantId && row.BusinessAssociateId==context.Company.BusinessAssociateId).ToArray();
        if(rules.Length!=1 || string.IsNullOrWhiteSpace(rules[0].TimeZoneId))throw new InvalidOperationException("One explicit company evidence time zone is required for native well return.");
        return rules[0].TimeZoneId;
    }
    private WellServices Owner(FieldSelectionReview context,string zone)=>qualifiedAssets is ICompanyMaintenanceQualifiedReturnParticipant participant
        ?new(editor,columns,defaults,metadata,context.Database.ConnectionName,policy,zone,participant)
        :new(editor,columns,defaults,metadata,context.Database.ConnectionName,policy,zone);
    private void RequireZone(FieldSelectionReview context,string zone)
    {if(Zone(context)!=zone)throw new InvalidOperationException("Configured company evidence date interpretation changed during native well review.");}
    public async Task<CompanyMaintenanceAssetReturnFacts?> ReadMaintenanceAssetReturnFactsAsync(FieldSelectionReview context,CompanyWorkOrderCompletionQuery query,Func<Task> authority)
    {
        query.Require();await authority();var zone=Zone(context);async Task Guard(){await authority();RequireZone(context,zone);}
        var result=await Owner(context,zone).ReadMaintenanceAssetReturnFactsAsync(context,query,Guard);await Guard();return result;
    }
    public async Task<CompanyMaintenanceWellEvidence> ReadMaintenanceWellEvidenceAsync(FieldSelectionReview context,string wellId,Func<Task> authority)
    {
        await authority();var zone=Zone(context);async Task Guard(){await authority();RequireZone(context,zone);}
        var result=await Owner(context,zone).ReadMaintenanceWellEvidenceAsync(context,wellId,Guard);await Guard();return result;
    }
    public async Task<CompanyMaintenanceAssetReturnRecovery> RecordMaintenanceAssetReturnAsync(CompanyMaintenanceAssetReturnCommand command,Func<string?,Task> authority,Func<bool,Task<CompanyWorkOrderReturnReview>> readPhysical,Func<bool,Task<CompanyMaintenanceClosureFacts?>> readClosure)
    {
        command.Require();var context=command.Review.Assets.Context;await authority(null);var zone=Zone(context);
        async Task Guard(string? role){await authority(role);RequireZone(context,zone);}
        var result=await Owner(context,zone).RecordMaintenanceAssetReturnAsync(command,Guard,readPhysical,readClosure);await Guard(command.Review.Physical.Assignment.Stage.AssignedRole);return result;
    }
    public async Task<CompanyMaintenanceAssetReturnRecovery> RecoverMaintenanceAssetReturnAsync(FieldSelectionReview context,CompanyMaintenanceAssetReturnReference reference,Func<string?,Task> authority)
    {
        reference.Require();await authority(null);var zone=Zone(context);async Task Guard(string? role){await authority(role);RequireZone(context,zone);}
        var result=await Owner(context,zone).RecoverMaintenanceAssetReturnAsync(context,reference,Guard);await Guard(result.Command.Review.Physical.Assignment.Stage.AssignedRole);return result;
    }
}
