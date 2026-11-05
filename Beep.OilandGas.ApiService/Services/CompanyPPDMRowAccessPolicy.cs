using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Microsoft.Extensions.Options;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM39.Core.Metadata;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Constants;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data.Security;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Reviewed PPDM ownership paths backed by the existing company asset grant store.</summary>
public sealed partial class CompanyPPDMRowAccessPolicy(CompanyModuleConnection company, IUserAssetAccessStore assets,
    IPPDMMetadataRepository metadata, ICompanyFieldEligibility fieldEligibility, IDMEEditor editor,
    ICommonColumnHandler columns, IPPDM39DefaultsRepository defaults, IOptions<PPDMFieldEligibilityConfiguration> configuration, string? subjectUserId = null) : IPPDMFilteredReadPolicy, IProductionInterventionWorkOrderReader
{
    public async Task<string?> GetEffectiveAccessLevelAsync(Type type, string table, object row, bool includeInherited = true)
    {
        var frozen = type == typeof(HSE_INCIDENT_BA) || type==typeof(WORK_ORDER) || type==typeof(EQUIPMENT) ? CompanyPPDMEvidenceReader.CanonicalRecord(row) : null;
        var paths = await PathsAsync(type, table);
        if (!type.IsInstanceOfType(row)) throw RefusalException.Invalid("A row access decision requires the canonical PPDM entity.");
        if(WellActivityTable(type))ValidateWellActivityRows([row]);
        if (ProjectTable(type)) await ValidateProjectReferencesAsync(type, [row]);
        if (!MatchesAll(row, await GetReadConstraintsAsync(type, table))) return null;
        if(type==typeof(FACILITY) && !includeInherited && ((FACILITY)row).CURRENT_OPERATOR!=company.Company.Scope.BusinessAssociateId)
        {
            var facility=(FACILITY)row;if(!CompanyPPDMEvidenceReader.Identifier(facility.PRIMARY_FIELD_ID))return null;
            var original=CompanyPPDMEvidenceReader.CanonicalRecord(facility);var combined=await GrantsAsync(paths);
            foreach(var level in new[]{"DELETE","WRITE","READ"})
            {
                var direct=combined.Where(grant=>grant.AssetType=="FACILITY" && grant.AssetId==facility.FACILITY_ID && grant.FacilityType==facility.FACILITY_TYPE && PPDMCompanyFieldEligibility.Rank(grant.AccessLevel)>=PPDMCompanyFieldEligibility.Rank(level) && grant.IsActive).ToArray();
                var field=combined.Where(grant=>grant.AssetType=="FIELD" && grant.AssetId==facility.PRIMARY_FIELD_ID && grant.Inherit && grant.IsActive && PPDMCompanyFieldEligibility.Rank(grant.AccessLevel)>=PPDMCompanyFieldEligibility.Rank(level)).ToArray();
                if(direct.Length==0 || field.Length==0)continue;
                var fresh=await GrantsAsync(paths);
                if(original!=CompanyPPDMEvidenceReader.CanonicalRecord(facility))throw RefusalException.Conflict("The partner facility changed during access review.");
                if(direct.Any(before=>fresh.Any(after=>SameGrant(before,after))) && field.Any(before=>fresh.Any(after=>SameGrant(before,after))))return level;
            }
            return null;
        }
        if (!includeInherited) paths = paths.Where(path => !path.Inherited).ToArray();
        var grants = await GrantsAsync(paths);
        foreach (var level in HseTable(type) || WellActivityTable(type) ? new[] { "READ" } : new[] { "DELETE", "WRITE", "READ" })
            if (Matches(row, await ReadPredicatesAsync(type, paths, HseTable(type) || WellActivityTable(type) ? grants : grants.Where(grant => grant.AccessLevel == level).ToList())))
            {
                if (type == typeof(HSE_INCIDENT_BA))
                {
                    await ValidateParticipantPayloadAsync([row]);
                    if (frozen != CompanyPPDMEvidenceReader.CanonicalRecord(row)) throw RefusalException.Conflict("The participant payload changed during access review.");
                    if (!Matches(row, await ReadPredicatesAsync(type, paths, await GrantsAsync(paths)))) return null;
                    if (frozen != CompanyPPDMEvidenceReader.CanonicalRecord(row)) throw RefusalException.Conflict("The participant payload changed before the access decision.");
                }
                if(type==typeof(WORK_ORDER))
                {
                    var effective=await CompanyWorkOrderFieldReview.LevelAsync(company,editor,columns,defaults,metadata,fieldEligibility,((WORK_ORDER)row).WORK_ORDER_ID,level);
                    if(frozen!=CompanyPPDMEvidenceReader.CanonicalRecord(row))throw RefusalException.Conflict("The work-order header changed during resource review.");
                    var freshGrants = await GrantsAsync(paths);
                    var unchangedGrants = grants.Where(before => before.AccessLevel == level &&
                        freshGrants.Any(after => SameGrant(before, after))).ToList();
                    if(!Matches(row,await ReadPredicatesAsync(type,paths,unchangedGrants)))return null;
                    if(frozen!=CompanyPPDMEvidenceReader.CanonicalRecord(row))throw RefusalException.Conflict("The work-order header changed before the resource decision.");
                    return effective;
                }
                if(type==typeof(EQUIPMENT))
                {
                    await RequireCurrentEquipmentOwnerAsync((EQUIPMENT)row,frozen!);
                    var freshGrants=await GrantsAsync(paths);var unchanged=grants.Where(before=>before.AccessLevel==level && freshGrants.Any(after=>SameGrant(before,after))).ToList();
                    if(!Matches(row,await ReadPredicatesAsync(type,paths,unchanged)))return null;
                    await RequireCurrentEquipmentOwnerAsync((EQUIPMENT)row,frozen!);
                }
                return level;
            }
        return null;
    }

    public async Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type, string table)
    {
        var paths = await PathsAsync(type, table);
        var grants = await GrantsAsync(paths);
        return await ReadPredicatesAsync(type, paths, grants);
    }

    public async Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsForFiltersAsync(Type type, string table, IReadOnlyList<AppFilter> requestedFilters)
    {
        var incident = ExactIncidentFilter(type, requestedFilters);
        var project = ExactProjectFilter(type, requestedFilters);
        var field = type == typeof(HSE_INCIDENT_COMPONENT) ? ExactReadIdentity(requestedFilters, "FIELD_ID") : null;
        var paths = await PathsAsync(type, table);
        return await ReadPredicatesAsync(type, paths, await GrantsAsync(paths), reviewIncident: incident, reviewField: field, reviewProject: project);
    }

    public Task ValidateRowsForFiltersAsync(Type type, string table, IReadOnlyList<AppFilter> requestedFilters, IReadOnlyList<object> rows) =>
        ValidateReadRowsAsync(type, table, rows, ExactIncidentFilter(type, requestedFilters), ExactProjectFilter(type, requestedFilters));

    private static string? ExactIncidentFilter(Type type, IReadOnlyList<AppFilter> requestedFilters)
    {
        if (!IncidentTable(type)) return null;
        return ExactReadIdentity(requestedFilters, "INCIDENT_ID");
    }

    private static string? ExactProjectFilter(Type type, IReadOnlyList<AppFilter> requestedFilters) =>
        ProjectTable(type) ? ExactReadIdentity(requestedFilters, "PROJECT_ID") : null;

    private static string? ExactReadIdentity(IReadOnlyList<AppFilter> requestedFilters, string column)
    {
        var filters = requestedFilters.Where(filter => filter.FieldName == column).ToArray();
        if (filters.Length == 0 || filters.Any(filter => filter.Operator != "=")) return null;
        var ids = filters.Select(filter => filter.FilterValue).Distinct(StringComparer.Ordinal).ToArray();
        if (ids.Length != 1 || !CompanyPPDMEvidenceReader.Identifier(ids[0]))
            throw RefusalException.Invalid("An exact resource read requires one unambiguous recorded identifier.");
        return ids[0];
    }

    public Task ValidateRowsAsync(Type type, string table, IReadOnlyList<object> rows) => ValidateReadRowsAsync(type, table, rows, null);

    private async Task ValidateReadRowsAsync(Type type, string table, IReadOnlyList<object> rows, string? incident, string? project = null)
    {
        var frozen = type == typeof(HSE_INCIDENT_BA) ? rows.Select(CompanyPPDMEvidenceReader.CanonicalRecord).ToArray() : null;
        var paths = await PathsAsync(type, table);
        if (rows.Any(row => !type.IsInstanceOfType(row))) throw RefusalException.Invalid("Row validation requires canonical PPDM entities.");
        if (incident is not null && rows.Any(row => type.GetProperty("INCIDENT_ID")!.GetValue(row) as string != incident))
            throw RefusalException.Forbidden("The provider returned another incident before scoped row validation.");
        if (project is not null && rows.Any(row => type.GetProperty("PROJECT_ID")!.GetValue(row) as string != project))
            throw RefusalException.Forbidden("The provider returned another project before scoped row validation.");
        if(WellActivityTable(type))ValidateWellActivityRows(rows);
        if (ProjectTable(type)) await ValidateProjectReferencesAsync(type, rows);
        if(type==typeof(WORK_ORDER))foreach(var row in rows.Cast<WORK_ORDER>())
            if(await CompanyWorkOrderFieldReview.LevelAsync(company,editor,columns,defaults,metadata,fieldEligibility,row.WORK_ORDER_ID,"READ") is null)
                throw RefusalException.Forbidden("Current company FIELD entitlement does not permit this recorded order read.");
        var predicates = await ReadPredicatesAsync(type, paths, await GrantsAsync(paths), reviewIncident: incident, reviewProject: project);
        var constraints = await GetReadConstraintsAsync(type, table);
        foreach (var row in rows)
            if (!type.IsInstanceOfType(row) || !Matches(row, predicates) || !MatchesAll(row, constraints))
                throw RefusalException.Forbidden("The provider returned a row outside current company asset access.");
        if(type==typeof(EQUIPMENT))foreach(var row in rows)
            if(await GetEffectiveAccessLevelAsync(type,table,row) is null)
                throw RefusalException.Forbidden("Current direct equipment authority and original company owner are required before delivery.");
        if (type == typeof(HSE_INCIDENT_BA))
        {
            await ValidateParticipantPayloadAsync(rows);
            if (!frozen!.SequenceEqual(rows.Select(CompanyPPDMEvidenceReader.CanonicalRecord), StringComparer.Ordinal))
                throw RefusalException.Conflict("The participant payload changed during validation.");
            var fresh = await ReadPredicatesAsync(type, paths, await GrantsAsync(paths), reviewIncident: incident);
            if (rows.Any(row => !Matches(row, fresh))) throw RefusalException.Forbidden("Participant case authority changed before delivery.");
            if (!frozen!.SequenceEqual(rows.Select(CompanyPPDMEvidenceReader.CanonicalRecord), StringComparer.Ordinal))
                throw RefusalException.Conflict("The participant payload changed before delivery.");
        }
    }

    public async Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type, string table)
    {
        await PathsAsync(type, table);
        await company.ResolveAsync();
        if (IncidentTable(type) || WellActivityTable(type) || type==typeof(WORK_ORDER)) return [CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y")];
        if (ProjectTable(type))
        {
            var filters = new List<AppFilter> { CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y") };
            if (type == typeof(PROJECT_CONDITION)) filters.Add(CompanyPPDMEvidenceReader.Equal("PROJECT_TYPE", HseReferenceCodes.Hazop.ProjectType));
            return filters;
        }
        // A separately operated facility can be visible through a current inherited FIELD grant.
        // Direct facility grants retain the selected-company operator constraint in their own conjunction.
        if(type==typeof(FACILITY))return [];
        var column = await CompanyColumnAsync(type, metadata);
        return column is null ? [] : [new AppFilter { FieldName = column, Operator = "=", FilterValue = company.Company.Scope.BusinessAssociateId }];
    }

    // These are existing canonical company relationships, not proof of legal ownership or JV interests.
    internal static async Task<string?> CompanyColumnAsync(Type type, IPPDMMetadataRepository metadata)
    {
        var column = type.Name switch { "WELL" => "OPERATOR", "POOL" => "BUSINESS_ASSOCIATE_ID", "FACILITY" => "CURRENT_OPERATOR", "EQUIPMENT" => "CURRENT_OWNER_BA_ID", _ => null };
        if (column is null) return null;
        var definition = await metadata.GetTableMetadataAsync(type.Name);
        var relationships = definition?.ForeignKeys.Where(key => key.ForeignKeyColumn == column).ToArray();
        if (definition?.EntityTypeName != type.Name || type.GetProperty(column)?.PropertyType != typeof(string) ||
            relationships is null || relationships.Length != 1 || relationships[0].ReferencedTable != "BUSINESS_ASSOCIATE" ||
            relationships[0].ReferencedPrimaryKey != "BUSINESS_ASSOCIATE_ID")
            throw RefusalException.Conflict("The PPDM company relationship metadata does not match its canonical model.");
        return column;
    }

    private async Task RequireCurrentEquipmentOwnerAsync(EQUIPMENT row,string revision)
    {
        if(row.ACTIVE_IND!="Y" || row.CURRENT_OWNER_BA_ID!=company.Company.Scope.BusinessAssociateId || CompanyPPDMEvidenceReader.CanonicalRecord(row)!=revision)
            throw RefusalException.Conflict("The original active equipment company owner changed during access review.");
        var database=await company.ResolveAsync();var reader=new CompanyPPDMEvidenceReader(company,editor,columns,defaults,metadata,database,company.Company.Scope,company.Company.UserId);
        await reader.OpenAsync();await CompanyPPDMEvidenceReader.DefinitionAsync(metadata,typeof(EQUIPMENT),["EQUIPMENT_ID"],("CURRENT_OWNER_BA_ID","BUSINESS_ASSOCIATE","BUSINESS_ASSOCIATE_ID"));
        var current=await reader.ReadAsync(typeof(EQUIPMENT),[CompanyPPDMEvidenceReader.Equal("EQUIPMENT_ID",row.EQUIPMENT_ID),CompanyPPDMEvidenceReader.Equal("CURRENT_OWNER_BA_ID",row.CURRENT_OWNER_BA_ID),CompanyPPDMEvidenceReader.Equal("ACTIVE_IND","Y")]);
        if(current.Length!=1 || CompanyPPDMEvidenceReader.CanonicalRecord(current[0])!=revision || CompanyPPDMEvidenceReader.CanonicalRecord(row)!=revision)
            throw RefusalException.Conflict("The original equipment owner/source changed before the access decision.");
    }

    public async Task ValidateMutationAsync(Type type, string table, object entity, object? previous,
        PPDMRowMutation operation, string? actor)
    {
        if (actor is not null) company.RequireActor(actor);
        if (!type.IsInstanceOfType(entity) || (previous is not null && !type.IsInstanceOfType(previous)))
            throw RefusalException.Invalid("An asset mutation requires the canonical table's entity type.");
        if (entity is HSE_INCIDENT_RESPONSE response && response.RESPONSE_TYPE == "STATUS" && response.RESPONSE_RESULT is IncidentState.PendingCloseOut or IncidentState.Closed ||
            previous is HSE_INCIDENT_RESPONSE priorResponse && priorResponse.RESPONSE_TYPE == "STATUS" && priorResponse.RESPONSE_RESULT is IncidentState.PendingCloseOut or IncidentState.Closed)
            throw RefusalException.Forbidden("Incident verification and closure statuses require reviewed lifecycle commands.");
        if (type == typeof(HSE_CA_VERIFICATION) || string.Equals(table, nameof(HSE_CA_VERIFICATION), StringComparison.OrdinalIgnoreCase) || type == typeof(HSE_INCIDENT_CLOSURE) || string.Equals(table, nameof(HSE_INCIDENT_CLOSURE), StringComparison.OrdinalIgnoreCase))
            throw RefusalException.Forbidden("Corrective-action verification requires its reviewed company command owner.");
        if(type==typeof(HSE_INCIDENT_RCA_INTENT) || string.Equals(table,"HSE_INCIDENT_RCA_INTENT",StringComparison.OrdinalIgnoreCase) || type==typeof(HSE_INCIDENT_REPORT_INTENT) || string.Equals(table,"HSE_INCIDENT_REPORT_INTENT",StringComparison.OrdinalIgnoreCase) || type==typeof(FACILITY_EDIT_INTENT) || string.Equals(table,"FACILITY_EDIT_INTENT",StringComparison.OrdinalIgnoreCase) || type==typeof(WORK_ORDER_MAINTENANCE_RETURN_POLICY) || string.Equals(table,"WORK_ORDER_MAINTENANCE_RETURN_POLICY",StringComparison.OrdinalIgnoreCase) || type==typeof(WELL_STATUS) || type==typeof(FACILITY_STATUS) || type==typeof(EQUIPMENT_STATUS) || type==typeof(WORK_ORDER_ASSET_RETURN) || string.Equals(table,"WELL_STATUS",StringComparison.OrdinalIgnoreCase) || string.Equals(table,"FACILITY_STATUS",StringComparison.OrdinalIgnoreCase) || string.Equals(table,"EQUIPMENT_STATUS",StringComparison.OrdinalIgnoreCase) || string.Equals(table,"WORK_ORDER_ASSET_RETURN",StringComparison.OrdinalIgnoreCase))
            throw RefusalException.Forbidden("Native statuses, return intents and maintenance policies require their reviewed company command owner.");
        if(type==typeof(FACILITY) || type==typeof(FACILITY_FIELD) || type==typeof(FACILITY_EQUIPMENT) || type==typeof(FACILITY_EQUIPMENT_INSTALL_INTENT) || type==typeof(FACILITY_CREATE_INTENT) || string.Equals(table,"FACILITY",StringComparison.OrdinalIgnoreCase) || string.Equals(table,"FACILITY_FIELD",StringComparison.OrdinalIgnoreCase) || string.Equals(table,"FACILITY_EQUIPMENT",StringComparison.OrdinalIgnoreCase) || string.Equals(table,"FACILITY_EQUIPMENT_INSTALL_INTENT",StringComparison.OrdinalIgnoreCase) || string.Equals(table,"FACILITY_CREATE_INTENT",StringComparison.OrdinalIgnoreCase))
            throw RefusalException.Forbidden("Facility master, FIELD-link, equipment installation and private creation writes require a reviewed company command owner.");
        if(string.Equals(table,"FIELD_DEVELOPMENT_PLAN",StringComparison.OrdinalIgnoreCase) || type.Name=="FIELD_DEVELOPMENT_PLAN")
            throw RefusalException.Forbidden("Development draft writes require the reviewed company draft command owner.");
        var paths = await PathsAsync(type, table);
        if(type==typeof(WORK_ORDER))throw RefusalException.Forbidden("Work-order writes require the reviewed canonical command owner.");
        if(type==typeof(EQUIPMENT))throw RefusalException.Forbidden("Equipment writes require a reviewed company command with its original installation and native state evidence.");
        if(WellActivityTable(type))throw RefusalException.Forbidden("Intervention activity writes require the reviewed original decision command owner.");
        if (HseTable(type))
            throw RefusalException.Forbidden("HSE writes require a reviewed command with reference and transaction evidence.");
        var predicates = Predicates(paths, await GrantsAsync(paths), operation, read: false);
        var constraints = await GetReadConstraintsAsync(type, table);
        if (!Matches(entity, predicates) || (operation != PPDMRowMutation.Insert &&
            (previous is null || !Matches(previous, predicates))) || !MatchesAll(entity, constraints) ||
            (previous is not null && !MatchesAll(previous, constraints)))
            throw RefusalException.Forbidden("The selected company does not grant this asset mutation.");
        // Reparenting requires its own reviewed command, even when both fields are visible.
        foreach (var column in paths.SelectMany(path => path.Qualifier is null ? new[] { path.Column } : new[] { path.Column, path.Qualifier }))
            if (previous is not null && Value(previous, column) != Value(entity, column))
                throw RefusalException.Forbidden("Generic mutations cannot change an asset's ownership path.");
    }

    private async Task<List<AppUserAssetAccess>> GrantsAsync(Path[] paths)
    {
        var database = await company.ResolveAsync();
        var grants = (await assets.ReadAsync(subjectUserId ?? company.Company.UserId, database.Fingerprint))
            .Where(grant => paths.Any(path => path.Asset == grant.AssetType && (!path.Inherited || grant.Inherit)) &&
                (grant.AssetType!="EQUIPMENT" || !grant.Inherit && grant.FacilityType is null)).ToList();
        var caps = new Dictionary<string, string?>(StringComparer.Ordinal);
        var result = new List<AppUserAssetAccess>();
        foreach (var grant in grants)
        {
            if (grant.AssetType != "FIELD" || !grant.IsActive) { result.Add(grant); continue; }
            if (!caps.TryGetValue(grant.AssetId, out var cap))
                caps.Add(grant.AssetId, cap = await fieldEligibility.GetMaximumLevelAsync(grant.AssetId));
            if (PPDMCompanyFieldEligibility.Rank(cap) == 0) continue;
            var level = PPDMCompanyFieldEligibility.Rank(cap) < PPDMCompanyFieldEligibility.Rank(grant.AccessLevel) ? cap! : grant.AccessLevel;
            // Effective authority is a detached projection; stored grants and retirement provenance stay intact.
            result.Add(new AppUserAssetAccess { Id = grant.Id, UserId = grant.UserId, DatabaseScope = grant.DatabaseScope,
                AssetType = grant.AssetType, AssetId = grant.AssetId, FacilityType = grant.FacilityType,
                OrganizationId = grant.OrganizationId, AccessLevel = level, Inherit = grant.Inherit, IsActive = grant.IsActive,
                CreatedBy = grant.CreatedBy, ChangedBy = grant.ChangedBy, CreatedUtc = grant.CreatedUtc,
                ChangedUtc = grant.ChangedUtc, ConcurrencyStamp = grant.ConcurrencyStamp });
        }
        var current = await assets.ReadAsync(subjectUserId ?? company.Company.UserId, database.Fingerprint);
        if (await company.ResolveAsync() != database) throw RefusalException.Conflict("The asset database scope changed while resolving company FIELD eligibility.");
        // An entitlement lookup cannot keep an app grant revoked or changed while prerequisite reads were in flight.
        var unchangedIds = grants.Where(before => current.Any(after => SameGrant(before, after)))
            .Select(grant => grant.Id).ToHashSet(StringComparer.Ordinal);
        return result.Where(grant => unchangedIds.Contains(grant.Id)).ToList();
    }

    private static bool SameGrant(AppUserAssetAccess before, AppUserAssetAccess after) =>
        after.Id == before.Id && after.UserId == before.UserId && after.DatabaseScope == before.DatabaseScope &&
        after.OrganizationId == before.OrganizationId && after.AssetType == before.AssetType &&
        after.AssetId == before.AssetId && after.FacilityType == before.FacilityType &&
        after.AccessLevel == before.AccessLevel && after.Inherit == before.Inherit &&
        after.IsActive == before.IsActive && after.ConcurrencyStamp == before.ConcurrencyStamp;

    private async Task<Path[]> PathsAsync(Type type, string table)
    {
        if (!string.Equals(type.Name, table, StringComparison.OrdinalIgnoreCase) ||
            type != typeof(Beep.OilandGas.PPDM.Models.IPPDMEntity).Assembly.GetType("Beep.OilandGas.PPDM39.Models." + type.Name))
            throw RefusalException.Invalid("A row policy requires the canonical PPDM entity and table.");
        if (HseTable(type))
        {
            var authority = await company.ResolveSubjectAsync(subjectUserId ?? company.Company.UserId);
            if (authority?.Permissions.Contains(PermissionConstants.HSE.View, StringComparer.Ordinal) != true)
                throw RefusalException.Forbidden("Current company HSE.View permission is required to read HSE incident or project data.");
        }
        if(WellActivityTable(type)){await WellActivityDefinitionAsync();return [new("UWI","WELL",false),new("ASSIGNED_FIELD","FIELD",true)];}
        if (IncidentTable(type))
        {
            await IncidentDefinitionAsync(type);
            return [new("INCIDENT_ID", "FIELD", true)];
        }
        if (ProjectTable(type))
        {
            await ProjectDefinitionAsync(type);
            return [new("PROJECT_ID", "FIELD", true)];
        }
        Path[] paths = type.Name switch
        {
            "WORK_ORDER" => [new("WORK_ORDER_ID","WORK_ORDER",false)],
            "EQUIPMENT" => [new("EQUIPMENT_ID","EQUIPMENT",false)],
            "FIELD" => [new("FIELD_ID", "FIELD", false)],
            "WELL" => [new("UWI", "WELL", false), new("ASSIGNED_FIELD", "FIELD", true)],
            "POOL" => [new("POOL_ID", "POOL", false), new("FIELD_ID", "FIELD", true)],
            "FACILITY" => [new("FACILITY_ID", "FACILITY", false, "FACILITY_TYPE"), new("PRIMARY_FIELD_ID", "FIELD", true)],
            _ => throw RefusalException.Forbidden("This PPDM table has no reviewed company ownership policy.")
        };
        var tableMetadata = await metadata.GetTableMetadataAsync(table);
        var expectedKeys = type.Name switch
        {
            "WORK_ORDER" => ["WORK_ORDER_ID"],
            "EQUIPMENT" => ["EQUIPMENT_ID"],
            "FIELD" => new[] { "FIELD_ID" }, "WELL" => ["UWI"], "POOL" => ["POOL_ID"],
            "FACILITY" => ["FACILITY_ID", "FACILITY_TYPE"], _ => Array.Empty<string>()
        };
        if (tableMetadata is null || tableMetadata.EntityTypeName != type.Name ||
            !expectedKeys.Order().SequenceEqual((tableMetadata.PrimaryKeyColumn ?? "").Split(',').Select(key => key.Trim()).Order()) ||
            paths.Any(path => type.GetProperty(path.Column)?.PropertyType != typeof(string) || path.Qualifier is not null && type.GetProperty(path.Qualifier)?.PropertyType != typeof(string)) ||
            paths.Where(path => path.Inherited).Any(path => !tableMetadata.ForeignKeys.Any(key =>
                key.ForeignKeyColumn == path.Column && key.ReferencedTable == "FIELD" && key.ReferencedPrimaryKey == "FIELD_ID")))
            throw RefusalException.Conflict("The PPDM ownership metadata does not match its canonical model.");
        await CompanyColumnAsync(type, metadata);
        return paths;
    }

    private static IReadOnlyList<IReadOnlyList<AppFilter>> Predicates(Path[] paths, List<AppUserAssetAccess> grants,
        PPDMRowMutation operation, bool read)
    {
        var result = new List<IReadOnlyList<AppFilter>>();
        foreach (var path in paths)
            foreach (var grant in grants.Where(grant => grant.IsActive && grant.AssetType == path.Asset &&
                (!path.Inherited || grant.Inherit) && !string.IsNullOrWhiteSpace(grant.AssetId) &&
                (path.Qualifier is null ? grant.FacilityType is null : CanonicalQualifier(grant.FacilityType)) &&
                (read ? grant.AccessLevel is "READ" or "WRITE" or "DELETE" :
                    operation == PPDMRowMutation.Delete ? grant.AccessLevel == "DELETE" : grant.AccessLevel is "WRITE" or "DELETE")))
            {
                var conjunction = new List<AppFilter> { new() { FieldName = path.Column, Operator = "=", FilterValue = grant.AssetId } };
                if (path.Qualifier is not null) conjunction.Add(new() { FieldName = path.Qualifier, Operator = "=", FilterValue = grant.FacilityType });
                if (!result.Any(group => group.Count == conjunction.Count && group.All(filter => conjunction.Any(value => value.FieldName == filter.FieldName && value.FilterValue == filter.FilterValue))))
                    result.Add(conjunction);
            }
        if (result.Count > 256)
            throw RefusalException.Conflict("This asset selection exceeds the supported scoped query limit. Narrow the assigned assets.");
        return result;
    }

    private static bool IncidentTable(Type type) => type.Name is "HSE_INCIDENT" or "HSE_INCIDENT_COMPONENT" or
        "HSE_INCIDENT_DETAIL" or "HSE_INCIDENT_RESPONSE" or "HSE_INCIDENT_CAUSE" or "HSE_INCIDENT_BA";
    private static bool ProjectTable(Type type) => type.Name is "PROJECT" or "PROJECT_COMPONENT" or "PROJECT_STEP" or "PROJECT_CONDITION" or "PROJECT_STATUS" or "PROJECT_STEP_BA";
    private static bool HseTable(Type type) => IncidentTable(type) || ProjectTable(type);

    private async Task IncidentDefinitionAsync(Type type)
    {
        string[] keys = type.Name switch
        {
            "HSE_INCIDENT" => ["INCIDENT_ID"], "HSE_INCIDENT_COMPONENT" => ["INCIDENT_ID", "COMPONENT_OBS_NO"],
            "HSE_INCIDENT_DETAIL" => ["INCIDENT_ID", "DETAIL_OBS_NO"], "HSE_INCIDENT_RESPONSE" => ["INCIDENT_ID", "RESPONSE_OBS_NO"],
            "HSE_INCIDENT_CAUSE" => ["INCIDENT_ID", "CAUSE_OBS_NO"], "HSE_INCIDENT_BA" => ["INCIDENT_ID", "PARTY_OBS_NO", "PARTY_ROLE_OBS_NO"],
            _ => throw RefusalException.Forbidden("This table has no reviewed incident relationship.")
        };
        await CompanyPPDMEvidenceReader.DefinitionAsync(metadata, type, keys,
            type == typeof(HSE_INCIDENT) ? [] : [("INCIDENT_ID", "HSE_INCIDENT", "INCIDENT_ID")]);
        if (type == typeof(HSE_INCIDENT_BA))
            await CompanyPPDMEvidenceReader.DefinitionAsync(metadata, type, keys,
                ("INVOLVED_PARTY_BA_ID", "BUSINESS_ASSOCIATE", "BUSINESS_ASSOCIATE_ID"), ("INVOLVED_BA_ROLE", "R_INCIDENT_BA_ROLE", "BA_ROLE"),
                ("INVOLVED_BA_STATUS", "R_EMPLOYEE_STATUS", "EMPLOYEE_STATUS"), ("UWI", "WELL_DRILL_PERIOD_CREW", "UWI"));
        if (type.GetProperty("INCIDENT_ID")?.PropertyType != typeof(string))
            throw RefusalException.Conflict("The canonical incident identifier is unavailable.");
    }

    private async Task<IReadOnlyList<IReadOnlyList<AppFilter>>> ReadPredicatesAsync(Type type, Path[] paths, List<AppUserAssetAccess> grants, bool completePopulation = false, bool metadataReview = false, string? reviewIncident = null, string? reviewField = null, string? reviewProject = null)
    {
        if(WellActivityTable(type))return await WellActivityPredicatesAsync(grants);
        if (ProjectTable(type))
        {
            if (reviewProject is not null) return await ExactProjectPredicatesAsync(type, paths, grants, reviewProject);
            var groups = new List<IReadOnlyList<AppFilter>>();
            if (type != typeof(PROJECT_STEP_BA)) groups.AddRange(await HazopPredicatesAsync(type, paths, grants));
            if (type == typeof(PROJECT) || type == typeof(PROJECT_STEP) || type == typeof(PROJECT_STATUS) || type == typeof(PROJECT_STEP_BA))
                groups.AddRange(await CorrectivePredicatesAsync(type, paths, grants));
            if (groups.Count > 256) throw RefusalException.Conflict("Narrow the assigned HSE project predicates.");
            return groups;
        }
        if(type==typeof(FACILITY))
        {
            var groups=Predicates(paths,grants,PPDMRowMutation.Insert,read:true);
            return groups.Select(group=>group.Any(filter=>filter.FieldName=="FACILITY_ID")
                ? (IReadOnlyList<AppFilter>)group.Append(CompanyPPDMEvidenceReader.Equal("CURRENT_OPERATOR",company.Company.Scope.BusinessAssociateId)).ToArray()
                : group).ToArray();
        }
        if (!IncidentTable(type)) return Predicates(paths, grants, PPDMRowMutation.Insert, read: true);
        var fields = Predicates(paths, grants, PPDMRowMutation.Insert, read: true)
            .Select(group => group.Single().FilterValue).ToHashSet(StringComparer.Ordinal);
        if (reviewField is not null) fields.IntersectWith([reviewField]);
        if (fields.Count == 0) {
            if (completePopulation) throw RefusalException.Forbidden("Complete HSE totals require inherited access to the exact requested FIELD.");
            return [];
        }
        await IncidentDefinitionAsync(typeof(HSE_INCIDENT));
        await CompanyPPDMEvidenceReader.DefinitionAsync(metadata, typeof(HSE_INCIDENT_COMPONENT), ["INCIDENT_ID", "COMPONENT_OBS_NO"],
            ("INCIDENT_ID", "HSE_INCIDENT", "INCIDENT_ID"), ("FIELD_ID", "FIELD", "FIELD_ID"),
            ("COMPONENT_ROLE", "R_HSE_COMP_ROLE", "COMPONENT_ROLE"), ("COMPONENT_TYPE", "R_HSE_INCIDENT_COMP_TYPE", "COMPONENT_TYPE"),
            ("RESTRICTION_ID", "RESTRICTION", "RESTRICTION_ID"));
        var database = await company.ResolveAsync();
        var reader = new CompanyPPDMEvidenceReader(company, editor, columns, defaults, metadata, database, company.Company.Scope, company.Company.UserId);
        await reader.OpenAsync();
        await ReferenceAsync(typeof(R_HSE_COMP_ROLE), "COMPONENT_ROLE", HseReferenceCodes.FieldContext.PrimaryRole);
        await ReferenceAsync(typeof(R_HSE_INCIDENT_COMP_TYPE), "COMPONENT_TYPE", HseReferenceCodes.FieldContext.ComponentType);
        await CompanyPPDMEvidenceReader.DefinitionAsync(metadata, typeof(FIELD), ["FIELD_ID"]);
        var candidates = new HashSet<string>(StringComparer.Ordinal);
        var inventory = new List<string>();
        var exactFieldEvidence = new Dictionary<string, string>(StringComparer.Ordinal);
        if (completePopulation && reviewIncident is not null)
            throw RefusalException.Conflict("A complete HSE population cannot be qualified by one incident.");
        foreach (var field in fields)
        {
            var fieldRows = await reader.ReadAsync(typeof(FIELD), [CompanyPPDMEvidenceReader.Equal("FIELD_ID", field), CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y")]);
            if (fieldRows.Length > 1) throw RefusalException.Conflict("The HSE FIELD target is ambiguous.");
            if (fieldRows.Length == 0 || !Current(fieldRows[0])) {
                if (completePopulation) throw RefusalException.Conflict("The requested complete HSE population has no current FIELD target.");
                continue;
            }
            if (reviewIncident is not null)
            {
                // A known case is authorized through its own complete primary association below.
                // Reading every unrelated FIELD case adds no authority to that exact association.
                exactFieldEvidence.Add(field, CompanyPPDMEvidenceReader.Fingerprint(fieldRows));
                continue;
            }
            foreach (var component in (await reader.ReadAsync(typeof(HSE_INCIDENT_COMPONENT),
                [CompanyPPDMEvidenceReader.Equal("FIELD_ID", field), CompanyPPDMEvidenceReader.Equal("COMPONENT_ROLE", HseReferenceCodes.FieldContext.PrimaryRole), CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y")])).Cast<HSE_INCIDENT_COMPONENT>())
            {
                inventory.Add(CompanyPPDMEvidenceReader.CanonicalRecord(component));
                if (!CompanyPPDMEvidenceReader.Identifier(component.INCIDENT_ID))
                    throw RefusalException.Conflict("A primary HSE FIELD component has an invalid incident identifier.");
                candidates.Add(component.INCIDENT_ID);
                if (candidates.Count > 256) throw RefusalException.Conflict("Narrow the assigned HSE FIELD evidence lookup.");
            }
        }
        if (reviewIncident is not null)
        {
            fields.IntersectWith(exactFieldEvidence.Keys);
            if (fields.Count == 0) return [];
            candidates.Add(reviewIncident);
        }
        var reviewed = new List<(string Incident, string Field, string Header, string Primary, IncidentRestrictionEvidence Restrictions, ParticipantRosterEvidence? Participants)>();
        foreach (var incident in candidates)
        {
            if (reviewIncident is not null && incident != reviewIncident) continue;
            // Read every active primary association for this incident, including another FIELD; a scoped candidate cannot hide ambiguity.
            var primary = (await reader.ReadAsync(typeof(HSE_INCIDENT_COMPONENT),
                [CompanyPPDMEvidenceReader.Equal("INCIDENT_ID", incident), CompanyPPDMEvidenceReader.Equal("COMPONENT_ROLE", HseReferenceCodes.FieldContext.PrimaryRole), CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y")])).Cast<HSE_INCIDENT_COMPONENT>().ToArray();
            if (primary.Length != 1 || primary[0].COMPONENT_TYPE != HseReferenceCodes.FieldContext.ComponentType || !CompanyPPDMEvidenceReader.Identifier(primary[0].FIELD_ID))
                throw RefusalException.Conflict("An incident requires exactly one canonical primary FIELD component.");
            if (!fields.Contains(primary[0].FIELD_ID) || !Current(primary[0])) continue;
            var headers = await reader.ReadAsync(typeof(HSE_INCIDENT), [CompanyPPDMEvidenceReader.Equal("INCIDENT_ID", incident), CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y")]);
            if (headers.Length > 1) throw RefusalException.Conflict("An incident header is ambiguous.");
            if (completePopulation && headers.Length == 0)
            {
                var history = await reader.ReadAsync(typeof(HSE_INCIDENT), [CompanyPPDMEvidenceReader.Equal("INCIDENT_ID", incident)]);
                if (history.Length != 1 || ((HSE_INCIDENT)history[0]).ACTIVE_IND != "N")
                    throw RefusalException.Conflict("An active FIELD association has missing or ambiguous incident-header evidence.");
            }
            if (headers.Length == 1 && Current(headers[0]))
            {
                var header = CompanyPPDMEvidenceReader.Fingerprint(headers); var association = CompanyPPDMEvidenceReader.Fingerprint(primary);
                var restrictions = await ReadIncidentRestrictionsAsync(reader, incident);
                if (!metadataReview && !await MayReadRestrictionsAsync(restrictions))
                {
                    if (completePopulation) throw RefusalException.Forbidden("The requested HSE total would omit withheld incident cases.");
                    continue;
                }
                var participants = type == typeof(HSE_INCIDENT_BA) ? await ReadParticipantRosterAsync(reader, incident, primary[0].FIELD_ID) : null;
                reviewed.Add((incident, primary[0].FIELD_ID, header, association, restrictions, participants));
            }
        }
        foreach (var value in reviewed)
        {
            var primary = await reader.ReadAsync(typeof(HSE_INCIDENT_COMPONENT), [CompanyPPDMEvidenceReader.Equal("INCIDENT_ID", value.Incident), CompanyPPDMEvidenceReader.Equal("COMPONENT_ROLE", HseReferenceCodes.FieldContext.PrimaryRole), CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y")]);
            var headers = await reader.ReadAsync(typeof(HSE_INCIDENT), [CompanyPPDMEvidenceReader.Equal("INCIDENT_ID", value.Incident), CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y")]);
            var restrictions = await ReadIncidentRestrictionsAsync(reader, value.Incident);
            var participants = type == typeof(HSE_INCIDENT_BA) ? await ReadParticipantRosterAsync(reader, value.Incident, value.Field) : null;
            if (participants?.Fingerprint != value.Participants?.Fingerprint) throw RefusalException.Conflict("The current participant roster or reference evidence changed during review.");
            if (CompanyPPDMEvidenceReader.Fingerprint(primary) != value.Primary || CompanyPPDMEvidenceReader.Fingerprint(headers) != value.Header ||
                restrictions.Components != value.Restrictions.Components || System.Text.Json.JsonSerializer.Serialize(restrictions.Definitions) != System.Text.Json.JsonSerializer.Serialize(value.Restrictions.Definitions) ||
                !metadataReview && !await MayReadRestrictionsAsync(restrictions))
                throw RefusalException.Forbidden("The current incident/restriction evidence changed during visibility review.");
        }
        foreach (var target in exactFieldEvidence)
        {
            var closingField = await reader.ReadAsync(typeof(FIELD), [CompanyPPDMEvidenceReader.Equal("FIELD_ID", target.Key), CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y")]);
            if (closingField.Length != 1 || !Current(closingField[0]) || CompanyPPDMEvidenceReader.Fingerprint(closingField) != target.Value)
                throw RefusalException.Forbidden("The exact incident FIELD changed during visibility review.");
        }
        if (completePopulation)
        {
            var freshCandidates = new HashSet<string>(StringComparer.Ordinal);
            var freshInventory = new List<string>();
            foreach (var field in fields)
            {
                var targets = await reader.ReadAsync(typeof(FIELD), [CompanyPPDMEvidenceReader.Equal("FIELD_ID", field), CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y")]);
                if (targets.Length != 1 || !Current(targets[0])) throw RefusalException.Conflict("The complete HSE FIELD target changed during review.");
                foreach (var component in (await reader.ReadAsync(typeof(HSE_INCIDENT_COMPONENT), [CompanyPPDMEvidenceReader.Equal("FIELD_ID", field), CompanyPPDMEvidenceReader.Equal("COMPONENT_ROLE", HseReferenceCodes.FieldContext.PrimaryRole), CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y")])).Cast<HSE_INCIDENT_COMPONENT>())
                { freshCandidates.Add(component.INCIDENT_ID); freshInventory.Add(CompanyPPDMEvidenceReader.CanonicalRecord(component)); }
            }
            if (!freshCandidates.SetEquals(candidates) || CompanyPPDMEvidenceReader.FingerprintRecords(freshInventory) != CompanyPPDMEvidenceReader.FingerprintRecords(inventory))
                throw RefusalException.Conflict("The complete HSE FIELD population changed during review.");
            foreach (var value in reviewed)
                if (!await MayReadRestrictionsAsync(value.Restrictions)) throw RefusalException.Forbidden("The complete HSE population lost a required case permission.");
        }
        await PathsAsync(type, type.Name);
        await reader.RequireCurrentAsync();
        var freshFields = Predicates(paths, await GrantsAsync(paths), PPDMRowMutation.Insert, read: true)
            .Select(group => group.Single().FilterValue).ToHashSet(StringComparer.Ordinal);
        if (completePopulation && (reviewField is null || !freshFields.Contains(reviewField) || reviewed.Any(value => !freshFields.Contains(value.Field))))
            throw RefusalException.Forbidden("The requested complete HSE FIELD population lost its current grant.");
        if (type == typeof(HSE_INCIDENT_BA))
            return reviewed.Where(value => freshFields.Contains(value.Field)).SelectMany(value => value.Participants!.Rows.Select(row =>
                (IReadOnlyList<AppFilter>)new[] { CompanyPPDMEvidenceReader.Equal("INCIDENT_ID", value.Incident), CompanyPPDMEvidenceReader.Equal("PARTY_OBS_NO", row.Party), CompanyPPDMEvidenceReader.Equal("PARTY_ROLE_OBS_NO", row.Role) })).ToArray();
        return reviewed.Where(value => freshFields.Contains(value.Field)).Select(value =>
            (IReadOnlyList<AppFilter>)(type == typeof(HSE_INCIDENT_COMPONENT)
                ? new[] { CompanyPPDMEvidenceReader.Equal("INCIDENT_ID", value.Incident), CompanyPPDMEvidenceReader.Equal("FIELD_ID", value.Field) }
                : new[] { CompanyPPDMEvidenceReader.Equal("INCIDENT_ID", value.Incident) })).ToArray();

        async Task ReferenceAsync(Type reference, string key, string code)
        {
            await CompanyPPDMEvidenceReader.DefinitionAsync(metadata, reference, [key]);
            var rows = await reader.ReadAsync(reference, [CompanyPPDMEvidenceReader.Equal(key, code), CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y")]);
            if (rows.Length != 1 || !Current(rows[0])) throw RefusalException.Conflict("The recorded HSE FIELD context reference vocabulary is unavailable or ambiguous.");
        }
    }

    public async Task RequireCompleteReadScopeAsync(Type type, string table, string fieldId)
    {
        if (!IncidentTable(type) || !CompanyPPDMEvidenceReader.Identifier(fieldId)) throw RefusalException.Conflict("Complete HSE population proof requires a canonical incident table.");
        var paths = await PathsAsync(type, table);
        await ReadPredicatesAsync(type, paths, await GrantsAsync(paths), completePopulation: true, reviewField: fieldId);
    }
    public async Task<HseRestrictionReview> ReviewHseRestrictionsAsync(FieldSelectionReview review, string incidentId)
    {
        if (!CompanyPPDMEvidenceReader.Identifier(incidentId) || !CompanyPPDMEvidenceReader.Identifier(review.FieldId) ||
            company.Company.UserId != review.ActorUserId || company.Company.Scope != review.Company || await company.ResolveAsync() != review.Database ||
            subjectUserId is not null && subjectUserId != review.ActorUserId)
            throw RefusalException.Forbidden("Restriction review requires the current actor/company/database/FIELD.");
        async Task RequireReviewerAsync()
        {
            var authority = await company.ResolveSubjectAsync(review.ActorUserId);
            if (authority?.Permissions.Contains(PermissionConstants.HSE.ViewRestricted, StringComparer.Ordinal) != true ||
                !authority.Permissions.Contains(PermissionConstants.HSE.ManageIncidents, StringComparer.Ordinal))
                throw RefusalException.Forbidden("Current HSE.ViewRestricted and HSE.ManageIncidents permissions are required to review restriction definitions.");
            if (await company.ResolveAsync() != review.Database || company.Company.Scope != review.Company || company.Company.UserId != review.ActorUserId)
                throw RefusalException.Conflict("Restriction-review scope changed.");
        }
        await RequireReviewerAsync();
        var paths = await PathsAsync(typeof(HSE_INCIDENT), "HSE_INCIDENT");
        async Task RequireIncidentAsync()
        {
            var predicates = await ReadPredicatesAsync(typeof(HSE_INCIDENT), paths, await GrantsAsync(paths), metadataReview: true, reviewIncident: incidentId, reviewField: review.FieldId);
            if (predicates.Count != 1) throw RefusalException.NotFound("The restriction-review incident is unavailable in the reviewed FIELD.");
        }
        await RequireIncidentAsync();
        var reader = await EvidenceReaderAsync();
        var evidence = await ReadIncidentRestrictionsAsync(reader, incidentId);
        await RequireIncidentAsync(); await RequireReviewerAsync();
        var fresh = await ReadIncidentRestrictionsAsync(reader, incidentId);
        if (fresh.Components != evidence.Components || System.Text.Json.JsonSerializer.Serialize(fresh.Definitions) != System.Text.Json.JsonSerializer.Serialize(evidence.Definitions))
            throw RefusalException.Conflict("Restriction-review evidence changed before delivery.");
        await RequireIncidentAsync(); await RequireReviewerAsync();
        return new(review.Company, review.Database, review.FieldId!, incidentId, fresh.Definitions);
    }

    private sealed record ParticipantRecord(decimal Party, decimal Role, string Payload);
    private sealed record ParticipantIdentity(ParticipantRecord Record, string PartyId, string PartyRole, string? Status, string? Company, string? Uwi, decimal Period, decimal Detail);
    private sealed record ParticipantRosterEvidence(ParticipantRecord[] Rows, string Fingerprint);
    private async Task<ParticipantRosterEvidence> ReadParticipantRosterAsync(CompanyPPDMEvidenceReader reader, string incident, string field)
    {
        await IncidentDefinitionAsync(typeof(HSE_INCIDENT_BA));
        var rows = (await reader.ReadAsync(typeof(HSE_INCIDENT_BA), [CompanyPPDMEvidenceReader.Equal("INCIDENT_ID", incident), CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y")])).Cast<HSE_INCIDENT_BA>().ToArray();
        if (rows.Any(row => !CompanyPPDMEvidenceReader.Observation(row.PARTY_OBS_NO) || !CompanyPPDMEvidenceReader.Observation(row.PARTY_ROLE_OBS_NO)) ||
            rows.Select(row => (row.PARTY_OBS_NO, row.PARTY_ROLE_OBS_NO)).Distinct().Count() != rows.Length)
            throw RefusalException.Conflict("The participant roster has incomplete or ambiguous full observation keys.");
        var rosterSnapshot = CompanyPPDMEvidenceReader.Fingerprint(rows);
        var evidence = rows.Select(CompanyPPDMEvidenceReader.CanonicalRecord).ToList();
        // Freeze every payload and identity before dependent provider awaits can mutate aliased rows.
        var current = rows.Where(Current).Select(row => new ParticipantIdentity(new ParticipantRecord(row.PARTY_OBS_NO, row.PARTY_ROLE_OBS_NO, CompanyPPDMEvidenceReader.CanonicalRecord(row)), row.INVOLVED_PARTY_BA_ID, row.INVOLVED_BA_ROLE, row.INVOLVED_BA_STATUS, row.COMPANY_BA_ID, row.UWI, row.PERIOD_OBS_NO, row.DETAIL_OBS_NO)).ToArray();
        await ValidateParticipantReferencesAsync(reader, current, field, evidence);
        await reader.RequireCurrentAsync();
        var finalRoster = await reader.ReadAsync(typeof(HSE_INCIDENT_BA), [CompanyPPDMEvidenceReader.Equal("INCIDENT_ID", incident), CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y")]);
        if (CompanyPPDMEvidenceReader.Fingerprint(finalRoster) != rosterSnapshot)
            throw RefusalException.Conflict("The stored participant roster changed during reference review.");
        return new(current.Select(item => item.Record).OrderBy(row => row.Party).ThenBy(row => row.Role).ToArray(), CompanyPPDMEvidenceReader.FingerprintRecords(evidence));
    }
    private async Task ValidateParticipantReferencesAsync(CompanyPPDMEvidenceReader reader, ParticipantIdentity[] current, string field, List<string> evidence)
    {
        async Task<object> Reference(Type type, string[] keys, AppFilter[] predicates)
        {
            await CompanyPPDMEvidenceReader.DefinitionAsync(metadata, type, keys);
            var found = await reader.ReadAsync(type, [.. predicates, CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y")]);
            if (found.Length != 1 || !Current(found[0])) throw RefusalException.Conflict("A recorded participant reference is missing, inactive or ambiguous.");
            evidence.Add(CompanyPPDMEvidenceReader.CanonicalRecord(found[0])); return found[0];
        }
        async Task Associate(string id)
        {
            if (!CompanyPPDMEvidenceReader.Identifier(id)) throw RefusalException.Conflict("The participant/company BA reference is incomplete.");
            await Reference(typeof(BUSINESS_ASSOCIATE), ["BUSINESS_ASSOCIATE_ID"], [CompanyPPDMEvidenceReader.Equal("BUSINESS_ASSOCIATE_ID", id)]);
        }
        foreach (var item in current)
        {
            var row = item;
            if (!CompanyPPDMEvidenceReader.Identifier(row.PartyRole) || !CompanyPPDMEvidenceReader.Observation(row.Period) || !CompanyPPDMEvidenceReader.Observation(row.Detail))
                throw RefusalException.Conflict("The participant role or crew observations are incomplete.");
            await Reference(typeof(R_INCIDENT_BA_ROLE), ["BA_ROLE"], [CompanyPPDMEvidenceReader.Equal("BA_ROLE", row.PartyRole)]);
            await Associate(row.PartyId);
            if (row.Status is not null && row.Status != "")
            {
                if (!CompanyPPDMEvidenceReader.Identifier(row.Status)) throw RefusalException.Conflict("The participant status reference is invalid.");
                await Reference(typeof(R_EMPLOYEE_STATUS), ["EMPLOYEE_STATUS"], [CompanyPPDMEvidenceReader.Equal("EMPLOYEE_STATUS", row.Status)]);
            }
            if (row.Company is not null && row.Company != "") await Associate(row.Company);
            if (row.Uwi is null || row.Uwi == "")
            {
                if (row.Period != 0 || row.Detail != 0) throw RefusalException.Conflict("A participant has orphan drilling-crew observations.");
                continue;
            }
            if (!CompanyPPDMEvidenceReader.Identifier(row.Uwi) || !CompanyPPDMEvidenceReader.Identifier(row.Company)) throw RefusalException.Conflict("The drilling-crew identity requires its actual WELL and company.");
            await CompanyPPDMEvidenceReader.DefinitionAsync(metadata, typeof(WELL_DRILL_PERIOD_CREW), ["UWI", "PERIOD_OBS_NO", "CREW_COMPANY_BA_ID", "DETAIL_OBS_NO"],
                ("CREW_COMPANY_BA_ID", "BUSINESS_ASSOCIATE", "BUSINESS_ASSOCIATE_ID"), ("CREW_MEMBER_BA_ID", "BUSINESS_ASSOCIATE", "BUSINESS_ASSOCIATE_ID"), ("UWI", "WELL_DRILL_PERIOD", "UWI"));
            var crew = (WELL_DRILL_PERIOD_CREW)await Reference(typeof(WELL_DRILL_PERIOD_CREW), ["UWI", "PERIOD_OBS_NO", "CREW_COMPANY_BA_ID", "DETAIL_OBS_NO"],
                [CompanyPPDMEvidenceReader.Equal("UWI", row.Uwi), CompanyPPDMEvidenceReader.Equal("PERIOD_OBS_NO", row.Period), CompanyPPDMEvidenceReader.Equal("CREW_COMPANY_BA_ID", row.Company!), CompanyPPDMEvidenceReader.Equal("DETAIL_OBS_NO", row.Detail)]);
            var member = crew.CREW_MEMBER_RECORD_IND == "Y" && (crew.CREW_RECORD_IND is null or "" or "N") && crew.CREW_MEMBER_BA_ID == row.PartyId;
            var whole = crew.CREW_RECORD_IND == "Y" && (crew.CREW_MEMBER_RECORD_IND is null or "" or "N") && (crew.CREW_MEMBER_BA_ID is null or "") && row.PartyId == row.Company;
            if (!member && !whole) throw RefusalException.Conflict("The incident participant is not the recorded crew member or explicitly recorded whole crew.");
            await CompanyPPDMEvidenceReader.DefinitionAsync(metadata, typeof(WELL_DRILL_PERIOD), ["UWI", "PERIOD_OBS_NO"], ("UWI", "WELL", "UWI"));
            await Reference(typeof(WELL_DRILL_PERIOD), ["UWI", "PERIOD_OBS_NO"], [CompanyPPDMEvidenceReader.Equal("UWI", row.Uwi), CompanyPPDMEvidenceReader.Equal("PERIOD_OBS_NO", row.Period)]);
            var well = (WELL)await Reference(typeof(WELL), ["UWI"], [CompanyPPDMEvidenceReader.Equal("UWI", row.Uwi)]);
            if (well.ASSIGNED_FIELD != field) throw RefusalException.Forbidden("The participant crew WELL differs from the incident's reviewed FIELD.");
            await ValidateRowsAsync(typeof(WELL), "WELL", [well]); // Existing canonical company/resource policy, not a promoted crew grant.
        }
    }
    public async Task<string> CaptureHseParticipantRosterAsync(FieldSelectionReview review, string incident)
    {
        var authority = await CaptureHseIncidentAuthorityAsync(review, incident);
        var reader = await EvidenceReaderAsync();
        var roster = await ReadParticipantRosterAsync(reader, incident, review.FieldId!);
        if (await CaptureHseIncidentAuthorityAsync(review, incident) != authority ||
            (await ReadParticipantRosterAsync(reader, incident, review.FieldId!)).Fingerprint != roster.Fingerprint)
            throw RefusalException.Conflict("Participant/reference evidence changed during capture.");
        return roster.Fingerprint;
    }

    public async Task<string> ValidatePreparedHseInvestigatorAsync(HSE_INCIDENT_BA candidate, string field)
    {
        await IncidentDefinitionAsync(typeof(HSE_INCIDENT_BA));
        if (!CompanyPPDMEvidenceReader.Identifier(field) || candidate.ACTIVE_IND != "Y" || !Current(candidate) ||
            !CompanyPPDMEvidenceReader.Observation(candidate.PARTY_OBS_NO) || !CompanyPPDMEvidenceReader.Observation(candidate.PARTY_ROLE_OBS_NO))
            throw RefusalException.Conflict("The prepared investigator has incomplete canonical identity or dates.");
        var frozen = CompanyPPDMEvidenceReader.CanonicalRecord(candidate);
        var item = new ParticipantIdentity(new ParticipantRecord(candidate.PARTY_OBS_NO, candidate.PARTY_ROLE_OBS_NO, frozen), candidate.INVOLVED_PARTY_BA_ID,
            candidate.INVOLVED_BA_ROLE, candidate.INVOLVED_BA_STATUS, candidate.COMPANY_BA_ID, candidate.UWI, candidate.PERIOD_OBS_NO, candidate.DETAIL_OBS_NO);
        var reader = await EvidenceReaderAsync(); var evidence = new List<string>();
        await ValidateParticipantReferencesAsync(reader, [item], field, evidence);
        await reader.RequireCurrentAsync();
        if (frozen != CompanyPPDMEvidenceReader.CanonicalRecord(candidate)) throw RefusalException.Conflict("The prepared investigator changed during reference review.");
        return CompanyPPDMEvidenceReader.FingerprintRecords(evidence);
    }
    public async Task<string> CaptureHseIncidentAuthorityAsync(FieldSelectionReview review, string incident)
    {
        if (!CompanyPPDMEvidenceReader.Identifier(incident) || review.ActorUserId != company.Company.UserId || review.Company != company.Company.Scope ||
            review.Database != await company.ResolveAsync() || !CompanyPPDMEvidenceReader.Identifier(review.FieldId) || subjectUserId is not null && subjectUserId != review.ActorUserId)
            throw RefusalException.Forbidden("HSE command evidence requires the current actor/company/database/FIELD.");
        var paths = await PathsAsync(typeof(HSE_INCIDENT), "HSE_INCIDENT");
        var predicates = await ReadPredicatesAsync(typeof(HSE_INCIDENT), paths, await GrantsAsync(paths), reviewIncident: incident, reviewField: review.FieldId);
        if (predicates.Count != 1) throw RefusalException.NotFound("The incident is unavailable in the exact reviewed FIELD.");
        var reader = await EvidenceReaderAsync();
        var header = await reader.ReadAsync(typeof(HSE_INCIDENT), [CompanyPPDMEvidenceReader.Equal("INCIDENT_ID", incident), CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y")]);
        var headerSnapshot = CompanyPPDMEvidenceReader.Fingerprint(header);
        var restrictions = await ReadIncidentRestrictionsAsync(reader, incident);
        if (header.Length != 1 || !Current(header[0]) || !await MayReadRestrictionsAsync(restrictions)) throw RefusalException.Forbidden("The HSE case authority changed.");
        var rules = configuration.Value.Companies.SingleOrDefault(value => value.BusinessAssociateId == review.Company.BusinessAssociateId)?.HseCaseReadRestrictions;
        var rulesSnapshot = System.Text.Json.JsonSerializer.Serialize(rules);
        var definitionsSnapshot = System.Text.Json.JsonSerializer.Serialize(restrictions.Definitions);
        var snapshot = CompanyPPDMEvidenceReader.FingerprintRecords([headerSnapshot, restrictions.Components,
            definitionsSnapshot, rulesSnapshot]);
        if ((await ReadPredicatesAsync(typeof(HSE_INCIDENT), paths, await GrantsAsync(paths), reviewIncident: incident, reviewField: review.FieldId)).Count != 1)
            throw RefusalException.Forbidden("The HSE case authority changed during capture.");
        var freshHeader = await reader.ReadAsync(typeof(HSE_INCIDENT), [CompanyPPDMEvidenceReader.Equal("INCIDENT_ID", incident), CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y")]);
        var freshRestrictions = await ReadIncidentRestrictionsAsync(reader, incident);
        if (CompanyPPDMEvidenceReader.Fingerprint(freshHeader) != headerSnapshot || freshRestrictions.Components != restrictions.Components ||
            System.Text.Json.JsonSerializer.Serialize(freshRestrictions.Definitions) != definitionsSnapshot ||
            System.Text.Json.JsonSerializer.Serialize(configuration.Value.Companies.SingleOrDefault(value => value.BusinessAssociateId == review.Company.BusinessAssociateId)?.HseCaseReadRestrictions) != rulesSnapshot ||
            !await MayReadRestrictionsAsync(freshRestrictions))
            throw RefusalException.Conflict("The stored case header or interpretation changed during command authority capture.");
        return snapshot;
    }
    private async Task ValidateParticipantPayloadAsync(IReadOnlyList<object> rows)
    {
        if (rows.Count == 0) return;
        var frozen = rows.Cast<HSE_INCIDENT_BA>().Select(row => new { row.INCIDENT_ID, Record = new ParticipantRecord(row.PARTY_OBS_NO, row.PARTY_ROLE_OBS_NO, CompanyPPDMEvidenceReader.CanonicalRecord(row)) }).ToArray();
        var reader = await EvidenceReaderAsync();
        foreach (var group in frozen.GroupBy(row => row.INCIDENT_ID, StringComparer.Ordinal))
        {
            if (!CompanyPPDMEvidenceReader.Identifier(group.Key)) throw RefusalException.Conflict("The participant incident identifier is incomplete.");
            var primary = (await reader.ReadAsync(typeof(HSE_INCIDENT_COMPONENT), [CompanyPPDMEvidenceReader.Equal("INCIDENT_ID", group.Key), CompanyPPDMEvidenceReader.Equal("COMPONENT_ROLE", HseReferenceCodes.FieldContext.PrimaryRole), CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y")])).Cast<HSE_INCIDENT_COMPONENT>().ToArray();
            if (primary.Length != 1 || !Current(primary[0]) || primary[0].COMPONENT_TYPE != HseReferenceCodes.FieldContext.ComponentType) throw RefusalException.Conflict("The participant's primary FIELD is unavailable or ambiguous.");
            var field = primary[0].FIELD_ID;
            var evidence = await ReadParticipantRosterAsync(reader, group.Key, field);
            foreach (var row in group)
                if (!evidence.Rows.Any(record => record == row.Record))
                    throw RefusalException.Forbidden("The returned participant differs from the current canonical roster payload.");
            var fresh = await ReadParticipantRosterAsync(reader, group.Key, field);
            if (fresh.Fingerprint != evidence.Fingerprint) throw RefusalException.Conflict("Participant/reference evidence changed before payload delivery.");
            if (!frozen.Select(value => value.Record.Payload).SequenceEqual(rows.Select(CompanyPPDMEvidenceReader.CanonicalRecord), StringComparer.Ordinal))
                throw RefusalException.Conflict("The participant payload changed during reference validation.");
        }
    }

    private sealed record IncidentRestrictionEvidence(string Components, HseRestrictionDefinitionReview[] Definitions);
    private async Task<IncidentRestrictionEvidence> ReadIncidentRestrictionsAsync(CompanyPPDMEvidenceReader reader, string incident)
    {
        var components = (await reader.ReadAsync(typeof(HSE_INCIDENT_COMPONENT), [CompanyPPDMEvidenceReader.Equal("INCIDENT_ID", incident), CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y")])).Cast<HSE_INCIDENT_COMPONENT>().ToArray();
        if (components.Any(row => !CompanyPPDMEvidenceReader.Observation(row.COMPONENT_OBS_NO)) || components.Select(row => row.COMPONENT_OBS_NO).Distinct().Count() != components.Length)
            throw RefusalException.Conflict("The complete incident component identities are ambiguous.");
        var componentEvidence = CompanyPPDMEvidenceReader.Fingerprint(components);
        var definitions = new List<HseRestrictionDefinitionReview>();
        foreach (var component in components)
        {
            if (component.RESTRICTION_ID is null || component.RESTRICTION_ID == "")
            {
                if (component.RESTRICTION_VERSION != 0) throw RefusalException.Conflict("An incident component has an orphan restriction version.");
                continue;
            }
            if (!CompanyPPDMEvidenceReader.Identifier(component.RESTRICTION_ID) || !CompanyPPDMEvidenceReader.Observation(component.RESTRICTION_VERSION) || !Current(component))
                throw RefusalException.Conflict("The incident restriction association has incomplete current identity.");
            if (!definitions.Any(row => row.RestrictionId == component.RESTRICTION_ID && row.RestrictionVersion == component.RESTRICTION_VERSION))
                definitions.Add(new(component.RESTRICTION_ID, component.RESTRICTION_VERSION, await ReadRestrictionDefinitionAsync(reader, component.RESTRICTION_ID, component.RESTRICTION_VERSION)));
        }
        return new(componentEvidence, definitions.OrderBy(row => row.RestrictionId, StringComparer.Ordinal).ThenBy(row => row.RestrictionVersion).ToArray());
    }
    private async Task<string> ReadRestrictionDefinitionAsync(CompanyPPDMEvidenceReader reader, string id, decimal version)
    {
        await CompanyPPDMEvidenceReader.DefinitionAsync(metadata, typeof(RESTRICTION), ["RESTRICTION_ID", "RESTRICTION_VERSION"],
            ("RESTRICTION_CLASS", "REST_CLASS", "RESTRICTION_CLASS_ID"), ("RESTRICTION_TYPE", "R_REST_TYPE", "RESTRICTION_TYPE"), ("REST_DURATION_TYPE", "R_REST_DURATION", "REST_DURATION_TYPE"));
        var rows = (await reader.ReadAsync(typeof(RESTRICTION), [CompanyPPDMEvidenceReader.Equal("RESTRICTION_ID", id), CompanyPPDMEvidenceReader.Equal("RESTRICTION_VERSION", version), CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y")])).Cast<RESTRICTION>().ToArray();
        if (rows.Length != 1 || !Current(rows[0]) || rows[0].EFFECTIVE_DATE is null || !CompanyPPDMEvidenceReader.Identifier(rows[0].RESTRICTION_CLASS) || !CompanyPPDMEvidenceReader.Identifier(rows[0].RESTRICTION_TYPE))
            throw RefusalException.Conflict("One current recorded restriction version and classification are required.");
        var header = rows[0];
        foreach (var value in new[] { header.EFFECTIVE_DATE, header.EXPIRY_DATE, header.START_DATE, header.END_DATE, header.START_DATE_EVENT, header.END_DATE_EVENT })
            if (value is DateTime date && date.Kind != DateTimeKind.Utc) throw RefusalException.Conflict("Restriction date evidence requires explicit UTC boundaries.");
        if (!CompanyPPDMEvidenceReader.Range(header.START_DATE, header.END_DATE, TimeZoneInfo.Utc, TimeProvider.System, required: false) || header.START_DATE_EVENT is not null || header.END_DATE_EVENT is not null)
            throw RefusalException.Conflict("Event-dependent or out-of-window restrictions require a reviewed date interpreter.");
        var evidence = new List<string> { CompanyPPDMEvidenceReader.CanonicalRecord(header) };
        async Task Reference(Type type, string key, string code)
        {
            await CompanyPPDMEvidenceReader.DefinitionAsync(metadata, type, [key]);
            var found = await reader.ReadAsync(type, [CompanyPPDMEvidenceReader.Equal(key, code), CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y")]);
            if (found.Length != 1 || !Current(found[0])) throw RefusalException.Conflict("The restriction reference definition is unavailable or ambiguous.");
            evidence.Add(CompanyPPDMEvidenceReader.CanonicalRecord(found[0]));
        }
        await Reference(typeof(REST_CLASS), "RESTRICTION_CLASS_ID", header.RESTRICTION_CLASS);
        await Reference(typeof(R_REST_TYPE), "RESTRICTION_TYPE", header.RESTRICTION_TYPE);
        if (header.REST_DURATION_TYPE is not null) {
            if (!CompanyPPDMEvidenceReader.Identifier(header.REST_DURATION_TYPE)) throw RefusalException.Conflict("The recorded restriction duration is invalid.");
            await Reference(typeof(R_REST_DURATION), "REST_DURATION_TYPE", header.REST_DURATION_TYPE);
        }
        await CompanyPPDMEvidenceReader.DefinitionAsync(metadata, typeof(REST_ACTIVITY), ["RESTRICTION_ID", "RESTRICTION_VERSION", "RESTRICTED_ACTIVITY"], ("RESTRICTION_ID", "RESTRICTION", "RESTRICTION_ID"), ("RESTRICTED_ACTIVITY", "R_REST_ACTIVITY", "RESTRICTED_ACTIVITY"));
        var activities = (await reader.ReadAsync(typeof(REST_ACTIVITY), [CompanyPPDMEvidenceReader.Equal("RESTRICTION_ID", id), CompanyPPDMEvidenceReader.Equal("RESTRICTION_VERSION", version), CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y")])).Cast<REST_ACTIVITY>().ToArray();
        if (activities.Any(row => !CompanyPPDMEvidenceReader.Identifier(row.RESTRICTED_ACTIVITY) || !Current(row)) || activities.Select(row => row.RESTRICTED_ACTIVITY).Distinct(StringComparer.Ordinal).Count() != activities.Length)
            throw RefusalException.Conflict("Recorded restriction activities are incomplete or ambiguous.");
        evidence.AddRange(activities.Select(CompanyPPDMEvidenceReader.CanonicalRecord));
        foreach (var activity in activities) { await Reference(typeof(R_REST_ACTIVITY), "RESTRICTED_ACTIVITY", activity.RESTRICTED_ACTIVITY); }
        await CompanyPPDMEvidenceReader.DefinitionAsync(metadata, typeof(REST_TEXT), ["RESTRICTION_ID", "RESTRICTION_VERSION", "TEXT_SEQ_NO"], ("RESTRICTION_ID", "RESTRICTION", "RESTRICTION_ID"));
        var texts = (await reader.ReadAsync(typeof(REST_TEXT), [CompanyPPDMEvidenceReader.Equal("RESTRICTION_ID", id), CompanyPPDMEvidenceReader.Equal("RESTRICTION_VERSION", version), CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y")])).Cast<REST_TEXT>().ToArray();
        if (texts.Any(row => !CompanyPPDMEvidenceReader.Observation(row.TEXT_SEQ_NO) || !Current(row)) || texts.Select(row => row.TEXT_SEQ_NO).Distinct().Count() != texts.Length)
            throw RefusalException.Conflict("Recorded restriction text identities are incomplete or ambiguous.");
        evidence.AddRange(texts.Select(CompanyPPDMEvidenceReader.CanonicalRecord));
        // Human-reviewed text is fingerprinted, never interpreted as executable permissions.
        await reader.RequireCurrentAsync();
        return CompanyPPDMEvidenceReader.FingerprintRecords(evidence);
    }
    private async Task<bool> MayReadRestrictionsAsync(IncidentRestrictionEvidence evidence)
    {
        if (evidence.Definitions.Length == 0) return true;
        var database = await company.ResolveAsync(); var scope = company.Company.Scope;
        var companies = (configuration.Value.Companies ?? throw RefusalException.Conflict("Company restriction interpretation is unavailable.")).Where(rule => rule is not null && rule.TenantId == scope.TenantId && rule.BusinessAssociateId == scope.BusinessAssociateId).ToArray();
        if (companies.Length != 1 || companies[0].HseCaseReadRestrictions is null || companies[0].HseCaseReadRestrictions.Any(rule => rule is null)) throw RefusalException.Conflict("Configure one explicit company HSE restriction interpretation.");
        // Freeze complete reviewed bindings before any role-provider await.
        var rules = companies[0].HseCaseReadRestrictions.Select(rule => new HseCaseRestrictionReadRule { DatabaseFingerprint = rule.DatabaseFingerprint,
            RestrictionId = rule.RestrictionId, RestrictionVersion = rule.RestrictionVersion, DefinitionFingerprint = rule.DefinitionFingerprint, RequiredPermission = rule.RequiredPermission }).ToArray();
        var captured = System.Text.Json.JsonSerializer.Serialize(rules);
        var authority = await company.ResolveSubjectAsync(subjectUserId ?? company.Company.UserId);
        var allowed = true;
        foreach (var definition in evidence.Definitions)
        {
            var bindings = rules.Where(rule => rule.DatabaseFingerprint == database.Fingerprint && rule.RestrictionId == definition.RestrictionId && rule.RestrictionVersion == definition.RestrictionVersion).ToArray();
            if (bindings.Length != 1 || bindings[0].DefinitionFingerprint != definition.DefinitionFingerprint || bindings[0].RequiredPermission is not (PermissionConstants.HSE.View or PermissionConstants.HSE.ViewRestricted))
                throw RefusalException.Conflict("The restriction has no unique current reviewed definition and registered HSE read permission.");
            allowed &= authority?.Permissions.Contains(bindings[0].RequiredPermission, StringComparer.Ordinal) == true;
        }
        var freshAuthority = await company.ResolveSubjectAsync(subjectUserId ?? company.Company.UserId);
        if (authority is null || freshAuthority is null || !authority.Permissions.Order(StringComparer.Ordinal).SequenceEqual(freshAuthority.Permissions.Order(StringComparer.Ordinal)))
            throw RefusalException.Forbidden("The restriction subject permissions changed during authorization.");
        var current = (configuration.Value.Companies ?? throw RefusalException.Conflict("Company restriction interpretation is unavailable.")).Where(rule => rule is not null && rule.TenantId == scope.TenantId && rule.BusinessAssociateId == scope.BusinessAssociateId).ToArray();
        if (current.Length != 1 || System.Text.Json.JsonSerializer.Serialize(current[0].HseCaseReadRestrictions) != captured || await company.ResolveAsync() != database)
            throw RefusalException.Conflict("The reviewed restriction interpretation changed during authorization.");
        return allowed;
    }

    private async Task ProjectDefinitionAsync(Type type)
    {
        string[] keys = type.Name switch
        {
            "PROJECT" => ["PROJECT_ID"], "PROJECT_COMPONENT" => ["PROJECT_ID", "COMPONENT_OBS_NO"],
            "PROJECT_STEP" => ["PROJECT_ID", "STEP_ID"], "PROJECT_CONDITION" => ["PROJECT_ID", "CONDITION_OBS_NO"],
            "PROJECT_STATUS" => ["PROJECT_ID", "STATUS_ID"],
            "PROJECT_STEP_BA" => ["PROJECT_ID", "BUSINESS_ASSOCIATE_ID", "ROLE", "ROLE_SEQ_NO", "STEP_ID", "STEP_BA_OBS_NO"],
            _ => throw RefusalException.Forbidden("This table has no reviewed HSE project relationship.")
        };
        await CompanyPPDMEvidenceReader.DefinitionAsync(metadata, type, keys,
            type == typeof(PROJECT) ? [] : type == typeof(PROJECT_STEP_BA)
                ? [("PROJECT_ID", "PROJECT_STEP", "PROJECT_ID"), ("PROJECT_ID", "PROJECT_BA_ROLE", "PROJECT_ID")]
                : [("PROJECT_ID", "PROJECT", "PROJECT_ID")]);
        if (type.GetProperty("PROJECT_ID")?.PropertyType != typeof(string)) throw RefusalException.Conflict("The canonical project identifier is unavailable.");
    }

    private async Task<IReadOnlyList<IReadOnlyList<AppFilter>>> HazopPredicatesAsync(Type type, Path[] paths, List<AppUserAssetAccess> grants)
    {
        var fields = Predicates(paths, grants, PPDMRowMutation.Insert, read: true).Select(group => group.Single().FilterValue).ToHashSet(StringComparer.Ordinal);
        if (fields.Count == 0) return [];
        await ProjectDefinitionAsync(typeof(PROJECT));
        await CompanyPPDMEvidenceReader.DefinitionAsync(metadata, typeof(PROJECT_COMPONENT), ["PROJECT_ID", "COMPONENT_OBS_NO"],
            ("PROJECT_ID", "PROJECT", "PROJECT_ID"), ("FIELD_ID", "FIELD", "FIELD_ID"));
        await CompanyPPDMEvidenceReader.DefinitionAsync(metadata, typeof(PROJECT), ["PROJECT_ID"], ("PROJECT_TYPE", "R_PROJECT_TYPE", "PROJECT_TYPE"));
        await CompanyPPDMEvidenceReader.DefinitionAsync(metadata, typeof(FIELD), ["FIELD_ID"]);
        var reader = await EvidenceReaderAsync();
        var candidates = new HashSet<string>(StringComparer.Ordinal);
        foreach (var field in fields)
        {
            var targets = await reader.ReadAsync(typeof(FIELD), [CompanyPPDMEvidenceReader.Equal("FIELD_ID", field), CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y")]);
            if (targets.Length > 1) throw RefusalException.Conflict("The HAZOP FIELD target is ambiguous.");
            if (targets.Length == 0) continue;
            foreach (var component in (await reader.ReadAsync(typeof(PROJECT_COMPONENT),
                [CompanyPPDMEvidenceReader.Equal("FIELD_ID", field), CompanyPPDMEvidenceReader.Equal("FIELD_SOURCE", HseReferenceCodes.Hazop.FieldContextSource), CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y")])).Cast<PROJECT_COMPONENT>())
            {
                if (!CompanyPPDMEvidenceReader.Identifier(component.PROJECT_ID)) throw RefusalException.Conflict("A HAZOP FIELD context has an invalid project identifier.");
                candidates.Add(component.PROJECT_ID);
                if (candidates.Count > 256) throw RefusalException.Conflict("Narrow the assigned HAZOP FIELD evidence lookup.");
            }
        }
        if (candidates.Count == 0) return [];
        var reviewed = new List<(string Project, string Field)>();
        foreach (var project in candidates)
        {
            var parents = await reader.ReadAsync(typeof(PROJECT), [CompanyPPDMEvidenceReader.Equal("PROJECT_ID", project), CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y")]);
            if (parents.Length > 1) throw RefusalException.Conflict("A HAZOP project header is ambiguous.");
            if (parents.Length == 0 || ((PROJECT)parents[0]).PROJECT_TYPE != HseReferenceCodes.Hazop.ProjectType || !Current(parents[0])) continue;
            // A selected FIELD context cannot hide another active FIELD association, regardless of its source marker.
            var components = (await reader.ReadAsync(typeof(PROJECT_COMPONENT), [CompanyPPDMEvidenceReader.Equal("PROJECT_ID", project), CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y")]))
                .Cast<PROJECT_COMPONENT>().Where(row => !string.IsNullOrWhiteSpace(row.FIELD_ID) || row.FIELD_SOURCE == HseReferenceCodes.Hazop.FieldContextSource).ToArray();
            if (components.Length != 1 || !CompanyPPDMEvidenceReader.Identifier(components[0].FIELD_ID) || components[0].FIELD_SOURCE != HseReferenceCodes.Hazop.FieldContextSource || components[0].SOURCE != HseReferenceCodes.Hazop.FieldContextSource)
                throw RefusalException.Conflict("A HAZOP study requires exactly one recorded canonical FIELD context.");
            if (!fields.Contains(components[0].FIELD_ID) || !Current(components[0])) continue;
            reviewed.Add((project, components[0].FIELD_ID));
        }
        if (reviewed.Count == 0) return [];
        await ReferenceAsync(reader, typeof(R_PROJECT_TYPE), ["PROJECT_TYPE"], [CompanyPPDMEvidenceReader.Equal("PROJECT_TYPE", HseReferenceCodes.Hazop.ProjectType)]);
        if (type == typeof(PROJECT_STEP))
        {
            await CompanyPPDMEvidenceReader.DefinitionAsync(metadata, type, ["PROJECT_ID", "STEP_ID"], ("STEP_TYPE", "R_PROJ_STEP_TYPE", "STEP_TYPE"));
            await ReferenceAsync(reader, typeof(R_PROJ_STEP_TYPE), ["STEP_TYPE"], [CompanyPPDMEvidenceReader.Equal("STEP_TYPE", HseReferenceCodes.Hazop.NodeType)]);
        }
        var freshFields = Predicates(paths, await GrantsAsync(paths), PPDMRowMutation.Insert, read: true).Select(group => group.Single().FilterValue).ToHashSet(StringComparer.Ordinal);
        await reader.RequireCurrentAsync();
        var result = new List<IReadOnlyList<AppFilter>>();
        foreach (var parent in reviewed.Where(value => freshFields.Contains(value.Field)))
        {
            var group = new List<AppFilter> { CompanyPPDMEvidenceReader.Equal("PROJECT_ID", parent.Project) };
            if (type == typeof(PROJECT)) group.Add(CompanyPPDMEvidenceReader.Equal("PROJECT_TYPE", HseReferenceCodes.Hazop.ProjectType));
            if (type == typeof(PROJECT_STEP)) group.Add(CompanyPPDMEvidenceReader.Equal("STEP_TYPE", HseReferenceCodes.Hazop.NodeType));
            if (type == typeof(PROJECT_COMPONENT)) group.AddRange([CompanyPPDMEvidenceReader.Equal("FIELD_ID", parent.Field), CompanyPPDMEvidenceReader.Equal("FIELD_SOURCE", HseReferenceCodes.Hazop.FieldContextSource), CompanyPPDMEvidenceReader.Equal("SOURCE", HseReferenceCodes.Hazop.FieldContextSource)]);
            if (type == typeof(PROJECT_STATUS))
            {
                foreach (var status in new[] { HseReferenceCodes.Hazop.StudyStatusType, HseReferenceCodes.Hazop.DeviationStatusType })
                    result.Add([.. group, CompanyPPDMEvidenceReader.Equal("STATUS_TYPE", status)]);
            }
            else result.Add(group);
        }
        if (result.Count > 256) throw RefusalException.Conflict("Narrow the assigned HAZOP project predicate groups.");
        return result;
    }

    private async Task<IReadOnlyList<IReadOnlyList<AppFilter>>> ExactProjectPredicatesAsync(Type type, Path[] paths, List<AppUserAssetAccess> grants, string project)
    {
        if (Predicates(paths, grants, PPDMRowMutation.Insert, read: true).Count == 0) return [];
        await ProjectDefinitionAsync(typeof(PROJECT));
        var reader = await EvidenceReaderAsync();
        var parents = await reader.ReadAsync(typeof(PROJECT), [CompanyPPDMEvidenceReader.Equal("PROJECT_ID", project), CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y")]);
        if (parents.Length > 1) throw RefusalException.Conflict("The exact project header is ambiguous.");
        if (parents.Length == 0 || !Current(parents[0])) return [];
        var snapshot = CompanyPPDMEvidenceReader.Fingerprint(parents);
        var projectType = ((PROJECT)parents[0]).PROJECT_TYPE;
        IReadOnlyList<IReadOnlyList<AppFilter>> result = projectType switch
        {
            HseReferenceCodes.CorrectiveAction.ProjectType when type == typeof(PROJECT) || type == typeof(PROJECT_STEP) || type == typeof(PROJECT_STATUS) || type == typeof(PROJECT_STEP_BA)
                => await CorrectivePredicatesAsync(type, paths, grants, project),
            HseReferenceCodes.Hazop.ProjectType when type != typeof(PROJECT_STEP_BA)
                => (await HazopPredicatesAsync(type, paths, grants)).Where(group => group.Any(filter => filter.FieldName == "PROJECT_ID" && filter.FilterValue == project)).ToArray(),
            _ => []
        };
        var closing = await reader.ReadAsync(typeof(PROJECT), [CompanyPPDMEvidenceReader.Equal("PROJECT_ID", project), CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y")]);
        if (CompanyPPDMEvidenceReader.Fingerprint(closing) != snapshot || closing.Length != 1 || !Current(closing[0]))
            throw RefusalException.Conflict("The exact project header changed during read authorization.");
        return result;
    }

    private async Task<IReadOnlyList<IReadOnlyList<AppFilter>>> CorrectivePredicatesAsync(Type type, Path[] paths, List<AppUserAssetAccess> grants, string? reviewProject = null)
    {
        var fields = Predicates(paths, grants, PPDMRowMutation.Insert, read: true).Select(group => group.Single().FilterValue).ToHashSet(StringComparer.Ordinal);
        if (fields.Count == 0) return [];
        await CompanyPPDMEvidenceReader.DefinitionAsync(metadata, typeof(HSE_INCIDENT_COMPONENT), ["INCIDENT_ID", "COMPONENT_OBS_NO"], ("FIELD_ID", "FIELD", "FIELD_ID"), ("INCIDENT_ID", "HSE_INCIDENT", "INCIDENT_ID"));
        await CompanyPPDMEvidenceReader.DefinitionAsync(metadata, typeof(HSE_INCIDENT_RESPONSE), ["INCIDENT_ID", "RESPONSE_OBS_NO"],
            ("INCIDENT_ID", "HSE_INCIDENT", "INCIDENT_ID"), ("PROJECT_ID", "PROJECT", "PROJECT_ID"), ("RESPONSE_TYPE", "R_HSE_RESPONSE_TYPE", "ACTION_TYPE"));
        await CompanyPPDMEvidenceReader.DefinitionAsync(metadata, typeof(PROJECT), ["PROJECT_ID"], ("PROJECT_TYPE", "R_PROJECT_TYPE", "PROJECT_TYPE"));
        var reader = await EvidenceReaderAsync();
        var incidents = new HashSet<string>(StringComparer.Ordinal);
        if (reviewProject is not null)
        {
            var links = (await reader.ReadAsync(typeof(HSE_INCIDENT_RESPONSE), [CompanyPPDMEvidenceReader.Equal("PROJECT_ID", reviewProject), CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y")])).Cast<HSE_INCIDENT_RESPONSE>().ToArray();
            if (links.Length != 1 || !CompanyPPDMEvidenceReader.Identifier(links[0].INCIDENT_ID) || !CompanyPPDMEvidenceReader.Identifier(links[0].RESPONSE_TYPE))
                throw RefusalException.Conflict("The exact corrective project requires one current incident response link.");
            if (!Current(links[0])) return [];
            incidents.Add(links[0].INCIDENT_ID);
        }
        else
        {
        foreach (var field in fields)
            foreach (var component in (await reader.ReadAsync(typeof(HSE_INCIDENT_COMPONENT), [CompanyPPDMEvidenceReader.Equal("FIELD_ID", field),
                CompanyPPDMEvidenceReader.Equal("COMPONENT_ROLE", HseReferenceCodes.FieldContext.PrimaryRole), CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y")])).Cast<HSE_INCIDENT_COMPONENT>())
            {
                if (!CompanyPPDMEvidenceReader.Identifier(component.INCIDENT_ID)) throw RefusalException.Conflict("A corrective-action incident candidate has an invalid identity.");
                incidents.Add(component.INCIDENT_ID);
                if (incidents.Count > 256) throw RefusalException.Conflict("Narrow the assigned corrective-action incident candidates.");
            }
        }
        var candidates = new HashSet<string>(StringComparer.Ordinal);
        if (reviewProject is not null) candidates.Add(reviewProject);
        else
        {
        foreach (var incident in incidents)
            foreach (var link in (await reader.ReadAsync(typeof(HSE_INCIDENT_RESPONSE), [CompanyPPDMEvidenceReader.Equal("INCIDENT_ID", incident), CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y")])).Cast<HSE_INCIDENT_RESPONSE>())
            {
                if (string.IsNullOrEmpty(link.PROJECT_ID)) continue;
                if (!CompanyPPDMEvidenceReader.Identifier(link.PROJECT_ID)) throw RefusalException.Conflict("The incident project link has an invalid project identity.");
                var parents = await reader.ReadAsync(typeof(PROJECT), [CompanyPPDMEvidenceReader.Equal("PROJECT_ID", link.PROJECT_ID), CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y")]);
                if (parents.Length > 1) throw RefusalException.Conflict("The linked corrective-action project is ambiguous.");
                if (parents.Length == 1 && ((PROJECT)parents[0]).PROJECT_TYPE == HseReferenceCodes.CorrectiveAction.ProjectType && Current(parents[0])) candidates.Add(link.PROJECT_ID);
                if (candidates.Count > 256) throw RefusalException.Conflict("Narrow the assigned corrective-action project candidates.");
            }
        }
        if (candidates.Count == 0) return [];
        await ReferenceAsync(reader, typeof(R_PROJECT_TYPE), ["PROJECT_TYPE"], [CompanyPPDMEvidenceReader.Equal("PROJECT_TYPE", HseReferenceCodes.CorrectiveAction.ProjectType)]);
        var exactIncident = reviewProject is not null ? incidents.Single() : null;
        var authorizedIncidents = (await ReadPredicatesAsync(typeof(HSE_INCIDENT), [new("INCIDENT_ID", "FIELD", true)], grants, reviewIncident: exactIncident))
            .Select(group => group.Single().FilterValue).ToHashSet(StringComparer.Ordinal);
        var result = new List<IReadOnlyList<AppFilter>>();
        var reviewedLinks = new Dictionary<string, (string Incident, decimal Observation, string Type, DateTime? Effective, DateTime? Expiry)>(StringComparer.Ordinal);
        foreach (var project in candidates)
        {
            // Query all active incident links for this actual project; an authorized incident must not hide another incident's link.
            var links = (await reader.ReadAsync(typeof(HSE_INCIDENT_RESPONSE), [CompanyPPDMEvidenceReader.Equal("PROJECT_ID", project), CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y")])).Cast<HSE_INCIDENT_RESPONSE>().ToArray();
            if (links.Length != 1 || !CompanyPPDMEvidenceReader.Identifier(links[0].INCIDENT_ID) || !CompanyPPDMEvidenceReader.Identifier(links[0].RESPONSE_TYPE))
                throw RefusalException.Conflict("The corrective-action project requires one exact current incident response link.");
            if (!authorizedIncidents.Contains(links[0].INCIDENT_ID) || !Current(links[0])) continue;
            var reviewedLink = (links[0].INCIDENT_ID, links[0].RESPONSE_OBS_NO, links[0].RESPONSE_TYPE, links[0].EFFECTIVE_DATE, links[0].EXPIRY_DATE);
            await ReferenceAsync(reader, typeof(R_HSE_RESPONSE_TYPE), ["ACTION_TYPE"], [CompanyPPDMEvidenceReader.Equal("ACTION_TYPE", links[0].RESPONSE_TYPE)]);
            var group = new List<AppFilter> { CompanyPPDMEvidenceReader.Equal("PROJECT_ID", project) };
            if (type == typeof(PROJECT)) group.Add(CompanyPPDMEvidenceReader.Equal("PROJECT_TYPE", HseReferenceCodes.CorrectiveAction.ProjectType));
            if (type == typeof(PROJECT_STATUS)) group.Add(CompanyPPDMEvidenceReader.Equal("STATUS_TYPE", HseReferenceCodes.CorrectiveAction.StatusType));
            if (type == typeof(PROJECT_STEP_BA)) group.Add(CompanyPPDMEvidenceReader.Equal("ROLE", HseReferenceCodes.CorrectiveAction.ResponsibleRole));
            reviewedLinks.Add(project, reviewedLink);
            result.Add(group);
        }
        // Rebuild incident authority after response/reference reads to prevent a grant revoked while proving links retaining the CA scope.
        var fresh = (await ReadPredicatesAsync(typeof(HSE_INCIDENT), [new("INCIDENT_ID", "FIELD", true)], await GrantsAsync(paths), reviewIncident: exactIncident))
            .Select(group => group.Single().FilterValue).ToHashSet(StringComparer.Ordinal);
        foreach (var group in result.ToArray())
        {
            var project = group[0].FilterValue;
            var links = (await reader.ReadAsync(typeof(HSE_INCIDENT_RESPONSE), [CompanyPPDMEvidenceReader.Equal("PROJECT_ID", project), CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y")])).Cast<HSE_INCIDENT_RESPONSE>().ToArray();
            if (links.Length != 1 || !fresh.Contains(links[0].INCIDENT_ID) || !Current(links[0]) || reviewedLinks[project] != (links[0].INCIDENT_ID, links[0].RESPONSE_OBS_NO, links[0].RESPONSE_TYPE, links[0].EFFECTIVE_DATE, links[0].EXPIRY_DATE)) throw RefusalException.Forbidden("The corrective-action incident relationship changed during evidence review.");
        }
        await PathsAsync(type, type.Name);
        await reader.RequireCurrentAsync();
        return result;
    }

    // Reference preparation only: this grants no generic mutation authority. The typed command owns
    // the exact one-use insert permit, and saved rows still pass normal persisted-node validation.
    internal async Task ValidatePreparedCorrectiveActionAsync(PROJECT_STEP step, PROJECT_STATUS status, PROJECT_STEP_BA responsible)
    {
        if (step.PROJECT_ID != status.PROJECT_ID || step.PROJECT_ID != responsible.PROJECT_ID || step.STEP_ID != status.STEP_ID || step.STEP_ID != responsible.STEP_ID ||
            status.STATUS_TYPE != HseReferenceCodes.CorrectiveAction.StatusType || status.STATUS != HseReferenceCodes.CorrectiveAction.Open || responsible.ROLE != HseReferenceCodes.CorrectiveAction.ResponsibleRole)
            throw RefusalException.Conflict("The prepared corrective-action rows do not share one complete relationship.");
        await CompanyPPDMEvidenceReader.DefinitionAsync(metadata, typeof(PROJECT_STEP_BA), ["PROJECT_ID", "BUSINESS_ASSOCIATE_ID", "ROLE", "ROLE_SEQ_NO", "STEP_ID", "STEP_BA_OBS_NO"], ("PROJECT_ID", "PROJECT_STEP", "PROJECT_ID"), ("PROJECT_ID", "PROJECT_BA_ROLE", "PROJECT_ID"));
        await ValidateProjectReferencesAsync(typeof(PROJECT_STEP), [step], step);
        await ValidateProjectReferencesAsync(typeof(PROJECT_STATUS), [status], step);
        await ValidateProjectReferencesAsync(typeof(PROJECT_STEP_BA), [responsible], step);
    }

    private async Task ValidateProjectReferencesAsync(Type type, IReadOnlyList<object> rows, PROJECT_STEP? preparedStep = null)
    {
        if (rows.Count == 0) return;
        var reader = await EvidenceReaderAsync();
        foreach (var row in rows)
        {
            var project = Value(row, "PROJECT_ID");
            if (!CompanyPPDMEvidenceReader.Identifier(project)) throw RefusalException.Conflict("The HSE child has no project identity.");
            var parentRows = await reader.ReadAsync(typeof(PROJECT), [CompanyPPDMEvidenceReader.Equal("PROJECT_ID", project!), CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y")]);
            if (parentRows.Length != 1 || !Current(parentRows[0])) throw RefusalException.Conflict("The HSE project parent is unavailable or ambiguous.");
            var parentType = ((PROJECT)parentRows[0]).PROJECT_TYPE;
            if (parentType is not (HseReferenceCodes.Hazop.ProjectType or HseReferenceCodes.CorrectiveAction.ProjectType)) throw RefusalException.Conflict("The HSE project type is unsupported.");
            await ReferenceAsync(reader, typeof(R_PROJECT_TYPE), ["PROJECT_TYPE"], [CompanyPPDMEvidenceReader.Equal("PROJECT_TYPE", parentType)]);
            if (row is PROJECT_STEP node) await RequireProjectNodeAsync(reader, project!, node.STEP_ID, parentType, preparedStep);
            if (row is PROJECT_CONDITION condition)
            {
                if (parentType != HseReferenceCodes.Hazop.ProjectType || condition.PROJECT_TYPE != parentType || !CompanyPPDMEvidenceReader.Identifier(condition.CONDITION_TYPE)) throw RefusalException.Conflict("The HAZOP guide word is unavailable.");
                await CompanyPPDMEvidenceReader.DefinitionAsync(metadata, type, ["PROJECT_ID", "CONDITION_OBS_NO"], ("PROJECT_TYPE", "R_PROJECT_CONDITION", "PROJECT_TYPE"));
                await CompanyPPDMEvidenceReader.DefinitionAsync(metadata, typeof(R_PROJECT_CONDITION), ["PROJECT_TYPE", "CONDITION_TYPE"], ("PROJECT_TYPE", "R_PROJECT_TYPE", "PROJECT_TYPE"));
                await ReferenceAsync(reader, typeof(R_PROJECT_CONDITION), ["PROJECT_TYPE", "CONDITION_TYPE"], [CompanyPPDMEvidenceReader.Equal("PROJECT_TYPE", parentType), CompanyPPDMEvidenceReader.Equal("CONDITION_TYPE", condition.CONDITION_TYPE)]);
                await RequireProjectNodeAsync(reader, project!, condition.SOURCE, parentType);
            }
            if (row is PROJECT_STATUS status)
            {
                var allowed = parentType == HseReferenceCodes.Hazop.ProjectType
                    ? status.STATUS_TYPE is HseReferenceCodes.Hazop.StudyStatusType or HseReferenceCodes.Hazop.DeviationStatusType
                    : status.STATUS_TYPE == HseReferenceCodes.CorrectiveAction.StatusType;
                if (!allowed || !CompanyPPDMEvidenceReader.Identifier(status.STATUS)) throw RefusalException.Conflict("The HSE project status vocabulary is unavailable.");
                await CompanyPPDMEvidenceReader.DefinitionAsync(metadata, type, ["PROJECT_ID", "STATUS_ID"], ("STATUS_TYPE", "R_PROJECT_STATUS_TYPE", "STATUS_TYPE"), ("STATUS_TYPE", "R_PROJECT_STATUS", "STATUS_TYPE"));
                await CompanyPPDMEvidenceReader.DefinitionAsync(metadata, typeof(R_PROJECT_STATUS_TYPE), ["STATUS_TYPE"], ("PROJECT_TYPE", "R_PROJECT_TYPE", "PROJECT_TYPE"));
                await CompanyPPDMEvidenceReader.DefinitionAsync(metadata, typeof(R_PROJECT_STATUS), ["STATUS_TYPE", "STATUS"], ("STATUS_TYPE", "R_PROJECT_STATUS_TYPE", "STATUS_TYPE"));
                await ReferenceAsync(reader, typeof(R_PROJECT_STATUS_TYPE), ["STATUS_TYPE"], [CompanyPPDMEvidenceReader.Equal("STATUS_TYPE", status.STATUS_TYPE), CompanyPPDMEvidenceReader.Equal("PROJECT_TYPE", parentType)]);
                await ReferenceAsync(reader, typeof(R_PROJECT_STATUS), ["STATUS_TYPE", "STATUS"], [CompanyPPDMEvidenceReader.Equal("STATUS_TYPE", status.STATUS_TYPE), CompanyPPDMEvidenceReader.Equal("STATUS", status.STATUS)]);
                if (status.STATUS_TYPE != HseReferenceCodes.Hazop.StudyStatusType) await RequireProjectNodeAsync(reader, project!, status.STEP_ID, parentType, preparedStep);
                else if (!string.IsNullOrEmpty(status.STEP_ID)) throw RefusalException.Conflict("A study status cannot assert a deviation node association.");
            }
            if (row is PROJECT_STEP_BA assignment)
            {
                if (parentType != HseReferenceCodes.CorrectiveAction.ProjectType || assignment.ROLE != HseReferenceCodes.CorrectiveAction.ResponsibleRole || !CompanyPPDMEvidenceReader.Identifier(assignment.BUSINESS_ASSOCIATE_ID) ||
                    !CompanyPPDMEvidenceReader.Observation(assignment.ROLE_SEQ_NO) || !CompanyPPDMEvidenceReader.Observation(assignment.STEP_BA_OBS_NO)) throw RefusalException.Conflict("The corrective-action assignment has incomplete participant/role identity.");
                await RequireProjectNodeAsync(reader, project!, assignment.STEP_ID, parentType, preparedStep);
                await CompanyPPDMEvidenceReader.DefinitionAsync(metadata, typeof(PROJECT_BA_ROLE), ["PROJECT_ID", "BUSINESS_ASSOCIATE_ID", "ROLE", "ROLE_SEQ_NO"], ("PROJECT_ID", "PROJECT_BA", "PROJECT_ID"), ("ROLE", "R_PROJECT_BA_ROLE", "ROLE"));
                await ReferenceAsync(reader, typeof(R_PROJECT_BA_ROLE), ["ROLE"], [CompanyPPDMEvidenceReader.Equal("ROLE", assignment.ROLE)]);
                var roles = await reader.ReadAsync(typeof(PROJECT_BA_ROLE), [CompanyPPDMEvidenceReader.Equal("PROJECT_ID", project!), CompanyPPDMEvidenceReader.Equal("BUSINESS_ASSOCIATE_ID", assignment.BUSINESS_ASSOCIATE_ID),
                    CompanyPPDMEvidenceReader.Equal("ROLE", assignment.ROLE), CompanyPPDMEvidenceReader.Equal("ROLE_SEQ_NO", assignment.ROLE_SEQ_NO), CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y")]);
                if (roles.Length != 1 || !Current(roles[0])) throw RefusalException.Conflict("The assignment has no exact current project participant role.");
                await CompanyPPDMEvidenceReader.DefinitionAsync(metadata, typeof(PROJECT_BA), ["PROJECT_ID", "BUSINESS_ASSOCIATE_ID"], ("PROJECT_ID", "PROJECT", "PROJECT_ID"), ("BUSINESS_ASSOCIATE_ID", "BUSINESS_ASSOCIATE", "BUSINESS_ASSOCIATE_ID"));
                var participants = await reader.ReadAsync(typeof(PROJECT_BA), [CompanyPPDMEvidenceReader.Equal("PROJECT_ID", project!), CompanyPPDMEvidenceReader.Equal("BUSINESS_ASSOCIATE_ID", assignment.BUSINESS_ASSOCIATE_ID), CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y")]);
                if (participants.Length != 1 || !Current(participants[0])) throw RefusalException.Conflict("The assignment has no unique active project participant.");
                await CompanyPPDMEvidenceReader.DefinitionAsync(metadata, typeof(BUSINESS_ASSOCIATE), ["BUSINESS_ASSOCIATE_ID"]);
                var associates = await reader.ReadAsync(typeof(BUSINESS_ASSOCIATE), [CompanyPPDMEvidenceReader.Equal("BUSINESS_ASSOCIATE_ID", assignment.BUSINESS_ASSOCIATE_ID), CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y")]);
                if (associates.Length != 1 || !Current(associates[0])) throw RefusalException.Conflict("The recorded responsible participant is unavailable or ambiguous.");
            }
            if (!Current(row)) throw RefusalException.Conflict("A returned HSE project row is outside its active evidence window.");
        }
        await reader.RequireCurrentAsync();
    }

    private async Task RequireProjectNodeAsync(CompanyPPDMEvidenceReader reader, string project, string? step, string parentType, PROJECT_STEP? preparedStep = null)
    {
        if (!CompanyPPDMEvidenceReader.Identifier(project) || !CompanyPPDMEvidenceReader.Identifier(step)) throw RefusalException.Conflict("The HSE child has no complete recorded node relationship.");
        await CompanyPPDMEvidenceReader.DefinitionAsync(metadata, typeof(PROJECT_STEP), ["PROJECT_ID", "STEP_ID"], ("PROJECT_ID", "PROJECT", "PROJECT_ID"), ("STEP_TYPE", "R_PROJ_STEP_TYPE", "STEP_TYPE"));
        if (preparedStep is not null && (parentType != HseReferenceCodes.CorrectiveAction.ProjectType || preparedStep.PROJECT_ID != project || preparedStep.STEP_ID != step || preparedStep.ACTIVE_IND != "Y"))
            throw RefusalException.Conflict("The prepared node differs from its corrective-action project.");
        var nodes = preparedStep is null
            ? await reader.ReadAsync(typeof(PROJECT_STEP), [CompanyPPDMEvidenceReader.Equal("PROJECT_ID", project), CompanyPPDMEvidenceReader.Equal("STEP_ID", step!), CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y")])
            : new object[] { preparedStep };
        if (nodes.Length != 1 || !Current(nodes[0])) throw RefusalException.Conflict("The HSE child has no unique active node in this exact project.");
        var node = (PROJECT_STEP)nodes[0];
        if (!CompanyPPDMEvidenceReader.Identifier(node.STEP_TYPE) || parentType == HseReferenceCodes.Hazop.ProjectType && node.STEP_TYPE != HseReferenceCodes.Hazop.NodeType)
            throw RefusalException.Conflict("The recorded HSE project node type is unavailable.");
        await ReferenceAsync(reader, typeof(R_PROJ_STEP_TYPE), ["STEP_TYPE"], [CompanyPPDMEvidenceReader.Equal("STEP_TYPE", node.STEP_TYPE)]);
    }

    private async Task<CompanyPPDMEvidenceReader> EvidenceReaderAsync()
    {
        var database = await company.ResolveAsync();
        var reader = new CompanyPPDMEvidenceReader(company, editor, columns, defaults, metadata, database, company.Company.Scope, company.Company.UserId);
        await reader.OpenAsync();
        return reader;
    }

    internal Task<object> CorrectiveActionRoleReferenceAsync(CompanyPPDMEvidenceReader reader) =>
        ReferenceAsync(reader, typeof(R_PROJECT_BA_ROLE), ["ROLE"], [CompanyPPDMEvidenceReader.Equal("ROLE", HseReferenceCodes.CorrectiveAction.ResponsibleRole)]);

    private async Task<object> ReferenceAsync(CompanyPPDMEvidenceReader reader, Type type, string[] keys, IReadOnlyList<AppFilter> filters)
    {
        await CompanyPPDMEvidenceReader.DefinitionAsync(metadata, type, keys);
        var rows = await reader.ReadAsync(type, [.. filters, CompanyPPDMEvidenceReader.Equal("ACTIVE_IND", "Y")]);
        if (rows.Length != 1 || !Current(rows[0])) throw RefusalException.Conflict("The recorded HSE project reference vocabulary is unavailable or ambiguous.");
        return rows[0];
    }

    internal static bool Current(object row)
    {
        foreach (var column in new[] { "EFFECTIVE_DATE", "EXPIRY_DATE" })
            if (row.GetType().GetProperty(column)!.GetValue(row) is DateTime value && value.Kind != DateTimeKind.Utc)
                throw RefusalException.Conflict("Dated HSE authority evidence requires an explicit UTC boundary.");
        return CompanyPPDMEvidenceReader.Window(row, TimeZoneInfo.Utc, TimeProvider.System, required: false);
    }

    private static bool CanonicalQualifier(string? value) => !string.IsNullOrWhiteSpace(value) && value.Length <= 128 && value == value.Trim() && !value.Any(char.IsControl);
    private static bool Matches(object row, IReadOnlyList<IReadOnlyList<AppFilter>> predicates) =>
        predicates.Any(group => group.Count != 0 && group.All(predicate => CompanyPPDMEvidenceReader.Exact(row.GetType().GetProperty(predicate.FieldName)?.GetValue(row), predicate.FilterValue)));
    private static bool MatchesAll(object row, IReadOnlyList<AppFilter> constraints) =>
        constraints.All(constraint => Value(row, constraint.FieldName) == constraint.FilterValue);
    private static string? Value(object row, string column) => row.GetType().GetProperty(column)?.GetValue(row) as string;
    private sealed record Path(string Column, string Asset, bool Inherited, string? Qualifier = null);
}
