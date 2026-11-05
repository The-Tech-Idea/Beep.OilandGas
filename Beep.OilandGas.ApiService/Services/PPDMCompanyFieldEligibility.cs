using System.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.Extensions.Options;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Explicit company FIELD data entitlement over existing PPDM owners; does not adjudicate legal title or JV interests.</summary>
public sealed class PPDMCompanyFieldEligibility(CompanyModuleConnection company, IDMEEditor editor,
    ICommonColumnHandler columns, IPPDM39DefaultsRepository defaults, IPPDMMetadataRepository metadata,
    IOptions<PPDMFieldEligibilityConfiguration> configuration, TimeProvider? timeProvider = null) : ICompanyFieldEligibility
{
    private readonly TimeProvider time = timeProvider ?? TimeProvider.System;
    public async Task<string?> GetMaximumLevelAsync(string fieldId)
    {
        if (!Identifier(fieldId)) throw RefusalException.Invalid("An exact field identifier is required for company eligibility.");
        var database = await company.ResolveAsync(); var captured = company.Company; var scope = captured.Scope;
        var rules = configuration.Value.Companies?.Where(rule => rule is not null && rule.TenantId == scope.TenantId && rule.BusinessAssociateId == scope.BusinessAssociateId).ToArray();
        if (rules is null || rules.Length != 1) throw RefusalException.Conflict("Configure exactly one reviewed FIELD data-entitlement vocabulary and date zone for the selected company.");
        var rule = rules[0];
        if (!Identifier(rule.EntitlementType) || !Identifier(rule.ComponentType) || !Identifier(rule.ComponentAccessConditionCode) || !Identifier(rule.TimeZoneId) ||
            rule.AccessTypes is null || rule.AccessTypes.Count != 3 || !rule.AccessTypes.Keys.Order(StringComparer.Ordinal).SequenceEqual(new[] { "DELETE", "READ", "WRITE" }) ||
            rule.AccessTypes.Values.Any(code => !Identifier(code)) || rule.AccessTypes.Values.Distinct(StringComparer.Ordinal).Count() != 3)
            throw RefusalException.Conflict("The company FIELD data-entitlement vocabulary is incomplete or ambiguous.");
        TimeZoneInfo zone;
        try { zone = TimeZoneInfo.FindSystemTimeZoneById(rule.TimeZoneId); }
        catch (TimeZoneNotFoundException) { throw RefusalException.Conflict("The reviewed company entitlement date zone is unavailable."); }
        catch (InvalidTimeZoneException) { throw RefusalException.Conflict("The reviewed company entitlement date zone is invalid."); }
        // Capture the reviewed configuration before any provider await; no request can silently adopt mutated codes.
        var entitlementType = rule.EntitlementType; var componentType = rule.ComponentType; var condition = rule.ComponentAccessConditionCode;
        var accessTypes = rule.AccessTypes.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        await DefinitionAsync(typeof(ENT_COMPONENT), ["ENTITLEMENT_ID", "COMPONENT_ID", "COMPONENT_OBS_NO"],
            ("FIELD_ID", "FIELD", "FIELD_ID"), ("BUSINESS_ASSOCIATE_ID", "BUSINESS_ASSOCIATE", "BUSINESS_ASSOCIATE_ID"),
            ("ENTITLEMENT_ID", "ENTITLEMENT", "ENTITLEMENT_ID"), ("ENT_COMP_TYPE", "R_ENT_COMPONENT_TYPE", "ENT_COMP_TYPE"),
            ("ACCESS_COND_CODE", "R_ACCESS_CONDITION", "ACCESS_CONDITION_CODE"));
        await DefinitionAsync(typeof(ENTITLEMENT), ["ENTITLEMENT_ID"], ("ENTITLEMENT_TYPE", "R_ENT_TYPE", "ENTITLEMENT_TYPE"), ("ACCESS_COND_CODE", "R_ENT_ACCESS_TYPE", "ACCESS_TYPE"));
        await DefinitionAsync(typeof(R_ENT_TYPE), ["ENTITLEMENT_TYPE"]);
        await DefinitionAsync(typeof(R_ENT_COMPONENT_TYPE), ["ENT_COMP_TYPE"]);
        await DefinitionAsync(typeof(R_ACCESS_CONDITION), ["ACCESS_CONDITION_CODE"]);
        await DefinitionAsync(typeof(R_ENT_ACCESS_TYPE), ["ACCESS_TYPE"]);
        var evidence = new CompanyPPDMEvidenceReader(company, editor, columns, defaults, metadata, database, scope, captured.UserId);
        await evidence.OpenAsync();
        Task<object[]> Read(Type type, params AppFilter[] filters) => evidence.ReadAsync(type, filters,
            type == typeof(ENT_COMPONENT) ? ["ENTITLEMENT_ID", "COMPONENT_ID", "COMPONENT_OBS_NO"] : null);
        var referenceEvidence = new List<object>();
        async Task<bool> Reference(Type type, string key, string code)
        {
            var rows = await Read(type, Equal(key, code), Equal("ACTIVE_IND", "Y"));
            if (rows.Length > 1) throw RefusalException.Conflict("The provider returned ambiguous FIELD entitlement reference evidence.");
            if (rows.Length != 1 || !Window(rows[0], zone, required: false)) return false;
            referenceEvidence.Add(rows[0]); return true;
        }
        if (!await Reference(typeof(R_ENT_TYPE), "ENTITLEMENT_TYPE", entitlementType) ||
            !await Reference(typeof(R_ENT_COMPONENT_TYPE), "ENT_COMP_TYPE", componentType) ||
            !await Reference(typeof(R_ACCESS_CONDITION), "ACCESS_CONDITION_CODE", condition)) return null;
        var vocabularyEvidence = referenceEvidence.ToArray();
        var components = (await Read(typeof(ENT_COMPONENT), Equal("FIELD_ID", fieldId), Equal("BUSINESS_ASSOCIATE_ID", scope.BusinessAssociateId),
            Equal("ENT_COMP_TYPE", componentType), Equal("ACCESS_COND_CODE", condition), Equal("ACTIVE_IND", "Y"))).Cast<ENT_COMPONENT>().ToArray();
        if (components.Any(row => !Identifier(row.ENTITLEMENT_ID) || !Identifier(row.COMPONENT_ID) || row.COMPONENT_OBS_NO < 0 || decimal.Truncate(row.COMPONENT_OBS_NO) != row.COMPONENT_OBS_NO) ||
            components.Select(row => (row.ENTITLEMENT_ID, row.COMPONENT_ID, row.COMPONENT_OBS_NO)).Distinct().Count() != components.Length)
            throw RefusalException.Conflict("FIELD entitlement components have incomplete or ambiguous canonical keys.");
        var eligible = new List<(ENT_COMPONENT Component, ENTITLEMENT Header, object AccessType, string Level)>();
        foreach (var component in components.Where(row => Window(row, zone, required: true)))
        {
            // Text rules are deliberately not executed or guessed. These reviewed codes describe unconditional application data access.
            if (!string.IsNullOrWhiteSpace(component.ENT_RULE) || !string.IsNullOrWhiteSpace(component.EXPIRY_ACTION))
                throw RefusalException.Conflict("Conditional FIELD entitlement rules require a reviewed interpreter before application access.");
            var headers = (await Read(typeof(ENTITLEMENT), Equal("ENTITLEMENT_ID", component.ENTITLEMENT_ID), Equal("ENTITLEMENT_TYPE", entitlementType), Equal("ACTIVE_IND", "Y"))).Cast<ENTITLEMENT>().ToArray();
            if (headers.Length > 1) throw RefusalException.Conflict("The provider returned an ambiguous FIELD entitlement header.");
            if (headers.Length == 0 || !Window(headers[0], zone, required: true)) continue;
            var header = headers[0];
            if (!string.IsNullOrWhiteSpace(header.ACCESS_CONDITION) || !string.IsNullOrWhiteSpace(header.USE_CONDITION) || !string.IsNullOrWhiteSpace(header.EXPIRY_ACTION))
                throw RefusalException.Conflict("Conditional FIELD entitlement terms require review before application access.");
            var level = accessTypes.SingleOrDefault(pair => pair.Value == header.ACCESS_COND_CODE).Key;
            if (level is null) continue;
            if (await Reference(typeof(R_ENT_ACCESS_TYPE), "ACCESS_TYPE", header.ACCESS_COND_CODE)) eligible.Add((component, header, referenceEvidence[^1], level));
        }
        // Recheck time-bound evidence at return, not just when each lookup began.
        await evidence.RequireCurrentAsync();
        if (vocabularyEvidence.Any(row => !Window(row, zone, required: false))) return null;
        return eligible.Where(path => Window(path.Component, zone, required: true) && Window(path.Header, zone, required: true) && Window(path.AccessType, zone, required: false))
            .OrderByDescending(path => Rank(path.Level)).Select(path => path.Level).FirstOrDefault();
    }

    private Task DefinitionAsync(Type type, string[] keys, params (string Column, string Table, string Key)[] relationships) =>
        CompanyPPDMEvidenceReader.DefinitionAsync(metadata, type, keys, relationships);
    private bool Window(object row, TimeZoneInfo zone, bool required) => CompanyPPDMEvidenceReader.Window(row, zone, time, required);
    private static AppFilter Equal(string column, string value) => new() { FieldName = column, Operator = "=", FilterValue = value };
    private static bool Identifier(string? value) => !string.IsNullOrWhiteSpace(value) && value.Length <= 128 && value == value.Trim() && !value.Any(char.IsControl);
    internal static int Rank(string? level) => level switch { "READ" => 1, "WRITE" => 2, "DELETE" => 3, _ => 0 };
}
