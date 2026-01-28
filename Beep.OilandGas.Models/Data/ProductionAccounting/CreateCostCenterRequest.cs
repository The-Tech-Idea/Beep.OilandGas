using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class CreateCostCenterRequest : ModelEntityBase
    {
        private string CostCenterIdValue = string.Empty;

        public string CostCenterId

        {

            get { return this.CostCenterIdValue; }

            set { SetProperty(ref CostCenterIdValue, value); }

        }
        private string CostCenterNameValue = string.Empty;

        public string CostCenterName

        {

            get { return this.CostCenterNameValue; }

            set { SetProperty(ref CostCenterNameValue, value); }

        }
        private string CostCenterTypeValue = string.Empty;

        public string CostCenterType

        {

            get { return this.CostCenterTypeValue; }

            set { SetProperty(ref CostCenterTypeValue, value); }

        } // Country, Region, Field
        private string DescriptionValue = string.Empty;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }
}
