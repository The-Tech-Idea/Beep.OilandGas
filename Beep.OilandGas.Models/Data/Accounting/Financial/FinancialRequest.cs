using System;
using Beep.OilandGas.Models.Data.ProductionAccounting;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Accounting.Financial
{
    /// <summary>
    /// Request DTO for impairment
    /// </summary>
    public class ImpairmentRequest : ModelEntityBase
    {
        private string PropertyIdValue = string.Empty;

        public string PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
        private decimal ImpairmentAmountValue;

        public decimal ImpairmentAmount

        {

            get { return this.ImpairmentAmountValue; }

            set { SetProperty(ref ImpairmentAmountValue, value); }

        }
    }

    /// <summary>
    /// Request DTO for full cost exploration
    /// </summary>
    public class FullCostExplorationRequest : ModelEntityBase
    {
        private string CostCenterIdValue = string.Empty;

        public string CostCenterId

        {

            get { return this.CostCenterIdValue; }

            set { SetProperty(ref CostCenterIdValue, value); }

        }
        private ExplorationCosts CostsValue = new();

        public ExplorationCosts Costs

        {

            get { return this.CostsValue; }

            set { SetProperty(ref CostsValue, value); }

        }
    }

    /// <summary>
    /// Request DTO for full cost development
    /// </summary>
    public class FullCostDevelopmentRequest : ModelEntityBase
    {
        private string CostCenterIdValue = string.Empty;

        public string CostCenterId

        {

            get { return this.CostCenterIdValue; }

            set { SetProperty(ref CostCenterIdValue, value); }

        }
        private DevelopmentCosts CostsValue = new();

        public DevelopmentCosts Costs

        {

            get { return this.CostsValue; }

            set { SetProperty(ref CostsValue, value); }

        }
    }

    /// <summary>
    /// Request DTO for full cost acquisition
    /// </summary>
    public class FullCostAcquisitionRequest : ModelEntityBase
    {
        private string CostCenterIdValue = string.Empty;

        public string CostCenterId

        {

            get { return this.CostCenterIdValue; }

            set { SetProperty(ref CostCenterIdValue, value); }

        }
        private UnprovedProperty PropertyValue = new();

        public UnprovedProperty Property

        {

            get { return this.PropertyValue; }

            set { SetProperty(ref PropertyValue, value); }

        }
    }

    /// <summary>
    /// Request DTO for ceiling test
    /// </summary>
    public class CeilingTestRequest : ModelEntityBase
    {
        private string CostCenterIdValue = string.Empty;

        public string CostCenterId

        {

            get { return this.CostCenterIdValue; }

            set { SetProperty(ref CostCenterIdValue, value); }

        }
        private ProvedReserves ReservesValue = new();

        public ProvedReserves Reserves

        {

            get { return this.ReservesValue; }

            set { SetProperty(ref ReservesValue, value); }

        }
        private ProductionData ProductionValue = new();

        public ProductionData Production

        {

            get { return this.ProductionValue; }

            set { SetProperty(ref ProductionValue, value); }

        }
    }
}







