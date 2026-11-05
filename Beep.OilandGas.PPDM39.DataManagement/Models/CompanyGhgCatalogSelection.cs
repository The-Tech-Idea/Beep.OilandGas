using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using System.Security.Cryptography;
using System.Text.Json;

namespace Beep.OilandGas.PPDM39.DataManagement.Models;

/// <summary>A proposed company selection is a record of intent, not factor approval.</summary>
public sealed record CompanyGhgSelectionRequest(string Jurisdiction, string CatalogRevision, string EnvironmentRevision,
    string Rationale, string ExpectedRevision, string IntentId);

public sealed record CompanyGhgSelection(CompanyScope Company, string Jurisdiction, string CatalogRevision,
    string EnvironmentRevision, string SelectedBy, string Rationale, string Revision, string PreviousRevision, string IntentId);
public sealed record CompanyGhgSelectionReadResult(CompanyScope Company, CompanyGhgSelection? Selection, string Status);

public static class CompanyGhgSelectionIdentity
{
    public static string ForCompany(CompanyScope company) => CompanyEnvironmentIdentity.ForCompany(company);

    public static void RequireRequest(CompanyScope company, string actor, CompanyGhgSelectionRequest request) => _ = ForChange(company, actor, request);

    public static CompanyGhgSelection Prepare(CompanyScope company, string actor, CompanyGhgSelectionRequest request, CompanyGhgSelection? current)
    {
        var revision = ForChange(company, actor, request);
        if (current is not null)
        {
            RequireValid(current, company);
            if (current.IntentId == request.IntentId)
            {
                if (current.Revision != revision) throw new InvalidOperationException("The selection intent belongs to another change.");
                return current;
            }
        }
        if ((current?.Revision ?? "") != request.ExpectedRevision) throw new InvalidOperationException("The company catalog selection changed.");
        return new(company, request.Jurisdiction, request.CatalogRevision, request.EnvironmentRevision, actor,
            request.Rationale, revision, request.ExpectedRevision, request.IntentId);
    }

    public static void RequireValid(CompanyGhgSelection selection, CompanyScope company)
    {
        ArgumentNullException.ThrowIfNull(selection);
        if (selection.Company != company) throw new InvalidOperationException("The selection belongs to another company.");
        var request = new CompanyGhgSelectionRequest(selection.Jurisdiction, selection.CatalogRevision, selection.EnvironmentRevision,
            selection.Rationale, selection.PreviousRevision, selection.IntentId);
        if (selection.Revision != ForChange(company, selection.SelectedBy, request))
            throw new InvalidOperationException("The selection does not match its recorded revision.");
    }

    private static string ForChange(CompanyScope company, string actor, CompanyGhgSelectionRequest request)
    {
        _ = ForCompany(company); ArgumentNullException.ThrowIfNull(request);
        _ = SystemSetupIdentity.ForKey("GHG_FACTORS", request.Jurisdiction);
        if (string.IsNullOrWhiteSpace(actor) || actor.Length > 128 || actor.Any(char.IsControl) ||
            !CompanyEnvironmentIdentity.IsRevision(request.CatalogRevision) || !CompanyEnvironmentIdentity.IsRevision(request.EnvironmentRevision) ||
            string.IsNullOrWhiteSpace(request.Rationale) || request.Rationale.Length > 2000 || request.Rationale.Any(char.IsControl) ||
            request.ExpectedRevision is null || (request.ExpectedRevision.Length != 0 && !CompanyEnvironmentIdentity.IsRevision(request.ExpectedRevision)) ||
            !Guid.TryParseExact(request.IntentId, "N", out var intent) || intent == Guid.Empty || intent.ToString("N") != request.IntentId)
            throw new ArgumentException("The reviewed company selection is incomplete or invalid.");
        return Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new { Company = company, Actor = actor, Request = request })));
    }
}
