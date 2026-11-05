using System.Security.Cryptography;
using System.Text.Json;

namespace TheTechIdea.Data.OilGas;

/// <summary>Stable storage identity and change revision for company-specific environment records.</summary>
public static class CompanyEnvironmentIdentity
{
    public const int MaxOverridesLength = 65536;

    public static string ForCompany(CompanyScope company)
    {
        RequireCompany(company);
        return Hash(new { company.TenantId, company.BusinessAssociateId });
    }

    public static string ForChange(CompanyScope company, CompanyEnvironmentSaveRequest request, string toolsVersion)
    {
        RequireCompany(company);
        ArgumentNullException.ThrowIfNull(request);
        if (request.CountryCode is null || request.CountryCode.Length != 2 ||
            request.CountryCode.Any(c => c is < 'A' or > 'Z') || request.OverridesJson is null ||
            request.OverridesJson.Length > MaxOverridesLength || string.IsNullOrWhiteSpace(request.OverridesJson) ||
            request.ExpectedRevision is null || request.ExpectedRevision.Length > 64 ||
            !Guid.TryParseExact(request.IntentId, "N", out var intent) || intent == Guid.Empty || intent.ToString("N") != request.IntentId ||
            string.IsNullOrWhiteSpace(toolsVersion) || toolsVersion.Length > 128)
            throw new ArgumentException("The environment change is incomplete or invalid.");
        if (request.ExpectedRevision.Length != 0 && !IsRevision(request.ExpectedRevision))
            throw new ArgumentException("The expected environment revision is invalid.");
        return Hash(new { Company = company, request.CountryCode, request.OverridesJson, request.ExpectedRevision, request.IntentId, ToolsVersion = toolsVersion });
    }

    public static bool IsRevision(string? value) => value is { Length: 64 } && value.All(c => c is >= '0' and <= '9' or >= 'A' and <= 'F');

    /// <summary>Computes an idempotent, revision-checked change. Invoke within the owned serializable storage transaction.</summary>
    public static CompanyEnvironmentConfiguration Prepare(CompanyScope company, CompanyEnvironmentSaveRequest request,
        string toolsVersion, CompanyEnvironmentConfiguration? current, string reviewedSystemRevision, string? reviewedCountryRevision)
    {
        var reviewedDraftRevision = CompanyEnvironmentDraftIdentity.ForReview(company, new(request.CountryCode, request.OverridesJson), reviewedSystemRevision, reviewedCountryRevision, toolsVersion);
        var revision = ReviewedChange(company, request, toolsVersion, reviewedDraftRevision);
        if (current is not null)
        {
            RequireValid(current, company);
            if (current.IntentId == request.IntentId)
            {
                if (current.Revision != revision) throw new InvalidOperationException("The environment intent was already used for another change.");
                return current;
            }
        }
        if ((current?.Revision ?? string.Empty) != request.ExpectedRevision)
            throw new InvalidOperationException("The company environment changed. Reload it before saving.");
        return new(company, request.CountryCode, request.OverridesJson, toolsVersion, revision, request.ExpectedRevision, request.IntentId, reviewedDraftRevision, reviewedSystemRevision, reviewedCountryRevision);
    }

    public static void RequireValid(CompanyEnvironmentConfiguration value, CompanyScope company)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.Company != company) throw new InvalidOperationException("The environment belongs to another company.");
        var request = new CompanyEnvironmentSaveRequest(value.CountryCode, value.OverridesJson, value.PreviousRevision, value.IntentId);
        var draft = CompanyEnvironmentDraftIdentity.ForReview(company, new(value.CountryCode, value.OverridesJson), value.ReviewedSystemRevision, value.ReviewedCountryRevision, value.ToolsVersion);
        if (draft != value.ReviewedDraftRevision) throw new InvalidOperationException("The reviewed shared setup does not match the saved draft.");
        if (value.Revision != ReviewedChange(company, request, value.ToolsVersion, value.ReviewedDraftRevision))
            throw new InvalidOperationException("The environment revision does not match its stored configuration.");
    }

    private static string ReviewedChange(CompanyScope company, CompanyEnvironmentSaveRequest request, string toolsVersion, string reviewedDraftRevision)
    {
        if (!IsRevision(reviewedDraftRevision)) throw new ArgumentException("The reviewed draft revision is invalid.");
        return Hash(new { Change = ForChange(company, request, toolsVersion), ReviewedDraftRevision = reviewedDraftRevision });
    }

    private static void RequireCompany(CompanyScope company)
    {
        ArgumentNullException.ThrowIfNull(company);
        if (string.IsNullOrWhiteSpace(company.TenantId) || company.TenantId.Length > 64 ||
            string.IsNullOrWhiteSpace(company.BusinessAssociateId) || company.BusinessAssociateId.Length > 128 ||
            company.TenantId.Any(char.IsControl) || company.BusinessAssociateId.Any(char.IsControl))
            throw new ArgumentException("A valid tenant and company are required.");
    }

    private static string Hash<T>(T value) => Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(value)));
}
