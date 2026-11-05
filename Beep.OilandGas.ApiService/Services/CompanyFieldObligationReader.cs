using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.Compliance;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Services.Compliance;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Editor;
using static Beep.OilandGas.ApiService.Services.CompanyPPDMEvidenceReader;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Reads recorded obligation facts through the selected company's FIELD and PPDM binding.</summary>
public sealed class CompanyFieldObligationReader(CompanyModuleConnection company, ICompanyFieldSelectionService selections,
    IAccessControlService access, IDMEEditor editor, ICommonColumnHandler columns, IPPDM39DefaultsRepository defaults, IPPDMMetadataRepository metadata)
{
    public async Task<FieldObligationSourceReview> ReadAsync(string actor)
    {
        company.RequireActor(actor); var selected = await selections.GetCurrentFieldSelectionAsync();
        var review = selected.SelectionContext ?? throw RefusalException.Conflict("The current FIELD review is unavailable.");
        FieldSelectionReviewValidator.RequireCurrent(selected, company.Company.Scope, actor);
        if (!selected.SelectionAvailable || review.Database != await company.ResolveAsync()) throw RefusalException.Conflict("The current FIELD or database is unavailable.");
        var reader = new CompanyPPDMEvidenceReader(company, editor, columns, defaults, metadata, review.Database, review.Company, actor);
        async Task Authority()
        {
            var current = await selections.GetCurrentFieldSelectionAsync();
            FieldSelectionReviewValidator.RequireCurrent(current, review.Company, actor);
            if (!current.SelectionAvailable || current.SelectionContext != review || company.Company.UserId != actor || company.Company.Scope != review.Company || await company.ResolveAsync() != review.Database)
                throw RefusalException.Conflict("The company or FIELD changed while reading obligation facts.");
            var grant = await access.CheckAssetAccessAsync(actor, review.FieldId!, "FIELD", PermissionConstants.Regulatory.ViewCompliance);
            if (grant is null || !grant.HasAccess) throw RefusalException.Forbidden("The current FIELD compliance permission is required.");
        }
        await Authority(); await reader.OpenAsync();
        await DefinitionAsync(metadata, typeof(OBLIGATION), ["OBLIGATION_ID", "OBLIGATION_SEQ_NO"]);
        await DefinitionAsync(metadata, typeof(OBLIGATION_COMPONENT), ["OBLIGATION_ID", "OBLIGATION_SEQ_NO", "COMPONENT_OBS_NO"]);
        async Task<(OBLIGATION_COMPONENT[] Links, OBLIGATION[] Parents)> Read()
        {
            await Authority();
            var links = (await reader.ReadAsync(typeof(OBLIGATION_COMPONENT), [Equal("FIELD_ID", review.FieldId!), Equal("ACTIVE_IND", "Y")],
                ["OBLIGATION_ID", "OBLIGATION_SEQ_NO", "COMPONENT_OBS_NO", "FIELD_ID", "ACTIVE_IND"],
                ["OBLIGATION_COMPONENT_TYPE", "CRITICAL_DATE", "FULFILLED_IND", "FULFILLED_DATE", "DESCRIPTION"])).Cast<OBLIGATION_COMPONENT>().ToArray();
            var parents = new List<OBLIGATION>();
            foreach (var key in links.Select(row => (row.OBLIGATION_ID, row.OBLIGATION_SEQ_NO)).Distinct())
            {
                if (!Identifier(key.OBLIGATION_ID) || !Observation(key.OBLIGATION_SEQ_NO)) throw RefusalException.Conflict("An obligation component has an invalid parent key.");
                parents.AddRange((await reader.ReadAsync(typeof(OBLIGATION), [Equal("OBLIGATION_ID", key.OBLIGATION_ID), Equal("OBLIGATION_SEQ_NO", key.OBLIGATION_SEQ_NO), Equal("ACTIVE_IND", "Y")],
                    ["OBLIGATION_ID", "OBLIGATION_SEQ_NO", "ACTIVE_IND"],
                    ["OBLIGATION_TYPE", "CRITICAL_DATE", "FULFILLED_IND", "FULFILLED_DATE", "DESCRIPTION"])).Cast<OBLIGATION>());
            }
            await Authority(); return (links, parents.ToArray());
        }
        var first = await Read(); var firstLinks = Fingerprint(first.Links); var firstParents = Fingerprint(first.Parents);
        var projection = ComplianceFieldObligationMapper.Project(review, first.Links, first.Parents);
        var second = await Read();
        if (Fingerprint(second.Links) != firstLinks || Fingerprint(second.Parents) != firstParents) throw RefusalException.Conflict("Obligation facts changed during the read.");
        await Authority(); return projection;
    }
}
