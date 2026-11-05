using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.Security;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Captures app-owned authority for a command on the exact reviewed company FIELD.</summary>
public sealed class CompanyFieldObligationAuthority(RepositoryRequestAccount account, CompanyModuleConnection company,
    ICompanyFieldSelectionService fields, ICompanyFieldSelectionAuthority preferences,
    IAccessControlService access, IUserAssetAccessStore grants)
{
    public async Task<CompanyFieldObligationCommandScope> CaptureAsync(ClaimsPrincipal user, FieldSelectionReview review)
    {
        ArgumentNullException.ThrowIfNull(user); ArgumentNullException.ThrowIfNull(review);
        var actor = await account.RequireAsync(); company.RequireActor(actor);
        var database = await company.ResolveAsync(); var companyScope = company.Company.Scope;
        FieldSelectionReviewValidator.RequireReview(review, companyScope, actor);
        if (review.Database != database || string.IsNullOrWhiteSpace(review.FieldId))
            throw RefusalException.Forbidden("Return to the original company and FIELD before creating an obligation.");

        async Task<string> AppSnapshot()
        {
            if (user.Identity?.IsAuthenticated != true || await account.RequireAsync() != actor)
                throw RefusalException.Forbidden("The obligation account changed.");
            var current = await company.RefreshCompanyAsync(); company.RequireActor(actor);
            if (current.Scope != companyScope || current.Roles.Count == 0 || !current.Roles.Any(user.IsInRole) ||
                !current.Permissions.Contains(PermissionConstants.Regulatory.ManageFilings, StringComparer.Ordinal) ||
                await company.ResolveAsync() != database)
                throw RefusalException.Forbidden("Current company roles, filing permission or database binding changed.");
            await preferences.RequireStoredSelectionAsync(review);
            var rows = await grants.ReadAsync(actor, database.Fingerprint, companyScope.BusinessAssociateId);
            var eligible = rows.Where(row => row.IsActive && row.Inherit && row.AssetType == "FIELD" && row.AssetId == review.FieldId &&
                row.AccessLevel is "WRITE" or "DELETE").ToArray();
            if (eligible.Length == 0) throw RefusalException.Forbidden("An inherited FIELD write grant is required to create obligations.");
            return JsonSerializer.Serialize(new { Roles = current.Roles.Order(StringComparer.Ordinal),
                Permissions = current.Permissions.Order(StringComparer.Ordinal), Grants = rows.OrderBy(row => row.Id, StringComparer.Ordinal) });
        }

        var captured = await AppSnapshot();
        async Task Check()
        {
            if (await AppSnapshot() != captured)
                throw RefusalException.Forbidden("Company filing authority or FIELD grants changed. Review again.");
        }
        async Task Confirm()
        {
            await Check();
            var selected = await fields.GetCurrentFieldSelectionAsync();
            FieldSelectionReviewValidator.RequireCurrent(selected, companyScope, actor);
            if (!selected.SelectionAvailable || selected.SelectionContext != review ||
                (await access.CheckAssetAccessAsync(actor, review.FieldId, "FIELD", "WRITE"))?.HasAccess != true ||
                (await access.CheckAssetAccessAsync(actor, review.FieldId, "FIELD", PermissionConstants.Regulatory.ManageFilings))?.HasAccess != true)
                throw RefusalException.Forbidden("Current FIELD write and filing permission could not be confirmed.");
            await Check();
        }
        await Confirm();
        return new(review, Check, Confirm);
    }
}

public sealed record CompanyFieldObligationCommandScope(FieldSelectionReview Context, Func<Task> CheckCurrentAsync, Func<Task> ConfirmAsync);
