using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;

/// <summary>Original save input retained for one company and actor; it is not evidence of a committed selection.</summary>
public sealed record GhgSelectionIntent(CompanyScope Company, string Actor, CompanyGhgSelectionRequest Request)
{
    public void Require() => CompanyGhgSelectionIdentity.RequireRequest(Company, Actor, Request);
}

public sealed record CompanyEnvironmentIntent(CompanyScope Company, string Actor, CompanyEnvironmentReviewedSaveRequest Request)
{
    public void Require()
    {
        ArgumentNullException.ThrowIfNull(Request);
        ArgumentNullException.ThrowIfNull(Request.Change);
        ArgumentNullException.ThrowIfNull(Request.Review);
        var review = Request.Review;
        if (string.IsNullOrWhiteSpace(Actor) || Actor.Length > 128 || Actor.Any(char.IsControl) || review.Settings is null ||
            review.Settings.Company != Company || review.Settings.CountryCode != Request.Change.CountryCode ||
            review.Settings.Warnings is null || review.Settings.Warnings.Length != 0)
            throw new ArgumentException("The reviewed environment request is invalid.");
        _ = CompanyEnvironmentIdentity.ForChange(Company, Request.Change, review.ToolsVersion);
        if (review.DraftRevision != CompanyEnvironmentDraftIdentity.ForReview(Company, new(Request.Change.CountryCode, Request.Change.OverridesJson), review.SystemRevision, review.CountryRevision, review.ToolsVersion))
            throw new ArgumentException("The environment request does not match its review.");
    }
}

public sealed record SystemSetupIntent(string Actor, SystemSetupRequest Request)
{
    public void Require()
    {
        if (string.IsNullOrWhiteSpace(Actor) || Actor.Length > 128 || Actor.Any(char.IsControl))
            throw new ArgumentException("A valid actor is required.");
        _ = SystemSetupIdentity.ForChange(Request);
    }
}

public sealed record InitialCompanyIntent(string Actor, InitialCompanyProvisionRequest Request)
{
    public void Require()
    {
        if (string.IsNullOrWhiteSpace(Actor) || Actor.Length > 128 || Actor.Any(char.IsControl)) throw new ArgumentException("A valid actor is required.");
        ArgumentNullException.ThrowIfNull(Request); Request.Require();
    }
}
public sealed record CompanyMembershipIntent(CompanyScope Company, string Actor, CompanyMembershipRequest Request)
{
    public void Require()
    {
        _ = CompanyGhgSelectionIdentity.ForCompany(Company);
        if (string.IsNullOrWhiteSpace(Actor) || Actor.Length > 128 || Actor.Any(char.IsControl)) throw new ArgumentException("A valid actor is required.");
        ArgumentNullException.ThrowIfNull(Request); Request.Require();
    }
}
public interface ICompanyMembershipIntentStore
{
    Task<CompanyMembershipIntent?> ReadMembershipAsync(CompanyScope company, string actor);
    Task RetainMembershipAsync(CompanyMembershipIntent intent);
    Task ClearMembershipAsync(CompanyMembershipIntent intent);
    Task<CompanyMembershipRetirementIntent?> ReadMembershipRetirementAsync(CompanyScope company, string actor);
    Task RetainMembershipRetirementAsync(CompanyMembershipRetirementIntent intent);
    Task ClearMembershipRetirementAsync(CompanyMembershipRetirementIntent intent);
}
public sealed record CompanyMembershipRetirementIntent(CompanyScope Company, string Actor, string UserId, CompanyMembershipRetirementRequest Request)
{
    public void Require()
    {
        _ = CompanyGhgSelectionIdentity.ForCompany(Company);
        static bool Valid(string? value) => !string.IsNullOrWhiteSpace(value) && value.Length <= 128 && value == value.Trim() && !value.Any(char.IsControl);
        if (!Valid(Actor) || !Valid(UserId)) throw new ArgumentException("A valid account and actor are required.");
        ArgumentNullException.ThrowIfNull(Request); Request.Require();
    }
}
public sealed record CompanyRoleIntent(CompanyScope Company, string Actor, string UserId, AssignCompanyRoleRequest Request)
{
    public void Require()
    {
        _ = CompanyGhgSelectionIdentity.ForCompany(Company);
        static bool Valid(string? value, int limit) => !string.IsNullOrWhiteSpace(value) && value.Length <= limit && value == value.Trim() && !value.Any(char.IsControl);
        if (!Valid(Actor, 128) || !Valid(UserId, 128) || Request is null || !Valid(Request.RoleId, 128) || !Valid(Request.Reason, 1000))
            throw new ArgumentException("A valid company role request is required.");
    }
}
public interface ICompanyRoleIntentStore
{
    Task<CompanyRoleIntent?> ReadRoleAsync(CompanyScope company, string actor);
    Task RetainRoleAsync(CompanyRoleIntent intent);
    Task ClearRoleAsync(CompanyRoleIntent intent);
    Task<CompanyRoleRevocationIntent?> ReadRevocationAsync(CompanyScope company, string actor);
    Task RetainRevocationAsync(CompanyRoleRevocationIntent intent);
    Task ClearRevocationAsync(CompanyRoleRevocationIntent intent);
}
public sealed record CompanyRoleRevocationIntent(CompanyScope Company, string Actor, string UserId, string AssignmentId)
{
    public void Require()
    {
        _ = CompanyGhgSelectionIdentity.ForCompany(Company);
        static bool Valid(string? value) => !string.IsNullOrWhiteSpace(value) && value.Length <= 128 && value == value.Trim() && !value.Any(char.IsControl);
        if (!Valid(Actor) || !Valid(UserId) || !Valid(AssignmentId)) throw new ArgumentException("A valid role revocation is required.");
    }
}
public interface IInitialCompanyIntentStore
{
    Task<InitialCompanyIntent?> ReadInitialCompanyAsync(string actor);
    Task RetainInitialCompanyAsync(InitialCompanyIntent intent);
    Task ClearInitialCompanyAsync(InitialCompanyIntent intent);
}

public interface ISystemSetupIntentStore
{
    Task<SystemSetupIntent?> ReadSystemAsync(string actor);
    Task RetainSystemAsync(SystemSetupIntent intent);
    Task ClearSystemAsync(SystemSetupIntent intent);
}

public interface ICompanyEnvironmentIntentStore
{
    Task<CompanyEnvironmentIntent?> ReadEnvironmentAsync(CompanyScope company, string actor);
    Task RetainEnvironmentAsync(CompanyEnvironmentIntent intent);
    Task ClearEnvironmentAsync(CompanyEnvironmentIntent intent);
}

public interface IGhgSelectionIntentStore
{
    Task<GhgSelectionIntent?> ReadAsync(CompanyScope company, string actor);
    Task RetainAsync(GhgSelectionIntent intent);
    Task ClearAsync(GhgSelectionIntent intent);
}

/// <summary>Protected tab-session recovery input; authoritative state stays in the system repository.</summary>
public sealed class CompanySetupIntentStore(ProtectedSessionStorage storage) : IGhgSelectionIntentStore, ICompanyEnvironmentIntentStore, ISystemSetupIntentStore, IInitialCompanyIntentStore, ICompanyMembershipIntentStore, ICompanyRoleIntentStore
{
    private readonly SemaphoreSlim gate = new(1, 1);
    public async Task<CompanyMembershipRetirementIntent?> ReadMembershipRetirementAsync(CompanyScope company, string actor)
    {
        var result = await storage.GetAsync<CompanyMembershipRetirementIntent>(Key(company, actor, "company.membership.retirement"));
        if (!result.Success) return null;
        var intent = result.Value ?? throw new InvalidOperationException("The retained membership retirement is unavailable.");
        intent.Require();
        if (intent.Company != company || intent.Actor != actor) throw new InvalidOperationException("The retirement belongs to another company or actor.");
        return intent;
    }
    public async Task RetainMembershipRetirementAsync(CompanyMembershipRetirementIntent intent)
    {
        intent.Require(); await gate.WaitAsync();
        try
        {
            var old = await ReadMembershipRetirementAsync(intent.Company, intent.Actor);
            if (old is not null && old != intent) throw new InvalidOperationException("Recover the original membership retirement first.");
            await storage.SetAsync(Key(intent.Company, intent.Actor, "company.membership.retirement"), intent);
            if (await ReadMembershipRetirementAsync(intent.Company, intent.Actor) != intent) throw new InvalidOperationException("The retirement could not be retained.");
        }
        finally { gate.Release(); }
    }
    public async Task ClearMembershipRetirementAsync(CompanyMembershipRetirementIntent intent)
    {
        intent.Require(); await gate.WaitAsync();
        try
        {
            var old = await ReadMembershipRetirementAsync(intent.Company, intent.Actor);
            if (old is not null && old != intent) throw new InvalidOperationException("A different retirement was preserved.");
            await storage.DeleteAsync(Key(intent.Company, intent.Actor, "company.membership.retirement"));
        }
        finally { gate.Release(); }
    }
    public async Task<CompanyRoleRevocationIntent?> ReadRevocationAsync(CompanyScope company, string actor)
    {
        var result = await storage.GetAsync<CompanyRoleRevocationIntent>(Key(company, actor, "company.role.revocation"));
        if (!result.Success) return null;
        var intent = result.Value ?? throw new InvalidOperationException("The retained revocation is unavailable.");
        intent.Require();
        if (intent.Company != company || intent.Actor != actor) throw new InvalidOperationException("The revocation belongs to another company or actor.");
        return intent;
    }
    public async Task RetainRevocationAsync(CompanyRoleRevocationIntent intent)
    {
        intent.Require(); await gate.WaitAsync();
        try
        {
            var old = await ReadRevocationAsync(intent.Company, intent.Actor);
            if (old is not null && old != intent) throw new InvalidOperationException("Review the original revocation first.");
            await storage.SetAsync(Key(intent.Company, intent.Actor, "company.role.revocation"), intent);
            if (await ReadRevocationAsync(intent.Company, intent.Actor) != intent) throw new InvalidOperationException("The revocation could not be retained.");
        }
        finally { gate.Release(); }
    }
    public async Task ClearRevocationAsync(CompanyRoleRevocationIntent intent)
    {
        intent.Require(); await gate.WaitAsync();
        try
        {
            var old = await ReadRevocationAsync(intent.Company, intent.Actor);
            if (old is not null && old != intent) throw new InvalidOperationException("A different revocation was preserved.");
            await storage.DeleteAsync(Key(intent.Company, intent.Actor, "company.role.revocation"));
        }
        finally { gate.Release(); }
    }
    public async Task<CompanyRoleIntent?> ReadRoleAsync(CompanyScope company, string actor)
    {
        var result = await storage.GetAsync<CompanyRoleIntent>(Key(company, actor, "company.role"));
        if (!result.Success) return null;
        var intent = result.Value ?? throw new InvalidOperationException("The retained role request is unavailable.");
        intent.Require();
        if (intent.Company != company || intent.Actor != actor) throw new InvalidOperationException("The role request belongs to another company or actor.");
        return intent;
    }
    public async Task RetainRoleAsync(CompanyRoleIntent intent)
    {
        intent.Require(); await gate.WaitAsync();
        try
        {
            var old = await ReadRoleAsync(intent.Company, intent.Actor);
            if (old is not null && old != intent) throw new InvalidOperationException("Review the original role request first.");
            await storage.SetAsync(Key(intent.Company, intent.Actor, "company.role"), intent);
            if (await ReadRoleAsync(intent.Company, intent.Actor) != intent) throw new InvalidOperationException("The role request could not be retained.");
        }
        finally { gate.Release(); }
    }
    public async Task ClearRoleAsync(CompanyRoleIntent intent)
    {
        intent.Require(); await gate.WaitAsync();
        try
        {
            var old = await ReadRoleAsync(intent.Company, intent.Actor);
            if (old is not null && old != intent) throw new InvalidOperationException("A different role request was preserved.");
            await storage.DeleteAsync(Key(intent.Company, intent.Actor, "company.role"));
        }
        finally { gate.Release(); }
    }
    public async Task<CompanyMembershipIntent?> ReadMembershipAsync(CompanyScope company, string actor)
    {
        var result = await storage.GetAsync<CompanyMembershipIntent>(Key(company, actor, "company.membership"));
        if (!result.Success) return null;
        var intent = result.Value ?? throw new InvalidOperationException("The retained membership request is unavailable.");
        intent.Require();
        if (intent.Company != company || intent.Actor != actor) throw new InvalidOperationException("The membership request belongs to another company or actor.");
        return intent;
    }
    public async Task RetainMembershipAsync(CompanyMembershipIntent intent)
    {
        intent.Require(); await gate.WaitAsync();
        try
        {
            var old = await ReadMembershipAsync(intent.Company, intent.Actor);
            if (old is not null && old != intent) throw new InvalidOperationException("Recover the original membership request first.");
            await storage.SetAsync(Key(intent.Company, intent.Actor, "company.membership"), intent);
            if (await ReadMembershipAsync(intent.Company, intent.Actor) != intent) throw new InvalidOperationException("The original membership request could not be retained.");
        }
        finally { gate.Release(); }
    }
    public async Task ClearMembershipAsync(CompanyMembershipIntent intent)
    {
        intent.Require(); await gate.WaitAsync();
        try
        {
            var old = await ReadMembershipAsync(intent.Company, intent.Actor);
            if (old is not null && old != intent) throw new InvalidOperationException("A different retained membership request was preserved.");
            await storage.DeleteAsync(Key(intent.Company, intent.Actor, "company.membership"));
        }
        finally { gate.Release(); }
    }
    private static string Key(CompanyScope company, string actor, string kind = "ghg.selection")
    {
        _ = CompanyGhgSelectionIdentity.ForCompany(company);
        if (string.IsNullOrWhiteSpace(actor) || actor.Length > 128 || actor.Any(char.IsControl)) throw new ArgumentException("A valid actor is required.");
        return "oilgas." + kind + "." + Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new { Company = company, Actor = actor })));
    }
    public async Task<GhgSelectionIntent?> ReadAsync(CompanyScope company, string actor)
    {
        var result = await storage.GetAsync<GhgSelectionIntent>(Key(company, actor));
        if (!result.Success) return null;
        var intent = result.Value ?? throw new InvalidOperationException("The retained selection is unavailable.");
        intent.Require();
        if (intent.Company != company || intent.Actor != actor) throw new InvalidOperationException("The retained selection belongs to another company or actor.");
        return intent;
    }
    public async Task RetainAsync(GhgSelectionIntent intent)
    {
        intent.Require(); await gate.WaitAsync();
        try
        {
            var old = await ReadAsync(intent.Company, intent.Actor);
            if (old is not null && old != intent) throw new InvalidOperationException("Recover the original selection before starting another.");
            await storage.SetAsync(Key(intent.Company, intent.Actor), intent);
            if (await ReadAsync(intent.Company, intent.Actor) != intent) throw new InvalidOperationException("The original selection was not confirmed retained.");
        }
        finally { gate.Release(); }
    }
    public async Task ClearAsync(GhgSelectionIntent intent)
    {
        intent.Require(); await gate.WaitAsync();
        try
        {
            var old = await ReadAsync(intent.Company, intent.Actor);
            if (old is not null && old != intent) throw new InvalidOperationException("A different retained selection was preserved.");
            await storage.DeleteAsync(Key(intent.Company, intent.Actor));
        }
        finally { gate.Release(); }
    }
    private static bool Same(CompanyEnvironmentIntent left, CompanyEnvironmentIntent right) =>
        JsonSerializer.Serialize(left) == JsonSerializer.Serialize(right);

    public async Task<CompanyEnvironmentIntent?> ReadEnvironmentAsync(CompanyScope company, string actor)
    {
        var result = await storage.GetAsync<CompanyEnvironmentIntent>(Key(company, actor, "environment"));
        if (!result.Success) return null;
        var intent = result.Value ?? throw new InvalidOperationException("The retained environment request is unavailable.");
        intent.Require();
        if (intent.Company != company || intent.Actor != actor) throw new InvalidOperationException("The retained environment belongs to another company or actor.");
        return intent;
    }
    public async Task RetainEnvironmentAsync(CompanyEnvironmentIntent intent)
    {
        intent.Require(); await gate.WaitAsync();
        try
        {
            var old = await ReadEnvironmentAsync(intent.Company, intent.Actor);
            if (old is not null && !Same(old, intent)) throw new InvalidOperationException("Recover the original environment before starting another.");
            await storage.SetAsync(Key(intent.Company, intent.Actor, "environment"), intent);
            var saved = await ReadEnvironmentAsync(intent.Company, intent.Actor);
            if (saved is null || !Same(saved, intent)) throw new InvalidOperationException("The original environment request was not confirmed retained.");
        }
        finally { gate.Release(); }
    }
    public async Task ClearEnvironmentAsync(CompanyEnvironmentIntent intent)
    {
        intent.Require(); await gate.WaitAsync();
        try
        {
            var old = await ReadEnvironmentAsync(intent.Company, intent.Actor);
            if (old is not null && !Same(old, intent)) throw new InvalidOperationException("A different retained environment was preserved.");
            await storage.DeleteAsync(Key(intent.Company, intent.Actor, "environment"));
        }
        finally { gate.Release(); }
    }

    private static string SystemKey(string actor)
    {
        if (string.IsNullOrWhiteSpace(actor) || actor.Length > 128 || actor.Any(char.IsControl)) throw new ArgumentException("A valid actor is required.");
        return "oilgas.system.setup." + Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(actor)));
    }
    public async Task<SystemSetupIntent?> ReadSystemAsync(string actor)
    {
        var result = await storage.GetAsync<SystemSetupIntent>(SystemKey(actor));
        if (!result.Success) return null;
        var intent = result.Value ?? throw new InvalidOperationException("The retained shared setup is unavailable.");
        intent.Require();
        if (intent.Actor != actor) throw new InvalidOperationException("The retained setup belongs to another actor.");
        return intent;
    }
    public async Task RetainSystemAsync(SystemSetupIntent intent)
    {
        intent.Require(); await gate.WaitAsync();
        try
        {
            var old = await ReadSystemAsync(intent.Actor);
            if (old is not null && old != intent) throw new InvalidOperationException("Recover the original setup before starting another.");
            await storage.SetAsync(SystemKey(intent.Actor), intent);
            if (await ReadSystemAsync(intent.Actor) != intent) throw new InvalidOperationException("The original setup was not confirmed retained.");
        }
        finally { gate.Release(); }
    }
    public async Task ClearSystemAsync(SystemSetupIntent intent)
    {
        intent.Require(); await gate.WaitAsync();
        try
        {
            var old = await ReadSystemAsync(intent.Actor);
            if (old is not null && old != intent) throw new InvalidOperationException("A different retained setup was preserved.");
            await storage.DeleteAsync(SystemKey(intent.Actor));
        }
        finally { gate.Release(); }
    }

    private static string InitialCompanyKey(string actor) => "oilgas.initial.company." + SystemKey(actor);
    public async Task<InitialCompanyIntent?> ReadInitialCompanyAsync(string actor)
    {
        var result = await storage.GetAsync<InitialCompanyIntent>(InitialCompanyKey(actor));
        if (!result.Success) return null;
        var intent = result.Value ?? throw new InvalidOperationException("The retained company setup is unavailable.");
        intent.Require();
        if (intent.Actor != actor) throw new InvalidOperationException("The retained setup belongs to another actor.");
        return intent;
    }
    public async Task RetainInitialCompanyAsync(InitialCompanyIntent intent)
    {
        intent.Require(); await gate.WaitAsync();
        try
        {
            var old = await ReadInitialCompanyAsync(intent.Actor);
            if (old is not null && old != intent) throw new InvalidOperationException("Recover the original company setup first.");
            await storage.SetAsync(InitialCompanyKey(intent.Actor), intent);
            if (await ReadInitialCompanyAsync(intent.Actor) != intent) throw new InvalidOperationException("The original request was not confirmed retained.");
        }
        finally { gate.Release(); }
    }
    public async Task ClearInitialCompanyAsync(InitialCompanyIntent intent)
    {
        intent.Require(); await gate.WaitAsync();
        try
        {
            var old = await ReadInitialCompanyAsync(intent.Actor);
            if (old is not null && old != intent) throw new InvalidOperationException("A different retained request was preserved.");
            await storage.DeleteAsync(InitialCompanyKey(intent.Actor));
        }
        finally { gate.Release(); }
    }

}
