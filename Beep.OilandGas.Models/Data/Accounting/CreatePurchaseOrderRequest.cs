
namespace Beep.OilandGas.Models.Data.Accounting
{
    public class CreatePurchaseOrderRequest : ModelEntityBase
    {
        private string PoNumberValue;

        public string PoNumber

        {

            get { return this.PoNumberValue; }

            set { SetProperty(ref PoNumberValue, value); }

        }
        private string VendorBaIdValue;

        public string VendorBaId

        {

            get { return this.VendorBaIdValue; }

            set { SetProperty(ref VendorBaIdValue, value); }

        }
        private DateTime PoDateValue;

        public DateTime PoDate

        {

            get { return this.PoDateValue; }

            set { SetProperty(ref PoDateValue, value); }

        }
        private DateTime? ExpectedDeliveryDateValue;

        public DateTime? ExpectedDeliveryDate

        {

            get { return this.ExpectedDeliveryDateValue; }

            set { SetProperty(ref ExpectedDeliveryDateValue, value); }

        }
        private string DescriptionValue;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }
}
