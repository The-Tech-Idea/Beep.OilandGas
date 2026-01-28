using System;
using System.Collections.Generic;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.EconomicAnalysis
{
    public partial class CashFlowAnalysis : ModelEntityBase {
        /// <summary>
        /// Project name or identifier
        /// </summary>
        private string _projectNameValue;
        public string ProjectName
        {
            get { return _projectNameValue; }
            set { SetProperty(ref _projectNameValue, value); }
        }

        /// <summary>
        /// Analysis start year
        /// </summary>
        private int? _startYearValue;
        public int? StartYear
        {
            get { return _startYearValue; }
            set { SetProperty(ref _startYearValue, value); }
        }

        /// <summary>
        /// Analysis end year
        /// </summary>
        private int? _endYearValue;
        public int? EndYear
        {
            get { return _endYearValue; }
            set { SetProperty(ref _endYearValue, value); }
        }

        /// <summary>
        /// Net Present Value at discount rate (dollars)
        /// </summary>
        private decimal? _netPresentValueValue;
        public decimal? NetPresentValue
        {
            get { return _netPresentValueValue; }
            set { SetProperty(ref _netPresentValueValue, value); }
        }

        /// <summary>
        /// Internal Rate of Return (percentage, annual)
        /// </summary>
        private double? _internalRateOfReturnValue;
        public double? InternalRateOfReturn
        {
            get { return _internalRateOfReturnValue; }
            set { SetProperty(ref _internalRateOfReturnValue, value); }
        }

        /// <summary>
        /// Discount rate used for NPV calculation (percentage)
        /// </summary>
        private double? _discountRateValue;
        public double? DiscountRate
        {
            get { return _discountRateValue; }
            set { SetProperty(ref _discountRateValue, value); }
        }

        /// <summary>
        /// Payback period (years) - time to recover initial investment
        /// </summary>
        private double? _paybackPeriodValue;
        public double? PaybackPeriod
        {
            get { return _paybackPeriodValue; }
            set { SetProperty(ref _paybackPeriodValue, value); }
        }

        /// <summary>
        /// Total initial capital investment (dollars)
        /// </summary>
        private decimal? _totalCapitalInvestmentValue;
        public decimal? TotalCapitalInvestment
        {
            get { return _totalCapitalInvestmentValue; }
            set { SetProperty(ref _totalCapitalInvestmentValue, value); }
        }

        /// <summary>
        /// Total cumulative cash flow over project life (dollars)
        /// </summary>
        private decimal? _totalCumulativeCashFlowValue;
        public decimal? TotalCumulativeCashFlow
        {
            get { return _totalCumulativeCashFlowValue; }
            set { SetProperty(ref _totalCumulativeCashFlowValue, value); }
        }

        /// <summary>
        /// Profitability Index (NPV / Capital Investment) - higher is better
        /// </summary>
        private double? _profitabilityIndexValue;
        public double? ProfitabilityIndex
        {
            get { return _profitabilityIndexValue; }
            set { SetProperty(ref _profitabilityIndexValue, value); }
        }

        /// <summary>
        /// Return on Investment percentage
        /// </summary>
        private double? _roiPercentValue;
        public double? ROIPercent
        {
            get { return _roiPercentValue; }
            set { SetProperty(ref _roiPercentValue, value); }
        }

        /// <summary>
        /// Break-even oil price (dollars per barrel)
        /// </summary>
        private double? _breakEvenOilPriceValue;
        public double? BreakEvenOilPrice
        {
            get { return _breakEvenOilPriceValue; }
            set { SetProperty(ref _breakEvenOilPriceValue, value); }
        }

        /// <summary>
        /// Break-even gas price (dollars per MMBTU)
        /// </summary>
        private double? _breakEvenGasPriceValue;
        public double? BreakEvenGasPrice
        {
            get { return _breakEvenGasPriceValue; }
            set { SetProperty(ref _breakEvenGasPriceValue, value); }
        }

        /// <summary>
        /// Peak net annual cash flow (dollars)
        /// </summary>
        private decimal? _peakAnnualCashFlowValue;
        public decimal? PeakAnnualCashFlow
        {
            get { return _peakAnnualCashFlowValue; }
            set { SetProperty(ref _peakAnnualCashFlowValue, value); }
        }

        /// <summary>
        /// Year of peak cash flow
        /// </summary>
        private int? _peakCashFlowYearValue;
        public int? PeakCashFlowYear
        {
            get { return _peakCashFlowYearValue; }
            set { SetProperty(ref _peakCashFlowYearValue, value); }
        }

        /// <summary>
        /// Economic status (Viable, Marginal, Not Viable)
        /// </summary>
        private string _economicStatusValue;
        public string EconomicStatus
        {
            get { return _economicStatusValue; }
            set { SetProperty(ref _economicStatusValue, value); }
        }

        /// <summary>
        /// Risk rating (Low, Medium, High)
        /// </summary>
        private string _riskRatingValue;
        public string RiskRating
        {
            get { return _riskRatingValue; }
            set { SetProperty(ref _riskRatingValue, value); }
        }

        /// <summary>
        /// Analysis notes and assumptions
        /// </summary>
        private string _notesValue;
        public string Notes
        {
            get { return _notesValue; }
            set { SetProperty(ref _notesValue, value); }
        }



        /// <summary>
        /// Default constructor
        /// </summary>
        public CashFlowAnalysis()
        {
            PPDM_GUID = Guid.NewGuid().ToString();
        }
    }
}
