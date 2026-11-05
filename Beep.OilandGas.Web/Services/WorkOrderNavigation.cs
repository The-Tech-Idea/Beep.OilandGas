namespace Beep.OilandGas.Web.Services;

/// <summary>Query transport preserves opaque recorded order keys, including slash and percent characters.</summary>
public static class WorkOrderNavigation
{
    public static string Detail(string id)
    {
        if(!TheTechIdea.Data.OilGas.LeaseSummaryValidator.Identifier(id))throw new ArgumentException("A recorded work-order identity is required.",nameof(id));
        return "/ppdm39/workorder/detail?workOrderId="+Uri.EscapeDataString(id);
    }
}
