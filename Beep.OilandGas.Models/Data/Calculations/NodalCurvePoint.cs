using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class NodalCurvePoint : ModelEntityBase
    {
        private decimal RateValue;

        public decimal Rate

        {

            get { return this.RateValue; }

            set { SetProperty(ref RateValue, value); }

        }
        private decimal PressureValue;

        public decimal Pressure

        {

            get { return this.PressureValue; }

            set { SetProperty(ref PressureValue, value); }

        }
        private string CurveTypeValue;

        public string CurveType

        {

            get { return this.CurveTypeValue; }

            set { SetProperty(ref CurveTypeValue, value); }

        } // IPR or VLP

        public decimal FlowRate { get; set; }
    }
}
