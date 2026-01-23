using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.LifeCycle
{
    /// <summary>
    /// Request DTO for starting lead to prospect process
    /// </summary>
    public class StartLeadToProspectRequest : ModelEntityBase
    {
        private string LeadIdValue = string.Empty;

        public string LeadId

        {

            get { return this.LeadIdValue; }

            set { SetProperty(ref LeadIdValue, value); }

        }
        private string UserIdValue = string.Empty;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    /// <summary>
    /// Request DTO for evaluating a lead
    /// </summary>
    public class EvaluateLeadRequest : ModelEntityBase
    {
        private string InstanceIdValue = string.Empty;

        public string InstanceId

        {

            get { return this.InstanceIdValue; }

            set { SetProperty(ref InstanceIdValue, value); }

        }
        public Dictionary<string, object>? EvaluationData { get; set; }
        private string UserIdValue = string.Empty;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    /// <summary>
    /// Request DTO for approving a lead
    /// </summary>
    public class ApproveLeadRequest : ModelEntityBase
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
    /// Request DTO for starting prospect to discovery process
    /// </summary>
    public class StartProspectToDiscoveryRequest : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private string UserIdValue = string.Empty;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    /// <summary>
    /// Request DTO for starting discovery to development process
    /// </summary>
    public class StartDiscoveryToDevelopmentRequest : ModelEntityBase
    {
        private string DiscoveryIdValue = string.Empty;

        public string DiscoveryId

        {

            get { return this.DiscoveryIdValue; }

            set { SetProperty(ref DiscoveryIdValue, value); }

        }
        private string UserIdValue = string.Empty;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }
}






