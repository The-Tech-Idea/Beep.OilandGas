using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.LifeCycle
{
    /// <summary>
    /// Request DTO for starting well abandonment process
    /// </summary>
    public class StartWellAbandonmentRequest : ModelEntityBase
    {
        private string WellIdValue = string.Empty;

        public string WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string UserIdValue = string.Empty;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    /// <summary>
    /// Request DTO for planning abandonment
    /// </summary>
    public class PlanAbandonmentRequest : ModelEntityBase
    {
        private string InstanceIdValue = string.Empty;

        public string InstanceId

        {

            get { return this.InstanceIdValue; }

            set { SetProperty(ref InstanceIdValue, value); }

        }
        public Dictionary<string, object>? PlanData { get; set; }
        private string UserIdValue = string.Empty;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    /// <summary>
    /// Request DTO for obtaining regulatory approval
    /// </summary>
    public class ObtainRegulatoryApprovalRequest : ModelEntityBase
    {
        private string InstanceIdValue = string.Empty;

        public string InstanceId

        {

            get { return this.InstanceIdValue; }

            set { SetProperty(ref InstanceIdValue, value); }

        }
        private string UserIdValue = string.Empty;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    /// <summary>
    /// Request DTO for plugging a well
    /// </summary>
    public class PlugWellRequest : ModelEntityBase
    {
        private string InstanceIdValue = string.Empty;

        public string InstanceId

        {

            get { return this.InstanceIdValue; }

            set { SetProperty(ref InstanceIdValue, value); }

        }
        public Dictionary<string, object>? PluggingData { get; set; }
        private string UserIdValue = string.Empty;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    /// <summary>
    /// Request DTO for restoring a site
    /// </summary>
    public class RestoreSiteRequest : ModelEntityBase
    {
        private string InstanceIdValue = string.Empty;

        public string InstanceId

        {

            get { return this.InstanceIdValue; }

            set { SetProperty(ref InstanceIdValue, value); }

        }
        public Dictionary<string, object>? RestorationData { get; set; }
        private string UserIdValue = string.Empty;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    /// <summary>
    /// Request DTO for completing abandonment
    /// </summary>
    public class CompleteAbandonmentRequest : ModelEntityBase
    {
        private string InstanceIdValue = string.Empty;

        public string InstanceId

        {

            get { return this.InstanceIdValue; }

            set { SetProperty(ref InstanceIdValue, value); }

        }
        private string UserIdValue = string.Empty;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    /// <summary>
    /// Request DTO for starting facility decommissioning
    /// </summary>
    public class StartFacilityDecommissioningRequest : ModelEntityBase
    {
        private string FacilityIdValue = string.Empty;

        public string FacilityId

        {

            get { return this.FacilityIdValue; }

            set { SetProperty(ref FacilityIdValue, value); }

        }
        private string UserIdValue = string.Empty;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    /// <summary>
    /// Request DTO for planning decommissioning
    /// </summary>
    public class PlanDecommissioningRequest : ModelEntityBase
    {
        private string InstanceIdValue = string.Empty;

        public string InstanceId

        {

            get { return this.InstanceIdValue; }

            set { SetProperty(ref InstanceIdValue, value); }

        }
        public Dictionary<string, object>? PlanData { get; set; }
        private string UserIdValue = string.Empty;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    /// <summary>
    /// Request DTO for removing equipment
    /// </summary>
    public class RemoveEquipmentRequest : ModelEntityBase
    {
        private string InstanceIdValue = string.Empty;

        public string InstanceId

        {

            get { return this.InstanceIdValue; }

            set { SetProperty(ref InstanceIdValue, value); }

        }
        public Dictionary<string, object>? RemovalData { get; set; }
        private string UserIdValue = string.Empty;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    /// <summary>
    /// Request DTO for cleaning up a site
    /// </summary>
    public class CleanupSiteRequest : ModelEntityBase
    {
        private string InstanceIdValue = string.Empty;

        public string InstanceId

        {

            get { return this.InstanceIdValue; }

            set { SetProperty(ref InstanceIdValue, value); }

        }
        public Dictionary<string, object>? CleanupData { get; set; }
        private string UserIdValue = string.Empty;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    /// <summary>
    /// Request DTO for obtaining regulatory closure
    /// </summary>
    public class ObtainRegulatoryClosureRequest : ModelEntityBase
    {
        private string InstanceIdValue = string.Empty;

        public string InstanceId

        {

            get { return this.InstanceIdValue; }

            set { SetProperty(ref InstanceIdValue, value); }

        }
        private string UserIdValue = string.Empty;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    /// <summary>
    /// Request DTO for completing decommissioning
    /// </summary>
    public class CompleteDecommissioningRequest : ModelEntityBase
    {
        private string InstanceIdValue = string.Empty;

        public string InstanceId

        {

            get { return this.InstanceIdValue; }

            set { SetProperty(ref InstanceIdValue, value); }

        }
        private string UserIdValue = string.Empty;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }
}






