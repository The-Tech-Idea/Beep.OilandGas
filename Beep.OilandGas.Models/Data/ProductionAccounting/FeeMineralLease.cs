using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class FeeMineralLease : LeaseAgreement
    {
        /// <summary>
        /// Gets or sets the mineral owner information.
        /// </summary>
        private string MineralOwnerValue = string.Empty;

        public string MineralOwner

        {

            get { return this.MineralOwnerValue; }

            set { SetProperty(ref MineralOwnerValue, value); }

        }

        /// <summary>
        /// Gets or sets the surface owner information.
        /// </summary>
        private string? SurfaceOwnerValue;

        public string? SurfaceOwner

        {

            get { return this.SurfaceOwnerValue; }

            set { SetProperty(ref SurfaceOwnerValue, value); }

        }

        public FeeMineralLease()
        {
            LeaseType = LeaseType.Fee;
        }
    }
}
