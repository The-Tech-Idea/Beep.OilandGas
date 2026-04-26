using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class SalvageValueAnalysis : ModelEntityBase
    {
        private string WellUWIValue = string.Empty;
        public string WellUWI
        {
            get { return this.WellUWIValue; }
            set { SetProperty(ref WellUWIValue, value); }
        }

        private string WellTypeValue = string.Empty;
        public string WellType
        {
            get { return this.WellTypeValue; }
            set { SetProperty(ref WellTypeValue, value); }
        }

        private double WellDepthValue;
        public double WellDepth
        {
            get { return this.WellDepthValue; }
            set { SetProperty(ref WellDepthValue, value); }
        }

        private DateTime AnalysisDateValue = DateTime.UtcNow;
        public DateTime AnalysisDate
        {
            get { return this.AnalysisDateValue; }
            set { SetProperty(ref AnalysisDateValue, value); }
        }

        private double EquipmentSalvageValueValue;
        public double EquipmentSalvageValue
        {
            get { return this.EquipmentSalvageValueValue; }
            set { SetProperty(ref EquipmentSalvageValueValue, value); }
        }

        private double MetalScrapValueValue;
        public double MetalScrapValue
        {
            get { return this.MetalScrapValueValue; }
            set { SetProperty(ref MetalScrapValueValue, value); }
        }

        private double WellheadEquipmentValueValue;
        public double WellheadEquipmentValue
        {
            get { return this.WellheadEquipmentValueValue; }
            set { SetProperty(ref WellheadEquipmentValueValue, value); }
        }

        private double TotalSalvageValueValue;
        public double TotalSalvageValue
        {
            get { return this.TotalSalvageValueValue; }
            set { SetProperty(ref TotalSalvageValueValue, value); }
        }

        private double SalvageValuePercentageOfDecomCostValue;
        public double SalvageValuePercentageOfDecomCost
        {
            get { return this.SalvageValuePercentageOfDecomCostValue; }
            set { SetProperty(ref SalvageValuePercentageOfDecomCostValue, value); }
        }

        private List<string> SalvageableItemsValue = new();
        public List<string> SalvageableItems
        {
            get { return this.SalvageableItemsValue; }
            set { SetProperty(ref SalvageableItemsValue, value); }
        }

        private double TransportationCostValue;
        public double TransportationCost
        {
            get { return this.TransportationCostValue; }
            set { SetProperty(ref TransportationCostValue, value); }
        }

        private double NetSalvageValueValue;
        public double NetSalvageValue
        {
            get { return this.NetSalvageValueValue; }
            set { SetProperty(ref NetSalvageValueValue, value); }
        }
    }
}
