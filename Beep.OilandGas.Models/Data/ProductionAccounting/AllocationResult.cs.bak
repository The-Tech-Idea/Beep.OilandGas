using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class AllocationResult : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the allocation identifier.
        /// </summary>
        private string AllocationIdValue = string.Empty;

        public string AllocationId

        {

            get { return this.AllocationIdValue; }

            set { SetProperty(ref AllocationIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the allocation date.
        /// </summary>
        private DateTime AllocationDateValue;

        public DateTime AllocationDate

        {

            get { return this.AllocationDateValue; }

            set { SetProperty(ref AllocationDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the allocation method used.
        /// </summary>
        private AllocationMethod MethodValue;

        public AllocationMethod Method

        {

            get { return this.MethodValue; }

            set { SetProperty(ref MethodValue, value); }

        }

        /// <summary>
        /// Gets or sets the total volume allocated.
        /// </summary>
        private decimal TotalVolumeValue;

        public decimal TotalVolume

        {

            get { return this.TotalVolumeValue; }

            set { SetProperty(ref TotalVolumeValue, value); }

        }

        /// <summary>
        /// Gets or sets the allocation details.
        /// </summary>
        private List<AllocationDetail> DetailsValue = new();

        public List<AllocationDetail> Details

        {

            get { return this.DetailsValue; }

            set { SetProperty(ref DetailsValue, value); }

        }

        /// <summary>
        /// Gets the sum of all allocated volumes (should equal TotalVolume).
        /// </summary>
        public decimal AllocatedVolume => Details.Sum(d => d.AllocatedVolume);

        /// <summary>
        /// Gets the allocation variance.
        /// </summary>
        public decimal AllocationVariance => TotalVolume - AllocatedVolume;
    }
}
