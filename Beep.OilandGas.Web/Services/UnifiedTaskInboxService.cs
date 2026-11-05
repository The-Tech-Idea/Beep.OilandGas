using System.Globalization;
using Microsoft.AspNetCore.WebUtilities;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;

public interface IUnifiedTaskInboxService
{
    Task<UnifiedInbox> GetInboxAsync(string personaCode);
    Task<InboxCounts> GetInboxCountsAsync(string personaCode);
    Task<List<UnifiedTask>> GetFilteredTasksAsync(InboxFilter filter, string personaCode);
}

/// <remarks>
/// Through <see cref="ApiClient"/>, which carries the API's address and the signed-in person's token and answers a refusal or
/// failure as <see cref="OilGasApiException"/>: the unnamed <see cref="HttpClient"/> this used had neither, so every inbox
/// request failed before it was sent.
/// </remarks>
public class UnifiedTaskInboxService(ApiClient api) : IUnifiedTaskInboxService
{
    public Task<UnifiedInbox> GetInboxAsync(string personaCode) =>
        GetAsync<UnifiedInbox>(QueryHelpers.AddQueryString("/api/workflow/tasks/inbox", "personaCode", personaCode));

    public Task<InboxCounts> GetInboxCountsAsync(string personaCode) =>
        GetAsync<InboxCounts>(QueryHelpers.AddQueryString("/api/workflow/tasks/counts", "personaCode", personaCode));

    public Task<List<UnifiedTask>> GetFilteredTasksAsync(InboxFilter filter, string personaCode) =>
        GetAsync<List<UnifiedTask>>(QueryHelpers.AddQueryString("/api/workflow/tasks", new Dictionary<string, string?>
        {
            ["personaCode"] = personaCode, ["taskType"] = filter.TaskType,
            ["minPriority"] = filter.MinPriority?.ToString(CultureInfo.InvariantCulture),
            ["dueBefore"] = filter.DueBefore?.ToString("O", CultureInfo.InvariantCulture),
            ["workflow"] = filter.WorkflowName, ["sortBy"] = filter.SortBy,
            ["pageSize"] = filter.PageSize.ToString(CultureInfo.InvariantCulture),
            ["page"] = filter.PageNumber.ToString(CultureInfo.InvariantCulture)
        }));

    private async Task<T> GetAsync<T>(string uri) =>
        await api.GetAsync<T>(uri) ?? throw new InvalidOperationException("The task API returned an empty response.");
}
