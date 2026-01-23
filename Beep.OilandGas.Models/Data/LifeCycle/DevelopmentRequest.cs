using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.LifeCycle
{
    /// <summary>
    /// Request DTO for starting pool definition process
    /// </summary>
    public class StartPoolDefinitionRequest : ModelEntityBase
    {
        private string PoolIdValue = string.Empty;

        public string PoolId

        {

            get { return this.PoolIdValue; }

            set { SetProperty(ref PoolIdValue, value); }

        }
        private string UserIdValue = string.Empty;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    /// <summary>
    /// Request DTO for delineating a pool
    /// </summary>
    public class DelineatePoolRequest : ModelEntityBase
    {
        private string InstanceIdValue = string.Empty;

        public string InstanceId

        {

            get { return this.InstanceIdValue; }

            set { SetProperty(ref InstanceIdValue, value); }

        }
        public Dictionary<string, object>? DelineationData { get; set; }
        private string UserIdValue = string.Empty;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    /// <summary>
    /// Request DTO for assigning reserves
    /// </summary>
    public class AssignReservesRequest : ModelEntityBase
    {
        private string InstanceIdValue = string.Empty;

        public string InstanceId

        {

            get { return this.InstanceIdValue; }

            set { SetProperty(ref InstanceIdValue, value); }

        }
        public Dictionary<string, object>? ReserveData { get; set; }
        private string UserIdValue = string.Empty;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    /// <summary>
    /// Request DTO for approving a pool
    /// </summary>
    public class ApprovePoolRequest : ModelEntityBase
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
    /// Request DTO for activating a pool
    /// </summary>
    public class ActivatePoolRequest : ModelEntityBase
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
    /// Request DTO for starting facility development
    /// </summary>
    public class StartFacilityDevelopmentRequest : ModelEntityBase
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
    /// Request DTO for starting well development
    /// </summary>
    public class StartWellDevelopmentRequest : ModelEntityBase
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
    /// Request DTO for starting pipeline development
    /// </summary>
    public class StartPipelineDevelopmentRequest : ModelEntityBase
    {
        private string PipelineIdValue = string.Empty;

        public string PipelineId

        {

            get { return this.PipelineIdValue; }

            set { SetProperty(ref PipelineIdValue, value); }

        }
        private string UserIdValue = string.Empty;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }
}






