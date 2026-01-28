using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class GetAccessibleAssetsRequest : ModelEntityBase
    {
        private string UserIdValue = string.Empty;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
        private string? AssetTypeValue;

        public string? AssetType

        {

            get { return this.AssetTypeValue; }

            set { SetProperty(ref AssetTypeValue, value); }

        }
        private string? OrganizationIdValue;

        public string? OrganizationId

        {

            get { return this.OrganizationIdValue; }

            set { SetProperty(ref OrganizationIdValue, value); }

        }
        private bool IncludeInheritedValue = true;

        public bool IncludeInherited

        {

            get { return this.IncludeInheritedValue; }

            set { SetProperty(ref IncludeInheritedValue, value); }

        }
    }
}
