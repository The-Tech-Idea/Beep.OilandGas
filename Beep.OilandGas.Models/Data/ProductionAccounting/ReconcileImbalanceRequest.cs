using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class ReconcileImbalanceRequest : ModelEntityBase
    {
        private string ImbalanceIdValue = string.Empty;

        [Required]
        public string ImbalanceId

        {

            get { return this.ImbalanceIdValue; }

            set { SetProperty(ref ImbalanceIdValue, value); }

        }
        private List<IMBALANCE_ADJUSTMENT> AdjustmentsValue = new();

        [Required]
        public List<IMBALANCE_ADJUSTMENT> Adjustments

        {

            get { return this.AdjustmentsValue; }

            set { SetProperty(ref AdjustmentsValue, value); }

        }
        private string ReconciledByValue = string.Empty;

        [Required]
        public string ReconciledBy

        {

            get { return this.ReconciledByValue; }

            set { SetProperty(ref ReconciledByValue, value); }

        }
        private string? NotesValue;

        public string? Notes

        {

            get { return this.NotesValue; }

            set { SetProperty(ref NotesValue, value); }

        }
    }
}
