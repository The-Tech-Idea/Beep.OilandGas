using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class AccessCheckRequest : ModelEntityBase
    {
        private string UserIdValue = string.Empty;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
        private string AssetIdValue = string.Empty;

        public string AssetId

        {

            get { return this.AssetIdValue; }

            set { SetProperty(ref AssetIdValue, value); }

        }
        private string AssetTypeValue = string.Empty;

        public string AssetType

        {

            get { return this.AssetTypeValue; }

            set { SetProperty(ref AssetTypeValue, value); }

        }
        private string? RequiredPermissionValue;

        public string? RequiredPermission

        {

            get { return this.RequiredPermissionValue; }

            set { SetProperty(ref RequiredPermissionValue, value); }

        }
    }
}
