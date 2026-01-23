using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
    public class RiskFactor : ModelEntityBase
    {
        private string RiskFactorIdValue = string.Empty;

        public string RiskFactorId

        {

            get { return this.RiskFactorIdValue; }

            set { SetProperty(ref RiskFactorIdValue, value); }

        }
        private string CategoryValue = string.Empty;

        public string Category

        {

            get { return this.CategoryValue; }

            set { SetProperty(ref CategoryValue, value); }

        }
        private string DescriptionValue = string.Empty;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private decimal? RiskScoreValue;

        public decimal? RiskScore

        {

            get { return this.RiskScoreValue; }

            set { SetProperty(ref RiskScoreValue, value); }

        }
        private string? MitigationValue;

        public string? Mitigation

        {

            get { return this.MitigationValue; }

            set { SetProperty(ref MitigationValue, value); }

        }
    }
}
