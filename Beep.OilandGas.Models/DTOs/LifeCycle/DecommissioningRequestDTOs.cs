using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs.LifeCycle
{
    /// <summary>
    /// Request DTO for starting well abandonment process
    /// </summary>
    public class StartWellAbandonmentRequest
    {
        public string WellId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for planning abandonment
    /// </summary>
    public class PlanAbandonmentRequest
    {
        public string InstanceId { get; set; } = string.Empty;
        public Dictionary<string, object>? PlanData { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for obtaining regulatory approval
    /// </summary>
    public class ObtainRegulatoryApprovalRequest
    {
        public string InstanceId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for plugging a well
    /// </summary>
    public class PlugWellRequest
    {
        public string InstanceId { get; set; } = string.Empty;
        public Dictionary<string, object>? PluggingData { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for restoring a site
    /// </summary>
    public class RestoreSiteRequest
    {
        public string InstanceId { get; set; } = string.Empty;
        public Dictionary<string, object>? RestorationData { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for completing abandonment
    /// </summary>
    public class CompleteAbandonmentRequest
    {
        public string InstanceId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for starting facility decommissioning
    /// </summary>
    public class StartFacilityDecommissioningRequest
    {
        public string FacilityId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for planning decommissioning
    /// </summary>
    public class PlanDecommissioningRequest
    {
        public string InstanceId { get; set; } = string.Empty;
        public Dictionary<string, object>? PlanData { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for removing equipment
    /// </summary>
    public class RemoveEquipmentRequest
    {
        public string InstanceId { get; set; } = string.Empty;
        public Dictionary<string, object>? RemovalData { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for cleaning up a site
    /// </summary>
    public class CleanupSiteRequest
    {
        public string InstanceId { get; set; } = string.Empty;
        public Dictionary<string, object>? CleanupData { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for obtaining regulatory closure
    /// </summary>
    public class ObtainRegulatoryClosureRequest
    {
        public string InstanceId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for completing decommissioning
    /// </summary>
    public class CompleteDecommissioningRequest
    {
        public string InstanceId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }
}
