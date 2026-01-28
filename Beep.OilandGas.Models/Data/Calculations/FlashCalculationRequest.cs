using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.FlashCalculations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class FlashCalculationRequest : ModelEntityBase
    {
        /// <summary>
        /// Flash conditions
        /// </summary>
        private FLASH_CONDITIONS ConditionsValue = null!;

        [Required(ErrorMessage = "Conditions are required")]
        public FLASH_CONDITIONS Conditions

        {

            get { return this.ConditionsValue; }

            set { SetProperty(ref ConditionsValue, value); }

        }

        /// <summary>
        /// Number of flash stages
        /// </summary>
        private int StagesValue;

        [Required]
        [Range(1, 100, ErrorMessage = "Stages must be between 1 and 100")]
        public int Stages

        {

            get { return this.StagesValue; }

            set { SetProperty(ref StagesValue, value); }

        }

        public string? WellId { get; set; }
        public string? FacilityId { get; set; }
        public List<Component>? FeedComposition { get; set; }
        public decimal? Pressure { get; set; }
        public decimal? Temperature { get; set; }
        public FlashCalculationOptions? AdditionalParameters { get; set; }
        public string CalculationType { get; set; }
        public string UserId { get; set; }
    }
}
