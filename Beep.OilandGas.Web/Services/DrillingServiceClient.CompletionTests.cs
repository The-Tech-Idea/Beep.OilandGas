using Beep.OilandGas.DrillingAndConstruction.Models;
namespace Beep.OilandGas.Web.Services;
public partial class DrillingServiceClient
{
    public async Task<DrillingCompletionTestInspection> InspectCompletionTestAsync(DrillingCompletionTestSelection selection,CancellationToken cancellationToken=default)
    {
        selection.Require();var result=await _apiClient.PostAsync<DrillingCompletionTestSelection,DrillingCompletionTestInspection>("/api/drilling/programs/execution/completion/test/inspect",selection,cancellationToken)
            ??throw new InvalidOperationException("The selected native completion test is unavailable.");result.Require(selection);return result;
    }
}
