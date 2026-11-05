using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.HSE.Models;

namespace Beep.OilandGas.Web.Services;

public partial class HSEServiceClient
{
    public async Task<CompanyHseAssignedActions> GetAssignedOverdueActionsAsync(FieldSelectionReview context, CancellationToken cancellationToken = default)
    {
        RequireClosureCompany(context);
        var snapshot = _apiClient.CompanySnapshot;
        var result = await _apiClient.GetAsync<CompanyHseAssignedActions>("/api/field/current/hse/assigned-overdue-actions", cancellationToken)
            ?? throw new InvalidOperationException("Assigned overdue actions were not returned.");
        result.Require();
        if (snapshot != _apiClient.CompanySnapshot || result.Context != context)
            throw new InvalidOperationException("Assigned actions belong to another company, account or FIELD.");
        return result;
    }
}
