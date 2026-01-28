using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class TrimOption : ModelEntityBase
    {
        private string TrimSizeValue = string.Empty;

        public string TrimSize

        {

            get { return this.TrimSizeValue; }

            set { SetProperty(ref TrimSizeValue, value); }

        } // AX, BX, CX, DX
        private decimal ChokeDiameterValue;

        public decimal ChokeDiameter

        {

            get { return this.ChokeDiameterValue; }

            set { SetProperty(ref ChokeDiameterValue, value); }

        }
        private decimal FlowCapacityValue;

        public decimal FlowCapacity

        {

            get { return this.FlowCapacityValue; }

            set { SetProperty(ref FlowCapacityValue, value); }

        }
        private string MaterialValue = string.Empty;

        public string Material

        {

            get { return this.MaterialValue; }

            set { SetProperty(ref MaterialValue, value); }

        }
        private decimal EstimatedLifeValue;

        public decimal EstimatedLife

        {

            get { return this.EstimatedLifeValue; }

            set { SetProperty(ref EstimatedLifeValue, value); }

        }
        private decimal CostValue;

        public decimal Cost

        {

            get { return this.CostValue; }

            set { SetProperty(ref CostValue, value); }

        }
        private decimal ErosionRatingValue;

        public decimal ErosionRating

        {

            get { return this.ErosionRatingValue; }

            set { SetProperty(ref ErosionRatingValue, value); }

        } // 0-100, 100=Best
        private string ProsValue = string.Empty;

        public string Pros

        {

            get { return this.ProsValue; }

            set { SetProperty(ref ProsValue, value); }

        }
        private string ConsValue = string.Empty;

        public string Cons

        {

            get { return this.ConsValue; }

            set { SetProperty(ref ConsValue, value); }

        }
    }
}
