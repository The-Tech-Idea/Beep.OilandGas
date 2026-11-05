using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.Extensions.Options;
using TheTechIdea.Beep.Editor;
using static Beep.OilandGas.ApiService.Services.CompanyPPDMEvidenceReader;

namespace Beep.OilandGas.ApiService.Services;

public sealed class PPDMCompanyFieldInterestReview(CompanyModuleConnection company, IDMEEditor editor,
    ICommonColumnHandler columns, IPPDM39DefaultsRepository defaults, IPPDMMetadataRepository metadata,
    IPPDMRowAccessPolicy policy, IOptions<PPDMFieldEligibilityConfiguration> configuration, TimeProvider? clock = null) : ICompanyFieldInterestReview
{
    private readonly TimeProvider time = clock ?? TimeProvider.System;
    public async Task<FieldInterestReview> ReadAsync(string fieldId)
    {
        if (!Identifier(fieldId)) throw RefusalException.Invalid("A canonical FIELD identifier is required.");
        var database = await company.ResolveAsync(); var captured = company.Company;
        if (!captured.Roles.Contains("Administrator", StringComparer.Ordinal)) throw RefusalException.Forbidden("A current company Administrator is required to review FIELD interests.");
        var rules = configuration.Value.Companies.Where(rule => rule.TenantId == captured.Scope.TenantId && rule.BusinessAssociateId == captured.Scope.BusinessAssociateId).ToArray();
        if (rules.Length != 1 || !Identifier(rules[0].InterestReviewComponentType) || !Identifier(rules[0].InterestReviewSetType) || !Identifier(rules[0].TimeZoneId))
            throw RefusalException.Conflict("Configure the selected company's reviewed interest component/type vocabulary and date zone.");
        var componentType = rules[0].InterestReviewComponentType; var setType = rules[0].InterestReviewSetType;
        TimeZoneInfo zone;
        try { zone = TimeZoneInfo.FindSystemTimeZoneById(rules[0].TimeZoneId); }
        catch (TimeZoneNotFoundException) { throw RefusalException.Conflict("The reviewed company interest date zone is unavailable."); }
        catch (InvalidTimeZoneException) { throw RefusalException.Conflict("The reviewed company interest date zone is invalid."); }
        await DefinitionAsync(metadata, typeof(INT_SET_COMPONENT), ["INTEREST_SET_ID", "INTEREST_SET_SEQ_NO", "COMPONENT_OBS_NO"],
            ("FIELD_ID", "FIELD", "FIELD_ID"), ("INTEREST_SET_ID", "INTEREST_SET", "INTEREST_SET_ID"), ("COMPONENT_TYPE", "R_INT_SET_COMPONENT", "COMPONENT_TYPE"));
        await DefinitionAsync(metadata, typeof(INTEREST_SET), ["INTEREST_SET_ID", "INTEREST_SET_SEQ_NO"], ("INTEREST_SET_TYPE", "R_INT_SET_TYPE", "INTEREST_SET_TYPE"));
        await DefinitionAsync(metadata, typeof(INT_SET_PARTNER), ["INTEREST_SET_ID", "INTEREST_SET_SEQ_NO", "PARTNER_BA_ID", "PARTNER_OBS_NO"],
            ("PARTNER_BA_ID", "BUSINESS_ASSOCIATE", "BUSINESS_ASSOCIATE_ID"), ("INTEREST_SET_ID", "INTEREST_SET", "INTEREST_SET_ID"), ("INTEREST_SET_ROLE", "R_INT_SET_ROLE", "INTEREST_SET_ROLE"));
        await DefinitionAsync(metadata, typeof(R_INT_SET_COMPONENT), ["COMPONENT_TYPE"]);
        await DefinitionAsync(metadata, typeof(R_INT_SET_TYPE), ["INTEREST_SET_TYPE"]);
        await DefinitionAsync(metadata, typeof(R_INT_SET_ROLE), ["INTEREST_SET_ROLE"]);
        // Check the actor's real FIELD grant/entitlement before any interest evidence query.
        if ((await policy.GetReadConjunctionsAsync(typeof(FIELD), "FIELD")).Count == 0)
            throw RefusalException.Forbidden("The administrator needs current FIELD data access to review interest records.");
        var evidence = new CompanyPPDMEvidenceReader(company, editor, columns, defaults, metadata, database, captured.Scope, captured.UserId);
        await evidence.OpenAsync();
        var fieldRepository = new PPDMGenericRepository(editor, columns, defaults, metadata, typeof(FIELD), database.ConnectionName, "FIELD", rowAccess: policy);
        var fields = (await fieldRepository.GetAsync([Equal("FIELD_ID", fieldId), Equal("ACTIVE_IND", "Y")])).ToArray();
        if (fields.Length != 1) throw RefusalException.Forbidden("No unique active FIELD is visible under the administrator's current access.");
        var referenceEvidence = new List<object>();
        async Task Reference(Type type, string key, string code)
        {
            var rows = await evidence.ReadAsync(type, [Equal(key, code), Equal("ACTIVE_IND", "Y")], [key]);
            if (rows.Length != 1 || !Window(rows[0], zone, time, required: false)) throw RefusalException.Conflict("A reviewed interest reference is missing, ambiguous or noncurrent.");
            referenceEvidence.Add(rows[0]);
        }
        await Reference(typeof(R_INT_SET_COMPONENT), "COMPONENT_TYPE", componentType);
        await Reference(typeof(R_INT_SET_TYPE), "INTEREST_SET_TYPE", setType);
        var components = (await evidence.ReadAsync(typeof(INT_SET_COMPONENT), [Equal("FIELD_ID", fieldId), Equal("COMPONENT_TYPE", componentType), Equal("ACTIVE_IND", "Y")],
            ["INTEREST_SET_ID", "INTEREST_SET_SEQ_NO", "COMPONENT_OBS_NO"])).Cast<INT_SET_COMPONENT>().ToArray();
        if (components.Any(row => !Identifier(row.INTEREST_SET_ID) || !Observation(row.INTEREST_SET_SEQ_NO) || !Observation(row.COMPONENT_OBS_NO)) ||
            components.Select(row => (row.INTEREST_SET_ID, row.INTEREST_SET_SEQ_NO, row.COMPONENT_OBS_NO)).Distinct().Count() != components.Length)
            throw RefusalException.Conflict("Interest component keys are incomplete or ambiguous.");
        var output = new List<FieldInterestSetReview>();
        foreach (var group in components.Where(row => Window(row, zone, time, required: true)).GroupBy(row => (row.INTEREST_SET_ID, row.INTEREST_SET_SEQ_NO)))
        {
            var setRows = (await evidence.ReadAsync(typeof(INTEREST_SET), [Equal("INTEREST_SET_ID", group.Key.INTEREST_SET_ID), Equal("INTEREST_SET_SEQ_NO", group.Key.INTEREST_SET_SEQ_NO),
                Equal("INTEREST_SET_TYPE", setType), Equal("ACTIVE_IND", "Y")], ["INTEREST_SET_ID", "INTEREST_SET_SEQ_NO"])).Cast<INTEREST_SET>().ToArray();
            if (setRows.Length > 1) throw RefusalException.Conflict("The provider returned an ambiguous interest set.");
            if (setRows.Length == 0 || !Window(setRows[0], zone, time, required: true)) continue;
            var partnerRows = (await evidence.ReadAsync(typeof(INT_SET_PARTNER), [Equal("INTEREST_SET_ID", group.Key.INTEREST_SET_ID), Equal("INTEREST_SET_SEQ_NO", group.Key.INTEREST_SET_SEQ_NO),
                Equal("PARTNER_BA_ID", captured.Scope.BusinessAssociateId), Equal("ACTIVE_IND", "Y")],
                ["INTEREST_SET_ID", "INTEREST_SET_SEQ_NO", "PARTNER_BA_ID", "PARTNER_OBS_NO", "GROSS_PERCENT_INTEREST", "NET_PERCENT_INTEREST"])).Cast<INT_SET_PARTNER>().ToArray();
            if (partnerRows.Any(row => !Observation(row.PARTNER_OBS_NO) || !Identifier(row.INTEREST_SET_ROLE)) ||
                partnerRows.Select(row => (row.INTEREST_SET_ID, row.INTEREST_SET_SEQ_NO, row.PARTNER_BA_ID, row.PARTNER_OBS_NO)).Distinct().Count() != partnerRows.Length)
                throw RefusalException.Conflict("Company partner keys or recorded roles are incomplete or ambiguous.");
            var currentPartners = partnerRows.Where(row => Window(row, zone, time, required: true) &&
                Range(null, row.INACTIVE_DATE, zone, time, required: false)).ToList();
            foreach (var role in currentPartners.Select(row => row.INTEREST_SET_ROLE).Distinct(StringComparer.Ordinal)) await Reference(typeof(R_INT_SET_ROLE), "INTEREST_SET_ROLE", role);
            if (currentPartners.Count != 0) output.Add(new(setRows[0], group.ToList(), currentPartners));
            if (output.Sum(set => 1 + set.Components.Count + set.CompanyPartners.Count) > 256)
                throw RefusalException.Conflict("The FIELD interest review exceeds the supported evidence response limit.");
        }
        await evidence.RequireCurrentAsync(); await policy.ValidateRowsAsync(typeof(FIELD), "FIELD", fields);
        if (referenceEvidence.Any(row => row.GetType().GetProperty("ACTIVE_IND")!.GetValue(row) as string != "Y" || !Window(row, zone, time, required: false)) ||
            output.Any(set => set.InterestSet.ACTIVE_IND != "Y" || !Window(set.InterestSet, zone, time, required: true) ||
            set.Components.Any(row => row.ACTIVE_IND != "Y" || !Window(row, zone, time, required: true)) || set.CompanyPartners.Any(row => row.ACTIVE_IND != "Y" || !Window(row, zone, time, required: true) ||
                !Range(null, row.INACTIVE_DATE, zone, time, required: false))))
            throw RefusalException.Conflict("Interest evidence changed its effective status while being reviewed.");
        return new(captured.Scope, database.Fingerprint, fieldId, time.GetUtcNow().UtcDateTime, zone.Id, output);
    }
}
