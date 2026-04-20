using Beep.OilandGas.Models.Data.Integrations;

namespace Beep.OilandGas.Models.Core.Interfaces;

public interface IOsduAdapter
{
    Task<OsduSyncResult> SyncWellAsync(string osduWellId, string fieldId, string userId);
    Task<OsduSyncResult> SyncPoolAsync(string osduPoolId, string fieldId, string userId);
    Task<OsduSyncResult> SyncSeismicSurveyAsync(string osduSurveyId, string fieldId, string userId);
    Task<List<OsduEntitySummary>> SearchWellsAsync(string fieldName, int maxResults = 100);
}
