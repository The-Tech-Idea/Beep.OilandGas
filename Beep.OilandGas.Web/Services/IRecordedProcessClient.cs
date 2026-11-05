using Beep.OilandGas.Models.Processes;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
/// <summary>Read projection of the existing workflow client for the recorded process page.</summary>
public interface IRecordedProcessClient
{
    Task<ProcessInstance?> GetInstanceAsync(string instanceId,CancellationToken cancellationToken=default);
    Task<List<ProcessHistoryEntry>> GetHistoryAsync(string instanceId,CancellationToken cancellationToken=default);
    Task<ProcessTransitionReview> GetTransitionReviewAsync(string instanceId);
}
