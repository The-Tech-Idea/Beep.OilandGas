using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class DevelopmentCosts : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the development cost identifier.
        /// </summary>
        private string DevelopmentCostIdValue = string.Empty;

        public string DevelopmentCostId

        {

            get { return this.DevelopmentCostIdValue; }

            set { SetProperty(ref DevelopmentCostIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the property identifier.
        /// </summary>
        private string PropertyIdValue = string.Empty;

        public string PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the development well drilling costs (IDC).
        /// </summary>
        private decimal DevelopmentWellDrillingCostsValue;

        public decimal DevelopmentWellDrillingCosts

        {

            get { return this.DevelopmentWellDrillingCostsValue; }

            set { SetProperty(ref DevelopmentWellDrillingCostsValue, value); }

        }

        /// <summary>
        /// Gets or sets the development well equipment costs.
        /// </summary>
        private decimal DevelopmentWellEquipmentValue;

        public decimal DevelopmentWellEquipment

        {

            get { return this.DevelopmentWellEquipmentValue; }

            set { SetProperty(ref DevelopmentWellEquipmentValue, value); }

        }

        /// <summary>
        /// Gets or sets the support equipment and facilities costs.
        /// </summary>
        private decimal SupportEquipmentAndFacilitiesValue;

        public decimal SupportEquipmentAndFacilities

        {

            get { return this.SupportEquipmentAndFacilitiesValue; }

            set { SetProperty(ref SupportEquipmentAndFacilitiesValue, value); }

        }

        /// <summary>
        /// Gets or sets the service well costs.
        /// </summary>
        private decimal ServiceWellCostsValue;

        public decimal ServiceWellCosts

        {

            get { return this.ServiceWellCostsValue; }

            set { SetProperty(ref ServiceWellCostsValue, value); }

        }

        /// <summary>
        /// Gets or sets the cost date.
        /// </summary>
        private DateTime CostDateValue;

        public DateTime CostDate

        {

            get { return this.CostDateValue; }

            set { SetProperty(ref CostDateValue, value); }

        }

        /// <summary>
        /// Gets the total development costs.
        /// </summary>
        public decimal TotalDevelopmentCosts =>
            DevelopmentWellDrillingCosts + DevelopmentWellEquipment +
            SupportEquipmentAndFacilities + ServiceWellCosts;
    }
}
