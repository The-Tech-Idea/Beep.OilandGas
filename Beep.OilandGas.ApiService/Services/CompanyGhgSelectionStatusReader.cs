using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Checks whether the selected catalog and company environment still match a recorded selection.</summary>
public sealed class CompanyGhgSelectionStatusReader(ICompanyGhgSelectionStore selections, CompanyGhgCatalogReader catalogs)
{
    public async Task<CompanyGhgSelectionReadResult> ReadAsync(CompanyScope company, Func<Task> authority)
    {
        var selected = await selections.ReadAsync(company, authority);
        if (selected is null)
        {
            await authority();
            if (await selections.ReadAsync(company, authority) is not null) throw new InvalidOperationException("The company selection changed during status review.");
            await authority(); return new(company, null, "NotSelected");
        }
        CompanyGhgSelectionIdentity.RequireValid(selected, company);
        string status;
        try
        {
            var current = await catalogs.ReviewAsync(company, selected.Jurisdiction, authority);
            status = current.Catalog.Revision == selected.CatalogRevision && current.EffectiveRevision == selected.EnvironmentRevision
                ? "Current" : "ReviewRequired";
        }
        catch (CompanyEnvironmentReviewRequiredException) { status = "ReviewRequired"; }
        catch (Exception error) when (error is ArgumentException or System.Text.Json.JsonException or FormatException or NotSupportedException or InvalidOperationException)
        { status = "Unconfirmed"; }
        await authority();
        if (await selections.ReadAsync(company, authority) != selected) throw new InvalidOperationException("The company selection changed during status review.");
        await authority();
        return new(company, selected, status);
    }
}
