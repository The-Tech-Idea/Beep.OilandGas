using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Operations
{
    /// <summary>
    /// Request DTO for ranking prospects
    /// </summary>
    public class RankProspectsRequest : ModelEntityBase
    {
        private List<string> ProspectIdsValue = new();

        public List<string> ProspectIds

        {

            get { return this.ProspectIdsValue; }

            set { SetProperty(ref ProspectIdsValue, value); }

        }
        public Dictionary<string, decimal> RankingCriteria { get; set; } = new();
    }

    /// <summary>
    /// Request DTO for updating lease status
    /// </summary>
    public class UpdateLeaseStatusRequest : ModelEntityBase
    {
        private string StatusValue = string.Empty;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
    }

    /// <summary>
    /// Request DTO for analyzing EOR potential
    /// </summary>
    public class AnalyzeEORRequest : ModelEntityBase
    {
        private string FieldIdValue = string.Empty;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string EorMethodValue = string.Empty;

        public string EorMethod

        {

            get { return this.EorMethodValue; }

            set { SetProperty(ref EorMethodValue, value); }

        }
    }

    /// <summary>
    /// Request DTO for calculating recovery factor
    /// </summary>
    public class CalculateRecoveryFactorRequest : ModelEntityBase
    {
        private string ProjectIdValue = string.Empty;

        public string ProjectId

        {

            get { return this.ProjectIdValue; }

            set { SetProperty(ref ProjectIdValue, value); }

        }
    }

    /// <summary>
    /// Request DTO for managing injection operations
    /// </summary>
    public class ManageInjectionRequest : ModelEntityBase
    {
        private string InjectionWellIdValue = string.Empty;

        public string InjectionWellId

        {

            get { return this.InjectionWellIdValue; }

            set { SetProperty(ref InjectionWellIdValue, value); }

        }
        private decimal InjectionRateValue;

        public decimal InjectionRate

        {

            get { return this.InjectionRateValue; }

            set { SetProperty(ref InjectionRateValue, value); }

        }
    }
}







