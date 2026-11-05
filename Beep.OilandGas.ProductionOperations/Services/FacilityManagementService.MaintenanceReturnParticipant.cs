using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.WorkOrder;
using System.Text.Json;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Report;
namespace Beep.OilandGas.ProductionOperations.Services;

public sealed partial class FacilityManagementService:ICompanyMaintenanceQualifiedReturnParticipant
{
    private static readonly System.Diagnostics.ActivitySource QualifiedReturnActivities=new("Beep.OilandGas.WorkOrderPermitDecision");
    private static CompanyMaintenanceQualifiedAssets QualifiedSubject(CompanyMaintenanceAssetReturnCommand command)
    {
        command.Require();return command.Review.Assets.QualifiedAssets??throw new InvalidOperationException("The original complete command has no qualified asset population.");
    }
    private QualifiedAssetReturnRows QualifiedProof(CompanyMaintenanceAssetReturnCommand command,CompanyMaintenanceQualifiedReturnProof proof)
    {
        var subject=QualifiedSubject(command);proof.Require();
        var rows=JsonSerializer.Deserialize<QualifiedAssetReturnRows>(proof.Body)??throw new InvalidOperationException("Original private qualified graph is unavailable.");
        if(rows.RecordedAtUtc!=proof.RecordedAtUtc || rows.BeforeFacilities is null || rows.BeforeEquipment is null || rows.AfterFacilities is null || rows.AfterEquipment is null || rows.TimeZoneReferences is null)
            throw new InvalidOperationException("Original complete qualified graph/audit is unavailable.");
        var expected=BuildQualifiedAssetReturnRows(subject,command.Review.Assets.Order,command.QualifiedAssets!,command.Review.Physical.Assignment.Stage.AssignedRole,
            command.ReturnId(),command.ActivatedAtUtc,command.Reason,rows.BeforeFacilities,rows.BeforeEquipment,rows.TimeZoneReferences,rows.RecordedAtUtc);
        if(JsonSerializer.Serialize(expected)!=proof.Body)throw new InvalidOperationException("Retained qualified proof differs from the complete original command/native preimage/effects.");return rows;
    }
    public void RequireMaintenanceQualifiedReturnProof(CompanyMaintenanceAssetReturnCommand command,CompanyMaintenanceQualifiedReturnProof proof)=>_=QualifiedProof(command,proof);
    public async Task<CompanyMaintenanceQualifiedReturnProof> PrepareMaintenanceQualifiedReturnAsync(CompanyMaintenanceAssetReturnCommand command,Func<Task> authority)
    {
        var subject=QualifiedSubject(command);
        var rows=await PrepareQualifiedAssetReturnRowsAsync(subject,command.Review.Assets.Order,command.QualifiedAssets!,command.Review.Physical.Assignment.Stage.AssignedRole,
            command.ReturnId(),command.ActivatedAtUtc,command.Reason,authority);
        var result=new CompanyMaintenanceQualifiedReturnProof(JsonSerializer.Serialize(rows),rows.RecordedAtUtc);RequireMaintenanceQualifiedReturnProof(command,result);return result;
    }
    public async Task ApplyMaintenanceQualifiedReturnAsync(CompanyMaintenanceAssetReturnCommand command,CompanyMaintenanceQualifiedReturnProof proof,Func<Task> authority)
    {
        var rows=QualifiedProof(command,proof);await ApplyQualifiedAssetReturnRowsAsync(QualifiedSubject(command),command.Review.Assets.Order,command.QualifiedAssets!,
            command.Review.Physical.Assignment.Stage.AssignedRole,command.ReturnId(),command.ActivatedAtUtc,command.Reason,rows,authority);
    }
    public async Task VerifyMaintenanceQualifiedReturnAsync(CompanyMaintenanceAssetReturnCommand command,CompanyMaintenanceQualifiedReturnProof proof,
        FieldSelectionReview reader,bool owned,bool requireWrite,Func<Task> authority)
    {
        using var verificationActivity=QualifiedReturnActivities.StartActivity("maintenance.qualified.return.verify");
        var subject=QualifiedSubject(command);var rows=QualifiedProof(command,proof);FieldSelectionReviewValidator.RequireReview(reader,reader.Company,reader.ActorUserId);
        if(reader.Company!=subject.Context.Company || reader.Database!=subject.Context.Database || reader.FieldId!=subject.Context.FieldId ||
            _maintenanceEvidenceZone!=subject.DateTimeZoneId || reader.Database.ConnectionName!=_connectionName)
            throw new UnauthorizedAccessException("Current original qualified company/FIELD/CORE and declared date interpretation are required.");
        var source=_editor.GetDataSource(_connectionName)??throw new InvalidOperationException("Original qualified return source is unavailable.");
        async Task Guard()
        {
            RequireMaintenanceEvidenceLane(source,owned);await authority();RequireMaintenanceEvidenceLane(source,owned);
            if(!ReferenceEquals(source,_editor.GetDataSource(_connectionName)))throw new InvalidOperationException("Original qualified source changed during retained return verification.");
        }
        async Task<CompanyMaintenanceQualifiedAssets> Identity()
        {
            await Guard();CompanyMaintenanceQualifiedAssets identity;
            using(QualifiedReturnActivities.StartActivity("maintenance.qualified.return.identity"))identity=await ReadMaintenanceQualifiedAssetsAsync(reader,command.Review.Assets.Order,Guard,owned);
            if(requireWrite && (identity.Equipment.Any(row=>row.AccessLevel is not ("WRITE" or "DELETE")) ||
                identity.Facilities.Any(row=>CompanyMaintenanceQualifiedAssets.IsOriginalFacilityTarget(command.Review.Assets.Order,row.FacilityId,row.FacilityType) && row.AccessLevel is not ("WRITE" or "DELETE"))))
                throw new UnauthorizedAccessException("Current affected qualified targets require WRITE for private original recovery.");
            // Access is checked using the current reader; private stored evidence retains the original maker's header/levels.
            var normalized=identity with{Context=subject.Context,
                Facilities=identity.Facilities.Select(row=>row with{AccessLevel=subject.Facilities.Single(value=>value.FacilityId==row.FacilityId && value.FacilityType==row.FacilityType).AccessLevel}).ToArray(),
                Equipment=identity.Equipment.Select(row=>row with{AccessLevel=subject.Equipment.Single(value=>value.EquipmentId==row.EquipmentId).AccessLevel}).ToArray()};
            if(normalized.IdentityRevision()!=subject.IdentityRevision())throw new InvalidOperationException("Original qualified parent/installation population or native identity changed after return.");
            await Guard();return identity;
        }
        async Task Sources()
        {
            using var sourceActivity=QualifiedReturnActivities.StartActivity("maintenance.qualified.return.sources");
            var identity=await Identity();
            CompanyMaintenanceQualifiedLicensing licensing;
            using(QualifiedReturnActivities.StartActivity("maintenance.qualified.return.licensing"))licensing=await ReadMaintenanceQualifiedLicensingAsync(reader,command.Review.Assets.Order,identity,Guard,owned);
            // This verifies historical source facts, not a new present-day operating clearance.
            var historical=licensing with{Context=subject.Context,IdentityRevision=subject.IdentityRevision(),Blockers=subject.Licensing!.Blockers};
            if(historical.SourceRevision()!=subject.Licensing.SourceRevision())throw new InvalidOperationException("Original qualified licensing/area/condition/reference source proof changed after return.");
            CompanyMaintenanceQualifiedOperating operating;
            using(QualifiedReturnActivities.StartActivity("maintenance.qualified.return.operating"))operating=await ReadMaintenanceQualifiedOperatingAsync(reader,command.Review.Assets.Order,identity,Guard,owned);
            foreach(var expected in subject.Operating!.References)
                if(operating.References.Count(row=>row.AssetType==expected.AssetType && row.StatusType==expected.StatusType && row.Value==expected.Value &&
                    row.NativeRevision==expected.NativeRevision && row.TypeReferenceRevision==expected.TypeReferenceRevision)!=1)
                    throw new InvalidOperationException("Original qualified operating reference proof changed after return.");
            var expectedZones=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(rows.TimeZoneReferences.Select(MaintenanceIdentityRevision).Order(StringComparer.Ordinal)));
            if(operating.TimeZoneReferencesRevision!=expectedZones)throw new InvalidOperationException("Original/new native qualified time-zone proof changed after return.");
            CompanyMaintenanceReturnPolicyCatalog catalog;
            using(QualifiedReturnActivities.StartActivity("maintenance.qualified.return.policy"))catalog=await ReadMaintenanceReturnPoliciesAsync(subject.ReturnPolicy!.Catalog.Context,Guard,owned);
            foreach(var original in subject.ReturnPolicy.Catalog.Versions)
                if(catalog.Versions.Count(current=>current.RecordId==original.RecordId && JsonSerializer.Serialize(current)==JsonSerializer.Serialize(original))!=1)
                    throw new InvalidOperationException("Original immutable qualified policy history changed; later valid appends cannot replace it.");
            await MaintenanceAssetDefinitionAsync(source,Guard,typeof(R_SOURCE),["SOURCE"]);
            foreach(var reference in subject.ReturnPolicy.NativeSources)
            {
                var current=await MaintenanceAssetRowsCoreAsync<R_SOURCE>(Guard,["SOURCE","ACTIVE_IND"],owned,Equal("SOURCE",reference.Source));
                if(current.Length!=1 || MaintenanceIdentityRevision(current[0])!=reference.NativeRevision)throw new InvalidOperationException("Original native qualified recording/source reference changed after return.");
            }
            await Guard();
        }
        // Admit bounded private postimage reads with current target authority, then validate frozen
        // historical sources once at the closing boundary. These readers retain their own paired observations.
        await Identity();
        var facilities=new List<FACILITY_STATUS>();var equipment=new List<EQUIPMENT_STATUS>();
        foreach(var parent in subject.Facilities)facilities.AddRange(await MaintenanceAssetRowsCoreAsync<FACILITY_STATUS>(Guard,["FACILITY_ID","FACILITY_TYPE","STATUS_ID"],true,Equal("FACILITY_ID",parent.FacilityId),Equal("FACILITY_TYPE",parent.FacilityType)));
        foreach(var parent in subject.Equipment)equipment.AddRange(await MaintenanceAssetRowsCoreAsync<EQUIPMENT_STATUS>(Guard,["EQUIPMENT_ID","EQUIP_STATUS_ID"],true,Equal("EQUIPMENT_ID",parent.EquipmentId)));
        var actualFacilities=facilities.OrderBy(row=>row.FACILITY_ID,StringComparer.Ordinal).ThenBy(row=>row.FACILITY_TYPE,StringComparer.Ordinal).ThenBy(row=>row.STATUS_ID,StringComparer.Ordinal).ToArray();
        var actualEquipment=equipment.OrderBy(row=>row.EQUIPMENT_ID,StringComparer.Ordinal).ThenBy(row=>row.EQUIP_STATUS_ID,StringComparer.Ordinal).ToArray();
        if(JsonSerializer.Serialize(actualFacilities)!=JsonSerializer.Serialize(rows.AfterFacilities) || JsonSerializer.Serialize(actualEquipment)!=JsonSerializer.Serialize(rows.AfterEquipment))
            throw new InvalidOperationException("Complete current native qualified effects differ from the retained original return.");
        await Sources();
        static AppFilter Equal(string name,string value)=>new(){FieldName=name,Operator="=",FilterValue=value};
    }
}
