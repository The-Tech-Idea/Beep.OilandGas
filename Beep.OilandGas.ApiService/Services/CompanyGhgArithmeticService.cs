using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.HSE.Models;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.Security;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Performs a bounded, non-reporting arithmetic check on explicitly reviewed company evidence.</summary>
public sealed class CompanyGhgArithmeticService(CompanyModuleConnection company, ICompanyFieldSelectionService fields,
    IAccessControlService access, CompanyGhgSelectionStatusReader status, CompanyGhgCatalogReader catalogs, CompanyFieldGhgProductionReader production)
{
    private async Task<(FieldSelectionReview Field, Func<Task> Guard)> AuthorityAsync(string actor)
    {
        company.RequireActor(actor); var selected = await fields.GetCurrentFieldSelectionAsync();
        var field = selected.SelectionContext ?? throw RefusalException.Conflict("The current FIELD review is unavailable.");
        FieldSelectionReviewValidator.RequireCurrent(selected, company.Company.Scope, actor);
        async Task Guard()
        {
            var current = await fields.GetCurrentFieldSelectionAsync();
            FieldSelectionReviewValidator.RequireCurrent(current, field.Company, actor);
            if (!current.SelectionAvailable || current.SelectionContext != field || company.Company.Scope != field.Company || company.Company.UserId != actor || await company.ResolveAsync() != field.Database)
                throw RefusalException.Conflict("Company or FIELD access changed.");
            var grant = await access.CheckAssetAccessAsync(actor, field.FieldId!, "FIELD", PermissionConstants.Regulatory.ViewCompliance);
            if (grant is null || !grant.HasAccess) throw RefusalException.Forbidden("FIELD compliance permission is required.");
        }
        await Guard(); return (field, Guard);
    }

    public async Task<GhgCalculationBasis> BasisAsync(string actor)
    {
        var (field, guard) = await AuthorityAsync(actor);
        var selected = await status.ReadAsync(field.Company, guard);
        if (selected.Status != "Current" || selected.Selection is null) throw RefusalException.Conflict("A current company catalog selection is required.");
        var catalog = await catalogs.ReviewAsync(field.Company, selected.Selection.Jurisdiction, guard);
        if (catalog.Catalog.Revision != selected.Selection.CatalogRevision || catalog.EffectiveRevision != selected.Selection.EnvironmentRevision)
            throw RefusalException.Conflict("The company catalog selection changed.");
        var parsed = GhgFactorCatalogValidation.Parse(catalog.Catalog.Key, catalog.Catalog.SettingsJson);
        var closing = await status.ReadAsync(field.Company, guard);
        if (closing.Status != "Current" || closing.Selection != selected.Selection) throw RefusalException.Conflict("The company selection changed during review.");
        await guard(); return new(field, selected.Selection, parsed);
    }

    public async Task<FieldGhgArithmeticCheck> CheckAsync(string actor, GhgArithmeticRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var basis = await BasisAsync(actor);
        if (basis.Selection.Revision != request.ExpectedSelectionRevision || !CompanyEnvironmentIdentity.IsRevision(request.ExpectedDispositionRevision))
            throw RefusalException.Conflict("Review the current selection and recorded dispositions first.");
        var source = await production.ReadDispositionsAsync(actor, request.Query);
        if (source.Field != basis.Field || source.SourceRevision != request.ExpectedDispositionRevision) throw RefusalException.Conflict("The disposition evidence changed.");
        var matches = source.Dispositions.Where(x => x.VolumeMethod == request.VolumeMethod && x.ActivityType == request.ActivityType && x.ProductType == request.ProductType &&
            x.PeriodType == request.PeriodType && x.Observation == request.Observation && x.Amendment == request.Amendment).ToArray();
        var factors = basis.Catalog.Factors.Where(x => x.Id == request.FactorId).ToArray();
        if (matches.Length != 1 || factors.Length != 1) throw RefusalException.Conflict("The selected disposition or factor is unavailable.");
        var check = GhgArithmetic.Check(matches[0], factors[0]);
        var closing = await BasisAsync(actor);
        var closingSource = await production.ReadDispositionsAsync(actor, request.Query);
        var final = await BasisAsync(actor);
        if (closing.Field != basis.Field || closing.Selection != basis.Selection || closingSource.Field != basis.Field || closingSource.SourceRevision != source.SourceRevision || final.Field != basis.Field || final.Selection != basis.Selection)
            throw RefusalException.Conflict("The calculation evidence changed during review.");
        return new(basis.Field, request, basis.Selection.CatalogRevision, check);
    }
}
