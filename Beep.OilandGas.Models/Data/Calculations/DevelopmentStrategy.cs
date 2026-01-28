using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class DevelopmentStrategy : ModelEntityBase
    {
        private string StrategyNameValue;

        public string StrategyName

        {

            get { return this.StrategyNameValue; }

            set { SetProperty(ref StrategyNameValue, value); }

        }
        private string DescriptionValue;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private double EstimatedCapexValue;

        public double EstimatedCapex

        {

            get { return this.EstimatedCapexValue; }

            set { SetProperty(ref EstimatedCapexValue, value); }

        }
        private double ProjectNPVValue;

        public double ProjectNPV

        {

            get { return this.ProjectNPVValue; }

            set { SetProperty(ref ProjectNPVValue, value); }

        }
        private double ProjectIRRValue;

        public double ProjectIRR

        {

            get { return this.ProjectIRRValue; }

            set { SetProperty(ref ProjectIRRValue, value); }

        }
        private double RiskLevelValue;

        public double RiskLevel

        {

            get { return this.RiskLevelValue; }

            set { SetProperty(ref RiskLevelValue, value); }

        }
        private int ProjectDurationValue;

        public int ProjectDuration

        {

            get { return this.ProjectDurationValue; }

            set { SetProperty(ref ProjectDurationValue, value); }

        }
        private double EstimatedEmissionsValue;

        public double EstimatedEmissions

        {

            get { return this.EstimatedEmissionsValue; }

            set { SetProperty(ref EstimatedEmissionsValue, value); }

        }
        private List<string> KeyBenefitsValue;

        public List<string> KeyBenefits

        {

            get { return this.KeyBenefitsValue; }

            set { SetProperty(ref KeyBenefitsValue, value); }

        }
        private List<string> KeyChallengesValue;

        public List<string> KeyChallenges

        {

            get { return this.KeyChallengesValue; }

            set { SetProperty(ref KeyChallengesValue, value); }

        }
    }
}
