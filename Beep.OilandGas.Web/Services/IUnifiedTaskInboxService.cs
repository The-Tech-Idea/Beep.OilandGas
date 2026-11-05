using Beep.OilandGas.Models.Data.Company;
namespace Beep.OilandGas.Web.Services;
public interface IUnifiedTaskInboxService
{
    Task<UnifiedInbox> GetInboxAsync(string personaCode);
    Task<InboxCounts> GetInboxCountsAsync(string personaCode);
    Task<List<UnifiedTask>> GetFilteredTasksAsync(InboxFilter filter, string personaCode);
}

