using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.LifeCycle.Models;
using System.Globalization;
using Microsoft.AspNetCore.WebUtilities;

namespace Beep.OilandGas.Web.Services;

public class UnifiedTaskInboxService(ApiClient api) : IUnifiedTaskInboxService
{
    public async Task<UnifiedInbox> GetInboxAsync(string personaCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(personaCode);
        var original=api.CompanySnapshot;
        if(original.Selection is null)throw new InvalidOperationException("Select a company before loading assigned work.");
        var response=await GetAsync<UnifiedInbox>(QueryHelpers.AddQueryString("/api/workflow/tasks/inbox", "personaCode", personaCode));
        if(original!=api.CompanySnapshot)throw new InvalidOperationException("Company changed while loading assigned work.");
        return WorkflowInboxReview.Capture(response,original.Selection.Scope,original.Selection.UserId,personaCode,original.Selection.Roles);
    }

    public Task<InboxCounts> GetInboxCountsAsync(string personaCode) =>
        GetAsync<InboxCounts>(QueryHelpers.AddQueryString("/api/workflow/tasks/counts", "personaCode", personaCode));

    public async Task<List<UnifiedTask>> GetFilteredTasksAsync(InboxFilter filter, string personaCode)
    {
        ArgumentNullException.ThrowIfNull(filter);ArgumentException.ThrowIfNullOrWhiteSpace(personaCode);
        if (filter.DueBefore is { Kind: not DateTimeKind.Utc }) throw new ArgumentException("The due-before filter must be an explicit UTC instant.");
        var original=api.CompanySnapshot;
        if(original.Selection is null)throw new InvalidOperationException("Select a company before loading assigned work.");
        var rows=await GetAsync<List<UnifiedTask>>(QueryHelpers.AddQueryString("/api/workflow/tasks", new Dictionary<string, string?>
        {
            ["personaCode"] = personaCode, ["taskType"] = filter.TaskType,
            ["minPriority"] = filter.MinPriority?.ToString(CultureInfo.InvariantCulture),
            ["dueBefore"] = filter.DueBefore?.ToString("O", CultureInfo.InvariantCulture),
            ["workflow"] = filter.WorkflowName, ["sortBy"] = filter.SortBy,
            ["pageSize"] = filter.PageSize.ToString(CultureInfo.InvariantCulture),
            ["page"] = filter.PageNumber.ToString(CultureInfo.InvariantCulture)
        }));
        if(original!=api.CompanySnapshot)throw new InvalidOperationException("Company changed while loading assigned work.");
        foreach(var row in rows)
        {
            WorkflowInboxReview.RequireTask(row,original.Selection.Roles);
            row.AccountingStepReview?.Require(original.Selection.Scope,original.Selection.UserId,row.WorkflowName);
        }
        if(rows.Select(row=>row.TaskId).Distinct(StringComparer.Ordinal).Count()!=rows.Count)
            throw new InvalidOperationException("Assigned task identities are ambiguous. Reload assigned work.");
        return rows;
    }

    private async Task<T> GetAsync<T>(string uri) =>
        await api.GetAsync<T>(uri) ?? throw new InvalidOperationException("The task API returned an empty response.");
}
