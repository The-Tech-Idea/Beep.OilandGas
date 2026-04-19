
namespace Beep.OilandGas.Models.Data.Accounting
{
    public class UpdatePurchaseOrderRequest : ModelEntityBase
    {
        private string PurchaseOrderIdValue;

        public string PurchaseOrderId

        {

            get { return this.PurchaseOrderIdValue; }

            set { SetProperty(ref PurchaseOrderIdValue, value); }

        }
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
        private DateTime? PoDateValue;

        public DateTime? PoDate

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
        private string StatusValue;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
    }
}
