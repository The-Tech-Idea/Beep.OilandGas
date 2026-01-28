using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class FieldStatistics : ModelEntityBase
    {
        private string FieldIdValue = string.Empty;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string FieldNameValue = string.Empty;

        public string FieldName

        {

            get { return this.FieldNameValue; }

            set { SetProperty(ref FieldNameValue, value); }

        }
        
        // Well statistics
        private int TotalWellCountValue;

        public int TotalWellCount

        {

            get { return this.TotalWellCountValue; }

            set { SetProperty(ref TotalWellCountValue, value); }

        }
        private int ActiveWellCountValue;

        public int ActiveWellCount

        {

            get { return this.ActiveWellCountValue; }

            set { SetProperty(ref ActiveWellCountValue, value); }

        }
        private int InactiveWellCountValue;

        public int InactiveWellCount

        {

            get { return this.InactiveWellCountValue; }

            set { SetProperty(ref InactiveWellCountValue, value); }

        }
        
        // Production statistics
        private decimal? TotalOilProductionValue;

        public decimal? TotalOilProduction

        {

            get { return this.TotalOilProductionValue; }

            set { SetProperty(ref TotalOilProductionValue, value); }

        }
        private decimal? TotalGasProductionValue;

        public decimal? TotalGasProduction

        {

            get { return this.TotalGasProductionValue; }

            set { SetProperty(ref TotalGasProductionValue, value); }

        }
        private decimal? TotalWaterProductionValue;

        public decimal? TotalWaterProduction

        {

            get { return this.TotalWaterProductionValue; }

            set { SetProperty(ref TotalWaterProductionValue, value); }

        }
        private decimal? AverageDailyProductionValue;

        public decimal? AverageDailyProduction

        {

            get { return this.AverageDailyProductionValue; }

            set { SetProperty(ref AverageDailyProductionValue, value); }

        }
        
        // Reserves statistics
        private decimal? ProvedReservesValue;

        public decimal? ProvedReserves

        {

            get { return this.ProvedReservesValue; }

            set { SetProperty(ref ProvedReservesValue, value); }

        }
        private decimal? ProbableReservesValue;

        public decimal? ProbableReserves

        {

            get { return this.ProbableReservesValue; }

            set { SetProperty(ref ProbableReservesValue, value); }

        }
        private decimal? PossibleReservesValue;

        public decimal? PossibleReserves

        {

            get { return this.PossibleReservesValue; }

            set { SetProperty(ref PossibleReservesValue, value); }

        }
        
        // Facility statistics
        private int TotalFacilityCountValue;

        public int TotalFacilityCount

        {

            get { return this.TotalFacilityCountValue; }

            set { SetProperty(ref TotalFacilityCountValue, value); }

        }
        private int ActiveFacilityCountValue;

        public int ActiveFacilityCount

        {

            get { return this.ActiveFacilityCountValue; }

            set { SetProperty(ref ActiveFacilityCountValue, value); }

        }
        
        // Financial statistics (if available)
        private decimal? TotalInvestmentValue;

        public decimal? TotalInvestment

        {

            get { return this.TotalInvestmentValue; }

            set { SetProperty(ref TotalInvestmentValue, value); }

        }
        private decimal? TotalRevenueValue;

        public decimal? TotalRevenue

        {

            get { return this.TotalRevenueValue; }

            set { SetProperty(ref TotalRevenueValue, value); }

        }
        private decimal? NetPresentValueValue;

        public decimal? NetPresentValue

        {

            get { return this.NetPresentValueValue; }

            set { SetProperty(ref NetPresentValueValue, value); }

        }
        
        // Date ranges
        private DateTime? FirstProductionDateValue;

        public DateTime? FirstProductionDate

        {

            get { return this.FirstProductionDateValue; }

            set { SetProperty(ref FirstProductionDateValue, value); }

        }
        private DateTime? LastProductionDateValue;

        public DateTime? LastProductionDate

        {

            get { return this.LastProductionDateValue; }

            set { SetProperty(ref LastProductionDateValue, value); }

        }
        private DateTime? StatisticsAsOfDateValue = DateTime.UtcNow;

        public DateTime? StatisticsAsOfDate

        {

            get { return this.StatisticsAsOfDateValue; }

            set { SetProperty(ref StatisticsAsOfDateValue, value); }

        }
    }
}
