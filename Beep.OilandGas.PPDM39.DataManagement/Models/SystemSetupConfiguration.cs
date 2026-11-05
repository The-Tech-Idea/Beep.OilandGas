using Beep.OilandGas.Models.Data.Company;
using System.Security.Cryptography;
using System.Text.Json;

namespace Beep.OilandGas.PPDM39.DataManagement.Models;

/// <summary>Installation-wide setup records do not contain company credentials or connection strings.</summary>
public sealed record SystemSetupRequest(string Category, string Key, string Version, string SettingsJson, string ExpectedRevision, string IntentId);
public sealed record SystemSetupConfiguration(string Category, string Key, string Version, string SettingsJson, string Revision, string PreviousRevision, string IntentId);
public sealed record SystemSetupReadResult(string Category, string Key, SystemSetupConfiguration? Configuration);
public sealed record SystemSetupToolsDefaultDraft(string ToolsVersion, string SettingsJson);

public static class SystemSetupIdentity
{
    public const int MaximumSettingsLength = 1024 * 1024;
    public static string ForKey(string category, string key)
    {
        RequireIdentifier(category, 64); RequireIdentifier(key, 128);
        return Hash(new { Category = category, Key = key });
    }
    public static string ForChange(SystemSetupRequest request)
    {
        ArgumentNullException.ThrowIfNull(request); _ = ForKey(request.Category, request.Key);
        if (string.IsNullOrWhiteSpace(request.Version) || request.Version.Length > 128 || request.Version.Any(char.IsControl) ||
            string.IsNullOrWhiteSpace(request.SettingsJson) || request.SettingsJson.Length > MaximumSettingsLength || request.ExpectedRevision is null ||
            request.ExpectedRevision.Length != 0 && !CompanyEnvironmentIdentity.IsRevision(request.ExpectedRevision) ||
            !Guid.TryParseExact(request.IntentId, "N", out var intent) || intent == Guid.Empty || intent.ToString("N") != request.IntentId)
            throw new ArgumentException("The system setup change is incomplete or invalid.");
        return Hash(request);
    }
    public static SystemSetupConfiguration Prepare(SystemSetupRequest request, SystemSetupConfiguration? current)
    {
        var revision = ForChange(request);
        if (current is not null)
        {
            RequireValid(current, request.Category, request.Key);
            if (current.IntentId == request.IntentId)
            {
                if (current.Revision != revision) throw new InvalidOperationException("The system setup intent belongs to a different change.");
                return current;
            }
        }
        if ((current?.Revision ?? "") != request.ExpectedRevision) throw new InvalidOperationException("The system setup record changed. Reload it before saving.");
        return new(request.Category, request.Key, request.Version, request.SettingsJson, revision, request.ExpectedRevision, request.IntentId);
    }
    public static void RequireValid(SystemSetupConfiguration value, string category, string key)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.Category != category || value.Key != key || value.Revision != ForChange(new(value.Category, value.Key, value.Version, value.SettingsJson, value.PreviousRevision, value.IntentId)))
            throw new InvalidOperationException("The saved system setup record does not match its identity and revision.");
    }
    private static void RequireIdentifier(string value, int max)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > max || value.Any(c => !(c is >= 'A' and <= 'Z' or >= '0' and <= '9' or '_' or '-')))
            throw new ArgumentException("Use an uppercase setup category and key.");
    }
    private static string Hash<T>(T value) => Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(value)));
}
