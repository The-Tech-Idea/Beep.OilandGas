using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public class DataQualityDashboardResult : ModelEntityBase
    {
        public Dictionary<string, DataQualityResult> TableQualityResults { get; set; } = new Dictionary<string, DataQualityResult>();
        private double OverallQualityScoreValue;

        public double OverallQualityScore

        {

            get { return this.OverallQualityScoreValue; }

            set { SetProperty(ref OverallQualityScoreValue, value); }

        }
        private int TotalTablesValue;

        public int TotalTables

        {

            get { return this.TotalTablesValue; }

            set { SetProperty(ref TotalTablesValue, value); }

        }
        private int TablesWithIssuesValue;

        public int TablesWithIssues

        {

            get { return this.TablesWithIssuesValue; }

            set { SetProperty(ref TablesWithIssuesValue, value); }

        }
    }
}
