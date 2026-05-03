using System.Text.Json.Serialization;

namespace Beep.OilandGas.Client.App.Services.Calculations
{
    /// <summary>JSON body from POST /api/NodalAnalysis/result on success.</summary>
    internal sealed class NodalAnalysisSaveApiResponse
    {
        [JsonPropertyName("message")]
        public string? message { get; set; }

        [JsonPropertyName("analysisId")]
        public string? analysisId { get; set; }
    }
}
