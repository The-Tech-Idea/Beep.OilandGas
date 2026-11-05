namespace Beep.OilandGas.Models.Data.Process
{
    public class ProcessTransitionRequest
    {
        [System.Text.Json.Serialization.JsonRequired]
        public string ExpectedRevision { get; set; } = string.Empty;
        public string Trigger { get; set; } = string.Empty;
        public string ToStateId { get; set; } = string.Empty;
        public string FromStateId { get; set; } = string.Empty;
    }

}
