using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs.LifeCycle
{
    /// <summary>
    /// Request DTO for starting pool definition process
    /// </summary>
    public class StartPoolDefinitionRequest
    {
        public string PoolId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for delineating a pool
    /// </summary>
    public class DelineatePoolRequest
    {
        public string InstanceId { get; set; } = string.Empty;
        public Dictionary<string, object>? DelineationData { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for assigning reserves
    /// </summary>
    public class AssignReservesRequest
    {
        public string InstanceId { get; set; } = string.Empty;
        public Dictionary<string, object>? ReserveData { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for approving a pool
    /// </summary>
    public class ApprovePoolRequest
    {
        public string InstanceId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for activating a pool
    /// </summary>
    public class ActivatePoolRequest
    {
        public string InstanceId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for starting facility development
    /// </summary>
    public class StartFacilityDevelopmentRequest
    {
        public string FacilityId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for starting well development
    /// </summary>
    public class StartWellDevelopmentRequest
    {
        public string WellId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for starting pipeline development
    /// </summary>
    public class StartPipelineDevelopmentRequest
    {
        public string PipelineId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }
}



