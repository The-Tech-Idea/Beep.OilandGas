using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class GetAssetHierarchyRequest : ModelEntityBase
    {
        private string? OrganizationIdValue;

        public string? OrganizationId

        {

            get { return this.OrganizationIdValue; }

            set { SetProperty(ref OrganizationIdValue, value); }

        }
        private string? RootAssetIdValue;

        public string? RootAssetId

        {

            get { return this.RootAssetIdValue; }

            set { SetProperty(ref RootAssetIdValue, value); }

        }
        private string? RootAssetTypeValue;

        public string? RootAssetType

        {

            get { return this.RootAssetTypeValue; }

            set { SetProperty(ref RootAssetTypeValue, value); }

        }
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        } // Optional: filter by user access
    }
}
