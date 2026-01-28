using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class SampleChainOfCustody : ModelEntityBase
    {
        private string SampleIdValue = string.Empty;

        public string SampleId

        {

            get { return this.SampleIdValue; }

            set { SetProperty(ref SampleIdValue, value); }

        }
        private List<CustodyTransfer> TransfersValue = new();

        public List<CustodyTransfer> Transfers

        {

            get { return this.TransfersValue; }

            set { SetProperty(ref TransfersValue, value); }

        }
        private string CurrentLocationValue = string.Empty;

        public string CurrentLocation

        {

            get { return this.CurrentLocationValue; }

            set { SetProperty(ref CurrentLocationValue, value); }

        }
        private string CurrentCustodianValue = string.Empty;

        public string CurrentCustodian

        {

            get { return this.CurrentCustodianValue; }

            set { SetProperty(ref CurrentCustodianValue, value); }

        }
        private DateTime LastTransferDateValue;

        public DateTime LastTransferDate

        {

            get { return this.LastTransferDateValue; }

            set { SetProperty(ref LastTransferDateValue, value); }

        }
        private string ChainIntegrityValue = string.Empty;

        public string ChainIntegrity

        {

            get { return this.ChainIntegrityValue; }

            set { SetProperty(ref ChainIntegrityValue, value); }

        }
    }
}
