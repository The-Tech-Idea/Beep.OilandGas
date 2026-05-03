using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.EconomicAnalysis;

namespace Beep.OilandGas.ApiService.Controllers.Common;

internal static class EconomicAnalysisControllerHelpers
{
    public static CashFlow[] ToCashFlows(List<double>? values)
    {
        return values?
            .Select((amount, index) => new CashFlow(index, amount))
            .ToArray() ?? Array.Empty<CashFlow>();
    }

    public static bool TryValidateSaveRequest(SaveAnalysisResultRequest? request, out string error)
    {
        if (request == null)
        {
            error = "Request payload is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.AnalysisId))
        {
            error = "Analysis ID is required.";
            return false;
        }

        if (request.Result == null)
        {
            error = "Result payload is required.";
            return false;
        }

        error = string.Empty;
        return true;
    }
}
