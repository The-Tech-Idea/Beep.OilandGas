using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class ProjectComparison : ModelEntityBase
    {
        /// <summary>
        /// Unique identifier for the comparison
        /// </summary>
        private string ComparisonIdValue;

        public string ComparisonId

        {

            get { return this.ComparisonIdValue; }

            set { SetProperty(ref ComparisonIdValue, value); }

        }

        /// <summary>
        /// Date the comparison was performed
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }

        /// <summary>
        /// Method used for ranking (NPV, IRR, PI, Payback)
        /// </summary>
        private string RankingMethodValue;

        public string RankingMethod

        {

            get { return this.RankingMethodValue; }

            set { SetProperty(ref RankingMethodValue, value); }

        }

        /// <summary>
        /// List of projects with their metrics
        /// </summary>
        private List<ProjectMetrics> ProjectsValue;

        public List<ProjectMetrics> Projects

        {

            get { return this.ProjectsValue; }

            set { SetProperty(ref ProjectsValue, value); }

        }

        /// <summary>
        /// Name of the highest-ranked (recommended) project
        /// </summary>
        private string RecommendedProjectValue;

        public string RecommendedProject

        {

            get { return this.RecommendedProjectValue; }

            set { SetProperty(ref RecommendedProjectValue, value); }

        }
    }
}
