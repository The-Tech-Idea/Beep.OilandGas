using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class LACTTransferRecord : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the transfer class identifier.
        /// </summary>
        private string TransferRecordIdValue = string.Empty;

        public string TransferRecordId

        {

            get { return this.TransferRecordIdValue; }

            set { SetProperty(ref TransferRecordIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the transfer date.
        /// </summary>
        private DateTime TransferDateValue;

        public DateTime TransferDate

        {

            get { return this.TransferDateValue; }

            set { SetProperty(ref TransferDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the volume transferred in barrels.
        /// </summary>
        private decimal VolumeTransferredValue;

        public decimal VolumeTransferred

        {

            get { return this.VolumeTransferredValue; }

            set { SetProperty(ref VolumeTransferredValue, value); }

        }

        /// <summary>
        /// Gets or sets the run ticket number.
        /// </summary>
        private string? RunTicketNumberValue;

        public string? RunTicketNumber

        {

            get { return this.RunTicketNumberValue; }

            set { SetProperty(ref RunTicketNumberValue, value); }

        }
    }
}
