using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PermitsAndApplications
{
    public class DrillingPermitApplication : PermitApplication
    {
        private string? WellUWIValue;

        public string? WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }
        private string? TargetFormationValue;

        public string? TargetFormation

        {

            get { return this.TargetFormationValue; }

            set { SetProperty(ref TargetFormationValue, value); }

        }
        private decimal ProposedDepthValue;

        public decimal ProposedDepth

        {

            get { return this.ProposedDepthValue; }

            set { SetProperty(ref ProposedDepthValue, value); }

        }
        private string? DrillingMethodValue;

        public string? DrillingMethod

        {

            get { return this.DrillingMethodValue; }

            set { SetProperty(ref DrillingMethodValue, value); }

        }
        private bool SurfaceOwnerNotifiedValue;

        public bool SurfaceOwnerNotified

        {

            get { return this.SurfaceOwnerNotifiedValue; }

            set { SetProperty(ref SurfaceOwnerNotifiedValue, value); }

        }
    }
}
