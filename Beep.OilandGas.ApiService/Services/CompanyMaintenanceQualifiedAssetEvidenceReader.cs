using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.ProductionOperations.Services;
using Microsoft.Extensions.Options;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
namespace Beep.OilandGas.ApiService.Services;

/// <summary>Binds the active facility owner to the current authorized CORE and recorded company date zone.</summary>
public sealed class CompanyMaintenanceQualifiedAssetEvidenceReader(IDMEEditor editor,ICommonColumnHandler columns,IPPDM39DefaultsRepository defaults,IPPDMMetadataRepository metadata,CompanyPPDMRowAccessPolicy policy,IOptions<PPDMFieldEligibilityConfiguration> configuration,IFailureReporter failures,CompanyMaintenanceReturnPolicyService returnPolicies):ICompanyMaintenanceQualifiedAssetEvidenceOwner,ICompanyMaintenanceQualifiedWorkAssetOwner,ICompanyMaintenanceQualifiedReturnParticipant
{
    private string Zone(FieldSelectionReview context)
    {
        var rows=configuration.Value.Companies.Where(row=>row.TenantId==context.Company.TenantId && row.BusinessAssociateId==context.Company.BusinessAssociateId).ToArray();
        if(rows.Length!=1 || string.IsNullOrWhiteSpace(rows[0].TimeZoneId))throw new InvalidOperationException("One explicit company evidence time zone is required for qualified native assets.");return rows[0].TimeZoneId;
    }
    private void RequireReturnContext(CompanyMaintenanceAssetReturnCommand command,FieldSelectionReview reader)
    {
        var original=command.Review.Assets;
        FieldSelectionReviewValidator.RequireReview(reader,reader.Company,reader.ActorUserId);
        if(original.QualifiedAssets is not {} assets || reader.Company!=original.Context.Company || reader.Database!=original.Context.Database || reader.FieldId!=original.Context.FieldId || Zone(reader)!=assets.DateTimeZoneId)
            throw new UnauthorizedAccessException("Original qualified company/FIELD/CORE and current declared evidence time zone are required.");
    }
    private FacilityManagementService ReturnOwner(CompanyMaintenanceAssetReturnCommand command,FieldSelectionReview reader)
    {
        command.Require();RequireReturnContext(command,reader);
        return new(editor,columns,defaults,metadata,failures,reader.Database.ConnectionName,maintenanceAssetAccess:policy,maintenanceEvidenceZone:command.Review.Assets.QualifiedAssets!.DateTimeZoneId);
    }
    private async Task ReturnGuard(CompanyMaintenanceAssetReturnCommand command,FieldSelectionReview reader,bool requireWrite,Func<Task> authority)
    {
        await authority();RequireReturnContext(command,reader);
        if(requireWrite)
        {
            if(reader.ActorUserId!=command.Review.Assets.Context.ActorUserId)throw new UnauthorizedAccessException("The original return maker is required for private command handling.");
            var roles=await returnPolicies.ReadEvidenceReturnRolesAsync(reader);
            if(!roles.Contains(command.Review.Physical.Assignment.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("The current app-owned configured return role is required.");
        }
        RequireReturnContext(command,reader);
    }
    public void RequireMaintenanceQualifiedReturnProof(CompanyMaintenanceAssetReturnCommand command,CompanyMaintenanceQualifiedReturnProof proof)=>
        ReturnOwner(command,command.Review.Assets.Context).RequireMaintenanceQualifiedReturnProof(command,proof);
    public async Task<CompanyMaintenanceQualifiedReturnProof> PrepareMaintenanceQualifiedReturnAsync(CompanyMaintenanceAssetReturnCommand command,Func<Task> authority)
    {
        var context=command.Review.Assets.Context;Task Guard()=>ReturnGuard(command,context,true,authority);
        await Guard();var result=await ReturnOwner(command,context).PrepareMaintenanceQualifiedReturnAsync(command,Guard);await Guard();return result;
    }
    public async Task ApplyMaintenanceQualifiedReturnAsync(CompanyMaintenanceAssetReturnCommand command,CompanyMaintenanceQualifiedReturnProof proof,Func<Task> authority)
    {
        var context=command.Review.Assets.Context;Task Guard()=>ReturnGuard(command,context,true,authority);
        await Guard();await ReturnOwner(command,context).ApplyMaintenanceQualifiedReturnAsync(command,proof,Guard);await Guard();
    }
    public async Task VerifyMaintenanceQualifiedReturnAsync(CompanyMaintenanceAssetReturnCommand command,CompanyMaintenanceQualifiedReturnProof proof,FieldSelectionReview reader,bool owned,bool requireWrite,Func<Task> authority)
    {
        Task Guard()=>ReturnGuard(command,reader,requireWrite,authority);
        await Guard();await ReturnOwner(command,reader).VerifyMaintenanceQualifiedReturnAsync(command,proof,reader,owned,requireWrite,Guard);await Guard();
    }
    public async Task<CompanyMaintenanceQualifiedAssets> ReadMaintenanceQualifiedWorkAssetsAsync(FieldSelectionReview context,WorkOrderDetailModel order,Func<Task> authority,bool owned)
    {
        await authority();var zone=Zone(context);
        async Task Guard(){await authority();if(Zone(context)!=zone)throw new InvalidOperationException("Original company work-scope date interpretation changed.");}
        var owner=new FacilityManagementService(editor,columns,defaults,metadata,failures,context.Database.ConnectionName,maintenanceAssetAccess:policy,maintenanceEvidenceZone:zone);
        var result=await owner.ReadMaintenanceQualifiedWorkAssetsAsync(context,order,Guard,owned);await Guard();return result;
    }
    public async Task<CompanyMaintenanceQualifiedAssets> ReadMaintenanceQualifiedCurrentStatusAsync(FieldSelectionReview context,WorkOrderDetailModel order,Func<Task> authority)
    {
        await authority();var zone=Zone(context);
        async Task Guard(){await authority();if(Zone(context)!=zone)throw new InvalidOperationException("Current qualified status date interpretation changed during READ observation.");}
        var owner=new FacilityManagementService(editor,columns,defaults,metadata,failures,context.Database.ConnectionName,maintenanceAssetAccess:policy,maintenanceEvidenceZone:zone);
        var result=await owner.ReadMaintenanceQualifiedCurrentStatusAsync(context,order,Guard);result.Require(order);await Guard();return result;
    }
    public async Task<CompanyMaintenanceQualifiedAssets> ReadMaintenanceQualifiedAssetsAsync(FieldSelectionReview context,WorkOrderDetailModel order,Func<Task> authority)
    {
        using var activity=CompanyMaintenanceHandoffService.PermitProofActivities.StartActivity("maintenance.qualified.evidence");
        await authority();var roles=await returnPolicies.ReadEvidenceReturnRolesAsync(context);var zone=Zone(context);async Task Guard(){await authority();if(Zone(context)!=zone)throw new InvalidOperationException("Recorded company native asset date interpretation changed during review.");}
        var owner=new FacilityManagementService(editor,columns,defaults,metadata,failures,context.Database.ConnectionName,maintenanceAssetAccess:policy,maintenanceEvidenceZone:zone);
        CompanyMaintenanceQualifiedAssets identity;using(CompanyMaintenanceHandoffService.PermitProofActivities.StartActivity("maintenance.qualified.identity"))identity=await owner.ReadMaintenanceQualifiedAssetsAsync(context,order,Guard);
        CompanyMaintenanceQualifiedOperating operating;using(CompanyMaintenanceHandoffService.PermitProofActivities.StartActivity("maintenance.qualified.operating"))operating=await owner.ReadMaintenanceQualifiedOperatingAsync(context,order,identity,Guard);
        CompanyMaintenanceQualifiedLicensing licensing;using(CompanyMaintenanceHandoffService.PermitProofActivities.StartActivity("maintenance.qualified.licensing"))licensing=await owner.ReadMaintenanceQualifiedLicensingAsync(context,order,identity,Guard);
        var sources=identity with{Operating=operating,Licensing=licensing};sources.Require(order);
        async Task RoleGuard(){await Guard();if(!(await returnPolicies.ReadEvidenceReturnRolesAsync(context)).SequenceEqual(roles,StringComparer.Ordinal))throw new UnauthorizedAccessException("Current configured return-role assignments changed during policy comparison.");}
        await RoleGuard();var completeCatalog=await owner.ReadMaintenanceReturnPoliciesAsync(CompanyMaintenanceQualifiedReturnPolicy.ContextFor(sources,roles),RoleGuard);var catalog=CompanyMaintenanceQualifiedReturnPolicy.ScopeCatalog(sources,order,completeCatalog);var nativeSources=await owner.ReadMaintenanceReturnNativeSourcesAsync(context,order,sources,catalog,RoleGuard);var comparison=CompanyMaintenanceQualifiedReturnPolicy.Create(sources,order,catalog,roles,nativeSources,DateTime.UtcNow);
        var result=sources with{ReturnPolicy=comparison};result.Require(order);await RoleGuard();return result;
    }
}
