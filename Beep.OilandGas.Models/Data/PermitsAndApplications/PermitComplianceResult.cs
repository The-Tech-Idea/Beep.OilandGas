using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PermitsAndApplications
{
    /// <summary>
    /// Compliance result for permit applications.
    /// </summary>
    public class PermitComplianceResult : ModelEntityBase
    {
        private bool IsCompliantValue;

        public bool IsCompliant

        {

            get { return this.IsCompliantValue; }

            set { SetProperty(ref IsCompliantValue, value); }

        }

        private List<string> ViolationsValue = new();

        public List<string> Violations

        {

            get { return this.ViolationsValue; }

            set { SetProperty(ref ViolationsValue, value); }

        }

        private List<string> WarningsValue = new();

        public List<string> Warnings

        {

            get { return this.WarningsValue; }

            set { SetProperty(ref WarningsValue, value); }

        }

        private decimal ComplianceScoreValue;

        public decimal ComplianceScore

        {

            get { return this.ComplianceScoreValue; }

            set { SetProperty(ref ComplianceScoreValue, value); }

        }
    }
}
