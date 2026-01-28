using Beep.OilandGas.Models.Data.Drilling;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class CompletionString : ModelEntityBase
    {
        private string CompletionIdValue = string.Empty;

        public string CompletionId

        {

            get { return this.CompletionIdValue; }

            set { SetProperty(ref CompletionIdValue, value); }

        }
        private string ConstructionIdValue = string.Empty;

        public string ConstructionId

        {

            get { return this.ConstructionIdValue; }

            set { SetProperty(ref ConstructionIdValue, value); }

        }
        private string CompletionTypeValue = string.Empty;

        public string CompletionType

        {

            get { return this.CompletionTypeValue; }

            set { SetProperty(ref CompletionTypeValue, value); }

        }
        private decimal? TopDepthValue;

        public decimal? TopDepth

        {

            get { return this.TopDepthValue; }

            set { SetProperty(ref TopDepthValue, value); }

        }
        private decimal? BottomDepthValue;

        public decimal? BottomDepth

        {

            get { return this.BottomDepthValue; }

            set { SetProperty(ref BottomDepthValue, value); }

        }
        private decimal? DiameterValue;

        public decimal? Diameter

        {

            get { return this.DiameterValue; }

            set { SetProperty(ref DiameterValue, value); }

        }
        private string? DiameterUnitValue;

        public string? DiameterUnit

        {

            get { return this.DiameterUnitValue; }

            set { SetProperty(ref DiameterUnitValue, value); }

        }
        private List<PERFORATION> PerforationsValue = new();

        public List<PERFORATION> Perforations

        {

            get { return this.PerforationsValue; }

            set { SetProperty(ref PerforationsValue, value); }

        }
    }
}
