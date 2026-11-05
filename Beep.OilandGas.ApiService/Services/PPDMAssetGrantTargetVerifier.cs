using System.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Bounded privileged metadata lookup before the canonical company administrator saves a grant.</summary>
public sealed class PPDMAssetGrantTargetVerifier(CompanyModuleConnection company, IDMEEditor editor,
    ICommonColumnHandler columns, IPPDM39DefaultsRepository defaults, IPPDMMetadataRepository metadata,
    ICompanyFieldEligibility fieldEligibility) : IAssetGrantTargetVerifier
{
    public async Task ValidateAsync(string databaseFingerprint, string assetType, string assetId, string accessLevel, string? facilityType = null)
    {
        if (PPDMCompanyFieldEligibility.Rank(accessLevel) == 0) throw RefusalException.Invalid("A reviewed READ, WRITE or DELETE grant level is required.");
        var target = assetType switch
        {
            "FIELD" => (typeof(FIELD), "FIELD_ID"), "WELL" => (typeof(WELL), "UWI"), "POOL" => (typeof(POOL), "POOL_ID"),
            "FACILITY" => (typeof(FACILITY), "FACILITY_ID"), "WORK_ORDER" => (typeof(WORK_ORDER),"WORK_ORDER_ID"),
            "EQUIPMENT" => (typeof(EQUIPMENT), "EQUIPMENT_ID"),
            _ => throw RefusalException.Invalid("This asset type has no reviewed grant target identity.")
        };
        if (string.IsNullOrWhiteSpace(assetId) || assetId != assetId.Trim() || assetId.Length > 128)
            throw RefusalException.Invalid("A canonical asset identifier is required.");
        if (assetType == "FACILITY" ? string.IsNullOrWhiteSpace(facilityType) || facilityType != facilityType.Trim() || facilityType.Length > 128 || facilityType.Any(char.IsControl) : facilityType is not null)
            throw RefusalException.Invalid("A facility grant requires FACILITY_ID and FACILITY_TYPE; the qualifier applies only to facilities.");
        var scope = await company.ResolveAsync();
        if (scope.Fingerprint != databaseFingerprint)
            throw RefusalException.Conflict("The asset grant target belongs to another reviewed database scope.");
        if (!company.Company.Roles.Contains("Administrator", StringComparer.Ordinal))
            throw RefusalException.Forbidden("A current company Administrator is required to verify a grant target.");
        var selected = company.Company.Scope;
        await ValidateMetadataAsync(typeof(BUSINESS_ASSOCIATE), "BUSINESS_ASSOCIATE_ID");
        await ValidateMetadataAsync(target.Item1, assetType == "FACILITY" ? [target.Item2, "FACILITY_TYPE"] : [target.Item2]);
        var companyColumn = await CompanyPPDMRowAccessPolicy.CompanyColumnAsync(target.Item1, metadata);
        ValidateCachedTarget(scope.ConnectionName);
        if (editor.OpenDataSource(scope.ConnectionName) != ConnectionState.Open)
            throw new InvalidOperationException("The reviewed PPDM datasource could not be opened for grant verification.");
        await RequireActiveRecordAsync(typeof(BUSINESS_ASSOCIATE), "BUSINESS_ASSOCIATE_ID", selected.BusinessAssociateId, scope.ConnectionName);
        var record=await RequireActiveRecordAsync(target.Item1, target.Item2, assetId, scope.ConnectionName, assetType=="FACILITY"?null:companyColumn, selected.BusinessAssociateId, facilityType);
        if(assetType=="FACILITY")
        {
            var facility=(FACILITY)record;var revision=CompanyPPDMEvidenceReader.CanonicalRecord(facility);
            if(facility.CURRENT_OPERATOR!=selected.BusinessAssociateId)
            {
                if(!CompanyPPDMEvidenceReader.Identifier(facility.PRIMARY_FIELD_ID))throw RefusalException.Forbidden("A partner-operated facility requires an exact entitled FIELD.");
                await CompanyPPDMEvidenceReader.DefinitionAsync(metadata,typeof(FACILITY),["FACILITY_ID","FACILITY_TYPE"],("PRIMARY_FIELD_ID","FIELD","FIELD_ID"));
                await ValidateMetadataAsync(typeof(FIELD),"FIELD_ID");await RequireActiveRecordAsync(typeof(FIELD),"FIELD_ID",facility.PRIMARY_FIELD_ID,scope.ConnectionName);
                if(PPDMCompanyFieldEligibility.Rank(await fieldEligibility.GetMaximumLevelAsync(facility.PRIMARY_FIELD_ID))<PPDMCompanyFieldEligibility.Rank(accessLevel))throw RefusalException.Forbidden("Current company FIELD entitlement does not permit this partner-operated facility grant.");
                var closing=await RequireActiveRecordAsync(typeof(FACILITY),"FACILITY_ID",assetId,scope.ConnectionName,facilityType:facilityType);
                if(CompanyPPDMEvidenceReader.CanonicalRecord(closing)!=revision || PPDMCompanyFieldEligibility.Rank(await fieldEligibility.GetMaximumLevelAsync(facility.PRIMARY_FIELD_ID))<PPDMCompanyFieldEligibility.Rank(accessLevel))throw RefusalException.Conflict("The partner facility or its FIELD entitlement changed during grant review.");
                await RequireActiveRecordAsync(typeof(FIELD),"FIELD_ID",facility.PRIMARY_FIELD_ID,scope.ConnectionName);
            }
        }
        if (assetType == "FIELD" && PPDMCompanyFieldEligibility.Rank(await fieldEligibility.GetMaximumLevelAsync(assetId)) < PPDMCompanyFieldEligibility.Rank(accessLevel))
            throw RefusalException.Forbidden("Current company PPDM FIELD entitlement does not permit the requested grant level.");
        if(assetType=="WORK_ORDER" && await CompanyWorkOrderFieldReview.LevelAsync(company,editor,columns,defaults,metadata,fieldEligibility,assetId,accessLevel)!=accessLevel)
            throw RefusalException.Forbidden("Current company entitlement to every recorded work-order FIELD must permit the requested grant.");
        if (await company.ResolveAsync() != scope || company.Company.Scope != selected)
            throw RefusalException.Conflict("The company or database changed while verifying an asset grant.");
        if(assetType=="EQUIPMENT")
        {
            await ValidateMetadataAsync(target.Item1,target.Item2);await CompanyPPDMRowAccessPolicy.CompanyColumnAsync(target.Item1,metadata);
            await RequireActiveRecordAsync(target.Item1,target.Item2,assetId,scope.ConnectionName,companyColumn,selected.BusinessAssociateId);
            if(await company.ResolveAsync()!=scope || company.Company.Scope!=selected)throw RefusalException.Conflict("Equipment grant ownership changed before admission.");
        }
    }

    private async Task ValidateMetadataAsync(Type type, params string[] keys)
    {
        var definition = await metadata.GetTableMetadataAsync(type.Name);
        if (definition is null || definition.EntityTypeName != type.Name ||
            !keys.Order(StringComparer.Ordinal).SequenceEqual((definition.PrimaryKeyColumn ?? "").Split(',').Select(key => key.Trim()).Order(StringComparer.Ordinal)) ||
            keys.Any(key => type.GetProperty(key)?.PropertyType != typeof(string)) || type.GetProperty("ACTIVE_IND")?.PropertyType != typeof(string))
            throw RefusalException.Conflict("The canonical PPDM grant target metadata is inconsistent.");
    }

    private async Task<object> RequireActiveRecordAsync(Type type, string key, string id, string connection,
        string? companyColumn = null, string? companyId = null, string? facilityType = null)
    {
        // Deliberately no user-grant policy here: initial access cannot depend on a grant that has not been admitted.
        // This server-only lookup has a current admin gate, exact canonical key, selected binding and postchecks.
        var repository = new PPDMGenericRepository(editor, columns, defaults, metadata, type, connection, type.Name);
        var filters = new List<AppFilter> {
            new AppFilter { FieldName = key, Operator = "=", FilterValue = id },
            new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
        };
        if (companyColumn is not null) filters.Add(new AppFilter { FieldName = companyColumn, Operator = "=", FilterValue = companyId });
        if (facilityType is not null) filters.Add(new AppFilter { FieldName = "FACILITY_TYPE", Operator = "=", FilterValue = facilityType });
        ValidateCachedTarget(connection);
        var records = (await repository.GetAsync(filters)).ToArray();
        var source = editor.GetDataSource(connection) ?? throw new InvalidOperationException("The reviewed PPDM datasource became unavailable during grant verification.");
        ValidateCachedTarget(connection);
        if (source.ErrorObject?.Flag == Errors.Failed)
            throw new InvalidOperationException("The PPDM provider failed while verifying an asset grant target.");
        if (records.Length == 0) throw RefusalException.Forbidden("The selected company or asset has no active PPDM record in the reviewed database.");
        if (records.Length != 1) throw RefusalException.Conflict("The provider returned an ambiguous PPDM grant target.");
        var record = records[0];
        if (!type.IsInstanceOfType(record) || type.GetProperty(key)!.GetValue(record) as string != id ||
            type.GetProperty("ACTIVE_IND")!.GetValue(record) as string != "Y" ||
            (companyColumn is not null && type.GetProperty(companyColumn)!.GetValue(record) as string != companyId) ||
            (facilityType is not null && type.GetProperty("FACILITY_TYPE")!.GetValue(record) as string != facilityType))
            throw RefusalException.Conflict("The provider did not honor the exact active PPDM grant target filters.");
        return record;
    }

    private void ValidateCachedTarget(string connection)
    {
        var source = editor.GetDataSource(connection) ?? throw new InvalidOperationException("The reviewed PPDM grant datasource is unavailable.");
        Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup.MigrationConnectionTarget.Validate(editor, source, connection);
    }
}
