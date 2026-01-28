using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class AllocationDetail : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the entity identifier (well, lease, tract, etc.).
        /// </summary>
        private string EntityIdValue = string.Empty;

        public string EntityId

        {

            get { return this.EntityIdValue; }

            set { SetProperty(ref EntityIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the entity name.
        /// </summary>
        private string EntityNameValue = string.Empty;

        public string EntityName

        {

            get { return this.EntityNameValue; }

            set { SetProperty(ref EntityNameValue, value); }

        }

        /// <summary>
        /// Gets or sets the allocated volume in barrels.
        /// </summary>
        private decimal AllocatedVolumeValue;

        public decimal AllocatedVolume

        {

            get { return this.AllocatedVolumeValue; }

            set { SetProperty(ref AllocatedVolumeValue, value); }

        }

        /// <summary>
        /// Gets or sets the allocation percentage (0-100).
        /// </summary>
        private decimal AllocationPercentageValue;

        public decimal AllocationPercentage

        {

            get { return this.AllocationPercentageValue; }

            set { SetProperty(ref AllocationPercentageValue, value); }

        }

        /// <summary>
        /// Gets or sets the basis for allocation (working interest, net revenue interest, etc.).
        /// </summary>
        private decimal AllocationBasisValue;

        public decimal AllocationBasis

        {

            get { return this.AllocationBasisValue; }

            set { SetProperty(ref AllocationBasisValue, value); }

        }
    }
}
