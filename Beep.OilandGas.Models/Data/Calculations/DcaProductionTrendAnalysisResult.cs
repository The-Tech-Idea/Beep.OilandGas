using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class DcaProductionTrendAnalysisResult : ModelEntityBase
    {
        /// <summary>
        /// Number of data points analyzed
        /// </summary>
        private int DataPointsAnalyzedValue;

        public int DataPointsAnalyzed

        {

            get { return this.DataPointsAnalyzedValue; }

            set { SetProperty(ref DataPointsAnalyzedValue, value); }

        }

        /// <summary>
        /// Date analysis was performed
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }

        /// <summary>
        /// Decline rates by interval
        /// </summary>
        private List<double> DeclineRatesByIntervalValue = new();

        public List<double> DeclineRatesByInterval

        {

            get { return this.DeclineRatesByIntervalValue; }

            set { SetProperty(ref DeclineRatesByIntervalValue, value); }

        }

        /// <summary>
        /// Identified inflection points
        /// </summary>
        private List<DcaInflectionPoint> InflectionPointsValue = new();

        public List<DcaInflectionPoint> InflectionPoints

        {

            get { return this.InflectionPointsValue; }

            set { SetProperty(ref InflectionPointsValue, value); }

        }

        /// <summary>
        /// Detected phase transitions
        /// </summary>
        private List<DcaPhaseTransition> PhaseTransitionsValue = new();

        public List<DcaPhaseTransition> PhaseTransitions

        {

            get { return this.PhaseTransitionsValue; }

            set { SetProperty(ref PhaseTransitionsValue, value); }

        }

        /// <summary>
        /// Average decline rate in early phase
        /// </summary>
        private double EarlyPhaseDeclineValue;

        public double EarlyPhaseDecline

        {

            get { return this.EarlyPhaseDeclineValue; }

            set { SetProperty(ref EarlyPhaseDeclineValue, value); }

        }

        /// <summary>
        /// Average decline rate in main phase
        /// </summary>
        private double MainPhaseDeclineValue;

        public double MainPhaseDecline

        {

            get { return this.MainPhaseDeclineValue; }

            set { SetProperty(ref MainPhaseDeclineValue, value); }

        }
    }
}
