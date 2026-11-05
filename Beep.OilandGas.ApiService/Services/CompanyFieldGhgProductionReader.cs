using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.HSE;
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

/// <summary>Reads recorded production-entity candidates for the selected company and FIELD.</summary>
public sealed class CompanyFieldGhgProductionReader(CompanyModuleConnection company, ICompanyFieldSelectionService selections,
    IAccessControlService access, IDMEEditor editor, ICommonColumnHandler columns, IPPDM39DefaultsRepository defaults, IPPDMMetadataRepository metadata)
{
    public async Task<FieldGhgProductionSources> ReadAsync(string actor)
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
                throw RefusalException.Conflict("The company or FIELD changed while reading production sources.");
            var grant = await access.CheckAssetAccessAsync(actor, review.FieldId!, "FIELD", PermissionConstants.Regulatory.ViewCompliance);
            if (grant is null || !grant.HasAccess) throw RefusalException.Forbidden("The current FIELD compliance permission is required.");
        }
        await Authority(); await reader.OpenAsync();
        await DefinitionAsync(metadata, typeof(PDEN), ["PDEN_SUBTYPE", "PDEN_ID", "SOURCE"], ("FIELD_ID", "FIELD", "FIELD_ID"));
        async Task<PDEN[]> Read()
        {
            await Authority();
            var rows = (await reader.ReadAsync(typeof(PDEN), [Equal("FIELD_ID", review.FieldId!), Equal("ACTIVE_IND", "Y")],
                ["PDEN_SUBTYPE", "PDEN_ID", "SOURCE", "FIELD_ID", "ACTIVE_IND"])).Cast<PDEN>().ToArray();
            await Authority(); return rows;
        }
        var first = await Read(); var revision = Fingerprint(first);
        var result = GhgProductionSourceMapper.Project(review, first, revision);
        var second = await Read();
        if (Fingerprint(second) != revision) throw RefusalException.Conflict("Production entities changed during the read.");
        await Authority(); return result;
    }

    public async Task<FieldGhgDispositions> ReadDispositionsAsync(string actor, GhgDispositionQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        var parents = await ReadAsync(actor);
        if (parents.SourceRevision != query.ExpectedSourceRevision || parents.Entities.Count(x => x.Subtype == query.Subtype && x.ProductionEntityId == query.ProductionEntityId && x.Source == query.Source) != 1)
            throw RefusalException.Conflict("The selected production entity is not in the reviewed FIELD sources.");
        var review = parents.Field;
        var reader = new CompanyPPDMEvidenceReader(company, editor, columns, defaults, metadata, review.Database, review.Company, actor);
        async Task Authority()
        {
            var current = await selections.GetCurrentFieldSelectionAsync();
            FieldSelectionReviewValidator.RequireCurrent(current, review.Company, actor);
            if (!current.SelectionAvailable || current.SelectionContext != review || company.Company.UserId != actor || company.Company.Scope != review.Company || await company.ResolveAsync() != review.Database)
                throw RefusalException.Conflict("The company or FIELD changed while reading dispositions.");
            var grant = await access.CheckAssetAccessAsync(actor, review.FieldId!, "FIELD", PermissionConstants.Regulatory.ViewCompliance);
            if (grant is null || !grant.HasAccess) throw RefusalException.Forbidden("The current FIELD compliance permission is required.");
        }
        await Authority(); await reader.OpenAsync();
        await DefinitionAsync(metadata, typeof(PDEN_VOL_DISPOSITION), ["PDEN_SUBTYPE", "PDEN_ID", "PDEN_SOURCE", "VOLUME_METHOD", "ACTIVITY_TYPE", "PRODUCT_TYPE", "PERIOD_TYPE", "DISPOSITION_OBS_NO", "AMENDMENT_SEQ_NO"],
            ("PDEN_SUBTYPE", "PDEN", "PDEN_SUBTYPE"));
        async Task<PDEN_VOL_DISPOSITION[]> Read()
        {
            await Authority();
            var rows = (await reader.ReadAsync(typeof(PDEN_VOL_DISPOSITION), [Equal("PDEN_SUBTYPE", query.Subtype), Equal("PDEN_ID", query.ProductionEntityId), Equal("PDEN_SOURCE", query.Source), Equal("ACTIVE_IND", "Y")],
                ["PDEN_SUBTYPE", "PDEN_ID", "PDEN_SOURCE", "ACTIVE_IND", "VOLUME_METHOD", "ACTIVITY_TYPE", "PRODUCT_TYPE", "PERIOD_TYPE", "DISPOSITION_OBS_NO", "AMENDMENT_SEQ_NO", "VOLUME", "VOLUME_UOM"])).Cast<PDEN_VOL_DISPOSITION>().ToArray();
            await Authority(); return rows;
        }
        var first = await Read(); var revision = Fingerprint(first);
        var result = GhgProductionSourceMapper.ProjectDispositions(review, query, first, revision);
        if (Fingerprint(await Read()) != revision) throw RefusalException.Conflict("Dispositions changed during the read.");
        var closing = await ReadAsync(actor);
        if (closing.Field != review || closing.SourceRevision != parents.SourceRevision) throw RefusalException.Conflict("Production sources changed during the disposition review.");
        await Authority(); return result;
    }
}
