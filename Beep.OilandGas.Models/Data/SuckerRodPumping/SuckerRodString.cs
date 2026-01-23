using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.SuckerRodPumping
{
    /// <summary>
    /// Represents sucker rod string configuration
    /// DTO for calculations - Entity class: SUCKER_ROD_STRING
    /// </summary>
    public class SuckerRodString : ModelEntityBase
    {
        /// <summary>
        /// Rod sections
        /// </summary>
        private List<RodSection> SectionsValue = new();

        public List<RodSection> Sections

        {

            get { return this.SectionsValue; }

            set { SetProperty(ref SectionsValue, value); }

        }

        /// <summary>
        /// Total length in feet
        /// </summary>
        private decimal TotalLengthValue;

        public decimal TotalLength

        {

            get { return this.TotalLengthValue; }

            set { SetProperty(ref TotalLengthValue, value); }

        }

        /// <summary>
        /// Total weight in pounds
        /// </summary>
        private decimal TotalWeightValue;

        public decimal TotalWeight

        {

            get { return this.TotalWeightValue; }

            set { SetProperty(ref TotalWeightValue, value); }

        }
    }
}






