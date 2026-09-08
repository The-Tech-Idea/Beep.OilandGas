using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.Models.Core.Interfaces;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.UserManagement.Services;


public class AuditChainService : IAuditChainService
{
    private readonly IDMEEditor _editor;
    private readonly ICommonColumnHandler _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository _metadata;
    private readonly string _connectionName;
    private readonly ILogger<AuditChainService> _logger;

    public AuditChainService(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        string connectionName = "PPDM39",
        ILogger<AuditChainService>? logger = null)
    {
        _editor = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults = defaults;
        _metadata = metadata;
        _connectionName = connectionName;
        _logger = logger;
    }

    public Task<string> ComputeChainHashAsync(
        string? previousEntryHash,
        string processHistoryId,
        string eventType,
        DateTime eventDate,
        string userId,
        string? fromStatus,
        string? toStatus,
        string? details)
    {
        var input = $"{previousEntryHash ?? "GENESIS"}|{processHistoryId}|{eventType}|{eventDate:O}|{userId}|{fromStatus}|{toStatus}|{details}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        var hashString = Convert.ToHexStringLower(hash);
        return Task.FromResult(hashString);
    }

    public async Task<ChainVerificationResult> VerifyChainIntegrityAsync(string processInstanceId)
    {
        var result = new ChainVerificationResult();
        var entries = await GetHistoryEntriesAsync(processInstanceId);

        result.TotalEntries = entries.Count;
        if (entries.Count == 0)
        {
            result.IsIntact = true;
            return result;
        }

        string? previousHash = null;

        for (var i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];

            // Extract stored hash from EVENT_DATA_JSON
            var storedHash = ExtractChainHash(entry);

            var computedHash = await ComputeChainHashAsync(
                previousHash,
                entry.PROCESS_HISTORY_ID,
                entry.EVENT_TYPE ?? string.Empty,
                entry.EVENT_DATE ?? DateTime.MinValue,
                entry.USER_ID ?? string.Empty,
                entry.FROM_STATUS,
                entry.TO_STATUS,
                entry.DETAILS);

            if (storedHash is not null && !string.Equals(storedHash, computedHash, StringComparison.OrdinalIgnoreCase))
            {
                result.Breaks.Add(new ChainBreakInfo
                {
                    EntryIndex = i,
                    ProcessHistoryId = entry.PROCESS_HISTORY_ID,
                    ExpectedHash = storedHash,
                    ComputedHash = computedHash,
                    BreakType = "HASH_MISMATCH",
                });
            }

            previousHash = computedHash;
            result.VerifiedEntries++;
        }

        result.IsIntact = result.Breaks.Count == 0;

        _logger?.LogInformation(
            "Audit chain verification for {InstanceId}: {Verified}/{Total} verified, Intact={IsIntact}",
            processInstanceId, result.VerifiedEntries, result.TotalEntries, result.IsIntact);

        return result;
    }

    public async Task ChainHistoryEntryAsync(PROCESS_HISTORY entry, string userId)
    {
        // Get the previous entry for this process instance
        var previousHash = await GetPreviousChainHashAsync(entry.PROCESS_INSTANCE_ID);

        var hash = await ComputeChainHashAsync(
            previousHash,
            entry.PROCESS_HISTORY_ID,
            entry.EVENT_TYPE ?? string.Empty,
            entry.EVENT_DATE ?? DateTime.UtcNow,
            entry.USER_ID ?? userId,
            entry.FROM_STATUS,
            entry.TO_STATUS,
            entry.DETAILS);

        // Store hash in EVENT_DATA_JSON
        var eventData = string.IsNullOrWhiteSpace(entry.EVENT_DATA_JSON)
            ? new Dictionary<string, object>()
            : JsonSerializer.Deserialize<Dictionary<string, object>>(entry.EVENT_DATA_JSON) ?? new();

        eventData["chainHash"] = hash;
        eventData["chainedAt"] = DateTime.UtcNow.ToString("O");
        entry.EVENT_DATA_JSON = JsonSerializer.Serialize(eventData);

        var repo = GetHistoryRepo();
        await repo.UpdateAsync(entry, userId);
    }

    private async Task<string?> GetPreviousChainHashAsync(string processInstanceId)
    {
        var entries = await GetHistoryEntriesAsync(processInstanceId);
        if (entries.Count == 0) return null;

        var lastEntry = entries[^1];
        return ExtractChainHash(lastEntry);
    }

    private async Task<List<PROCESS_HISTORY>> GetHistoryEntriesAsync(string processInstanceId)
    {
        var repo = GetHistoryRepo();
        var filters = new List<AppFilter>
        {
            new() { FieldName = "PROCESS_INSTANCE_ID", FilterValue = processInstanceId },
        };
        var results = await repo.GetAsync(filters);
        return results.OfType<PROCESS_HISTORY>()
            .OrderBy(h => h.EVENT_DATE)
            .ThenBy(h => h.ROW_CREATED_DATE)
            .ToList();
    }

    private static string? ExtractChainHash(PROCESS_HISTORY entry)
    {
        if (string.IsNullOrWhiteSpace(entry.EVENT_DATA_JSON)) return null;
        try
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(entry.EVENT_DATA_JSON);
            return data?.TryGetValue("chainHash", out var hash) == true ? hash?.ToString() : null;
        }
        catch { return null; }
    }

    private PPDMGenericRepository GetHistoryRepo() =>
        new(_editor, _commonColumnHandler, _defaults, _metadata,
            typeof(PROCESS_HISTORY), _connectionName, "PROCESS_HISTORY", null);
}
