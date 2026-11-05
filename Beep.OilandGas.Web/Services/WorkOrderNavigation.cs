using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Company;
namespace Beep.OilandGas.Web.Services;

/// <summary>Query transport preserves opaque recorded order keys, including slash and percent characters.</summary>
public static class WorkOrderNavigation
{
    public static string Detail(string id)
    {
        if(!Beep.OilandGas.Models.Data.Lease.LeaseSummaryValidator.Identifier(id))throw new ArgumentException("A recorded work-order identity is required.",nameof(id));
        return "/ppdm39/workorder/detail?workOrderId="+Uri.EscapeDataString(id);
    }
}
