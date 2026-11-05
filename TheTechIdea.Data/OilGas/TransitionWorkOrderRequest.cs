namespace Beep.OilandGas.Models.Data.WorkOrder;
public sealed class TransitionWorkOrderRequest
{
    [System.Text.Json.Serialization.JsonRequired]
    public string ExpectedFieldId { get; set; } = string.Empty;
    [System.Text.Json.Serialization.JsonRequired]
    public string ExpectedState { get; set; } = string.Empty;
    public string ToState { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
