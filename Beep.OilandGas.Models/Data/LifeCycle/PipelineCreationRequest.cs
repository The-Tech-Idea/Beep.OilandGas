using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.LifeCycle
{
    public class PipelineCreationRequest : ModelEntityBase
    {
        private string PipelineNameValue = string.Empty;

        public string PipelineName

        {

            get { return this.PipelineNameValue; }

            set { SetProperty(ref PipelineNameValue, value); }

        }
        private string FieldIdValue = string.Empty;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string? PipelineTypeValue;

        public string? PipelineType

        {

            get { return this.PipelineTypeValue; }

            set { SetProperty(ref PipelineTypeValue, value); }

        }
        private decimal? DiameterValue;

        public decimal? Diameter

        {

            get { return this.DiameterValue; }

            set { SetProperty(ref DiameterValue, value); }

        }
        private decimal? LengthValue;

        public decimal? Length

        {

            get { return this.LengthValue; }

            set { SetProperty(ref LengthValue, value); }

        }
        private string? MaterialValue;

        public string? Material

        {

            get { return this.MaterialValue; }

            set { SetProperty(ref MaterialValue, value); }

        }
        public Dictionary<string, object>? AdditionalProperties { get; set; }
    }
}
