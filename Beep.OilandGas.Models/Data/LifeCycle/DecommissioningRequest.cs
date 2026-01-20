using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.LifeCycle
{
    /// <summary>
    /// Request DTO for starting well abandonment process
    /// </summary>
    public class StartWellAbandonmentRequest : ModelEntityBase
    {
        public string WellId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for planning abandonment
    /// </summary>
    public class PlanAbandonmentRequest : ModelEntityBase
    {
        public string InstanceId { get; set; } = string.Empty;
        public Dictionary<string, object>? PlanData { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for obtaining regulatory approval
    /// </summary>
    public class ObtainRegulatoryApprovalRequest : ModelEntityBase
    {
        public string InstanceId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for plugging a well
    /// </summary>
    public class PlugWellRequest : ModelEntityBase
    {
        public string InstanceId { get; set; } = string.Empty;
        public Dictionary<string, object>? PluggingData { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for restoring a site
    /// </summary>
    public class RestoreSiteRequest : ModelEntityBase
    {
        public string InstanceId { get; set; } = string.Empty;
        public Dictionary<string, object>? RestorationData { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for completing abandonment
    /// </summary>
    public class CompleteAbandonmentRequest : ModelEntityBase
    {
        public string InstanceId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for starting facility decommissioning
    /// </summary>
    public class StartFacilityDecommissioningRequest : ModelEntityBase
    {
        public string FacilityId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for planning decommissioning
    /// </summary>
    public class PlanDecommissioningRequest : ModelEntityBase
    {
        public string InstanceId { get; set; } = string.Empty;
        public Dictionary<string, object>? PlanData { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for removing equipment
    /// </summary>
    public class RemoveEquipmentRequest : ModelEntityBase
    {
        public string InstanceId { get; set; } = string.Empty;
        public Dictionary<string, object>? RemovalData { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for cleaning up a site
    /// </summary>
    public class CleanupSiteRequest : ModelEntityBase
    {
        public string InstanceId { get; set; } = string.Empty;
        public Dictionary<string, object>? CleanupData { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for obtaining regulatory closure
    /// </summary>
    public class ObtainRegulatoryClosureRequest : ModelEntityBase
    {
        public string InstanceId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for completing decommissioning
    /// </summary>
    public class CompleteDecommissioningRequest : ModelEntityBase
    {
        public string InstanceId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }
}




