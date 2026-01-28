using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class FieldLifecycleSummary : ModelEntityBase
    {
        private string FieldIdValue = string.Empty;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string FieldNameValue = string.Empty;

        public string FieldName

        {

            get { return this.FieldNameValue; }

            set { SetProperty(ref FieldNameValue, value); }

        }
        
        // Exploration phase summary
        private int ProspectCountValue;

        public int ProspectCount

        {

            get { return this.ProspectCountValue; }

            set { SetProperty(ref ProspectCountValue, value); }

        }
        private int SeismicSurveyCountValue;

        public int SeismicSurveyCount

        {

            get { return this.SeismicSurveyCountValue; }

            set { SetProperty(ref SeismicSurveyCountValue, value); }

        }
        private int ExploratoryWellCountValue;

        public int ExploratoryWellCount

        {

            get { return this.ExploratoryWellCountValue; }

            set { SetProperty(ref ExploratoryWellCountValue, value); }

        }
        
        // Development phase summary
        private int PoolCountValue;

        public int PoolCount

        {

            get { return this.PoolCountValue; }

            set { SetProperty(ref PoolCountValue, value); }

        }
        private int DevelopmentWellCountValue;

        public int DevelopmentWellCount

        {

            get { return this.DevelopmentWellCountValue; }

            set { SetProperty(ref DevelopmentWellCountValue, value); }

        }
        private int FacilityCountValue;

        public int FacilityCount

        {

            get { return this.FacilityCountValue; }

            set { SetProperty(ref FacilityCountValue, value); }

        }
        private int PipelineCountValue;

        public int PipelineCount

        {

            get { return this.PipelineCountValue; }

            set { SetProperty(ref PipelineCountValue, value); }

        }
        
        // Production phase summary
        private int ProductionWellCountValue;

        public int ProductionWellCount

        {

            get { return this.ProductionWellCountValue; }

            set { SetProperty(ref ProductionWellCountValue, value); }

        }
        private decimal? TotalProductionVolumeValue;

        public decimal? TotalProductionVolume

        {

            get { return this.TotalProductionVolumeValue; }

            set { SetProperty(ref TotalProductionVolumeValue, value); }

        }
        private DateTime? LastProductionDateValue;

        public DateTime? LastProductionDate

        {

            get { return this.LastProductionDateValue; }

            set { SetProperty(ref LastProductionDateValue, value); }

        }
        
        // Decommissioning phase summary
        private int AbandonedWellCountValue;

        public int AbandonedWellCount

        {

            get { return this.AbandonedWellCountValue; }

            set { SetProperty(ref AbandonedWellCountValue, value); }

        }
        private int DecommissionedFacilityCountValue;

        public int DecommissionedFacilityCount

        {

            get { return this.DecommissionedFacilityCountValue; }

            set { SetProperty(ref DecommissionedFacilityCountValue, value); }

        }
        
        // Overall status
        private string? CurrentLifecyclePhaseValue;

        public string? CurrentLifecyclePhase

        {

            get { return this.CurrentLifecyclePhaseValue; }

            set { SetProperty(ref CurrentLifecyclePhaseValue, value); }

        }
        private DateTime? FieldDiscoveryDateValue;

        public DateTime? FieldDiscoveryDate

        {

            get { return this.FieldDiscoveryDateValue; }

            set { SetProperty(ref FieldDiscoveryDateValue, value); }

        }
        private DateTime? FieldStartProductionDateValue;

        public DateTime? FieldStartProductionDate

        {

            get { return this.FieldStartProductionDateValue; }

            set { SetProperty(ref FieldStartProductionDateValue, value); }

        }
    }
}
