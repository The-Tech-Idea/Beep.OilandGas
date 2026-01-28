using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.HeatMap;

namespace Beep.OilandGas.Models.Data
{
    public class GenerateProductionHeatMapRequest : ModelEntityBase
    {
        private string FieldIdValue = string.Empty;

        [Required(ErrorMessage = "FieldId is required")]
        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }

        private DateTime StartDateValue;


        [Required(ErrorMessage = "StartDate is required")]
        public DateTime StartDate


        {


            get { return this.StartDateValue; }


            set { SetProperty(ref StartDateValue, value); }


        }

        private DateTime EndDateValue;


        [Required(ErrorMessage = "EndDate is required")]
        public DateTime EndDate


        {


            get { return this.EndDateValue; }


            set { SetProperty(ref EndDateValue, value); }


        }

         private string? ProductionTypeValue;


         public string? ProductionType


         {


             get { return this.ProductionTypeValue; }


             set { SetProperty(ref ProductionTypeValue, value); }


         } // OIL, GAS, WATER
     }
}
