using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.PlungerLift;
using Beep.OilandGas.Models.Data.SuckerRodPumping;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Pumps
{
    public class SuckerRodPumpDesign : ModelEntityBase
    {
        /// <summary>
        /// Unique design identifier
        /// </summary>
        private string DesignIdValue = string.Empty;

        public string DesignId

        {

            get { return this.DesignIdValue; }

            set { SetProperty(ref DesignIdValue, value); }

        }

        /// <summary>
        /// Well UWI (Unique Well Identifier)
        /// </summary>
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }

        /// <summary>
        /// Design date
        /// </summary>
        private System.DateTime DesignDateValue;

        public System.DateTime DesignDate

        {

            get { return this.DesignDateValue; }

            set { SetProperty(ref DesignDateValue, value); }

        }

        /// <summary>
        /// Pump depth in feet
        /// </summary>
        private decimal PumpDepthValue;

        [Range(0, double.MaxValue)]
        public decimal PumpDepth

        {

            get { return this.PumpDepthValue; }

            set { SetProperty(ref PumpDepthValue, value); }

        }

        /// <summary>
        /// Pump size in inches
        /// </summary>
        private decimal PumpSizeValue;

        [Range(0, double.MaxValue)]
        public decimal PumpSize

        {

            get { return this.PumpSizeValue; }

            set { SetProperty(ref PumpSizeValue, value); }

        }

        /// <summary>
        /// Stroke length in inches
        /// </summary>
        private decimal StrokeLengthValue;

        [Range(0, double.MaxValue)]
        public decimal StrokeLength

        {

            get { return this.StrokeLengthValue; }

            set { SetProperty(ref StrokeLengthValue, value); }

        }

        /// <summary>
        /// Strokes per minute
        /// </summary>
        private decimal StrokesPerMinuteValue;

        [Range(0, double.MaxValue)]
        public decimal StrokesPerMinute

        {

            get { return this.StrokesPerMinuteValue; }

            set { SetProperty(ref StrokesPerMinuteValue, value); }

        }

        /// <summary>
        /// Rod string load in pounds
        /// </summary>
        private decimal RodStringLoadValue;

        [Range(0, double.MaxValue)]
        public decimal RodStringLoad

        {

            get { return this.RodStringLoadValue; }

            set { SetProperty(ref RodStringLoadValue, value); }

        }

        /// <summary>
        /// Pump type designation
        /// </summary>
        private string? PumpTypeValue;

        public string? PumpType

        {

            get { return this.PumpTypeValue; }

            set { SetProperty(ref PumpTypeValue, value); }

        }

        /// <summary>
        /// Rod grade specification
        /// </summary>
        private string? RodGradeValue;

        public string? RodGrade

        {

            get { return this.RodGradeValue; }

            set { SetProperty(ref RodGradeValue, value); }

        }

        /// <summary>
        /// Estimated capacity in bbl/day
        /// </summary>
        private decimal EstimatedCapacityValue;

        [Range(0, double.MaxValue)]
        public decimal EstimatedCapacity

        {

            get { return this.EstimatedCapacityValue; }

            set { SetProperty(ref EstimatedCapacityValue, value); }

        }

        /// <summary>
        /// Power requirement in horsepower
        /// </summary>
        private decimal PowerRequirementValue;

        [Range(0, double.MaxValue)]
        public decimal PowerRequirement

        {

            get { return this.PowerRequirementValue; }

            set { SetProperty(ref PowerRequirementValue, value); }

        }

        /// <summary>
        /// Design status
        /// </summary>
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
    }
}
