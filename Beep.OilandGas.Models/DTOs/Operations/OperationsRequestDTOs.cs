using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs.Operations
{
    /// <summary>
    /// Request DTO for ranking prospects
    /// </summary>
    public class RankProspectsRequest
    {
        public List<string> ProspectIds { get; set; } = new();
        public Dictionary<string, decimal> RankingCriteria { get; set; } = new();
    }

    /// <summary>
    /// Request DTO for updating lease status
    /// </summary>
    public class UpdateLeaseStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for analyzing EOR potential
    /// </summary>
    public class AnalyzeEORRequest
    {
        public string FieldId { get; set; } = string.Empty;
        public string EorMethod { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for calculating recovery factor
    /// </summary>
    public class CalculateRecoveryFactorRequest
    {
        public string ProjectId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for managing injection operations
    /// </summary>
    public class ManageInjectionRequest
    {
        public string InjectionWellId { get; set; } = string.Empty;
        public decimal InjectionRate { get; set; }
    }
}



