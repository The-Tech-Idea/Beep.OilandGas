using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class OptimizeSystemRequest : ModelEntityBase
    {
        /// <summary>
        /// Well UWI (Unique Well Identifier)
        /// </summary>
        private string WellUWIValue = string.Empty;

        [Required(ErrorMessage = "WellUWI is required")]
        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }

        /// <summary>
        /// Optimization goals
        /// </summary>
        private OptimizationGoals OptimizationGoalsValue = null!;

        [Required(ErrorMessage = "OptimizationGoals are required")]
        public OptimizationGoals OptimizationGoals

        {

            get { return this.OptimizationGoalsValue; }

            set { SetProperty(ref OptimizationGoalsValue, value); }

        }
    }
}
