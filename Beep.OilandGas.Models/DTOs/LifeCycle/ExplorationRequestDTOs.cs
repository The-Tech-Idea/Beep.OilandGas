using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs.LifeCycle
{
    /// <summary>
    /// Request DTO for starting lead to prospect process
    /// </summary>
    public class StartLeadToProspectRequest
    {
        public string LeadId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for evaluating a lead
    /// </summary>
    public class EvaluateLeadRequest
    {
        public string InstanceId { get; set; } = string.Empty;
        public Dictionary<string, object>? EvaluationData { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for approving a lead
    /// </summary>
    public class ApproveLeadRequest
    {
        public string InstanceId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for starting prospect to discovery process
    /// </summary>
    public class StartProspectToDiscoveryRequest
    {
        public string ProspectId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for starting discovery to development process
    /// </summary>
    public class StartDiscoveryToDevelopmentRequest
    {
        public string DiscoveryId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }
}
