using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class AssetAccess : ModelEntityBase
    {
        private string UserIdValue = string.Empty;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
        private string AssetTypeValue = string.Empty;

        public string AssetType

        {

            get { return this.AssetTypeValue; }

            set { SetProperty(ref AssetTypeValue, value); }

        }
        private string AssetIdValue = string.Empty;

        public string AssetId

        {

            get { return this.AssetIdValue; }

            set { SetProperty(ref AssetIdValue, value); }

        }
        private string AccessLevelValue = "READ";

        public string AccessLevel

        {

            get { return this.AccessLevelValue; }

            set { SetProperty(ref AccessLevelValue, value); }

        } // READ, WRITE, DELETE
        private bool InheritValue = true;

        public bool Inherit

        {

            get { return this.InheritValue; }

            set { SetProperty(ref InheritValue, value); }

        }
        private string? OrganizationIdValue;

        public string? OrganizationId

        {

            get { return this.OrganizationIdValue; }

            set { SetProperty(ref OrganizationIdValue, value); }

        }
        private bool ActiveValue = true;

        public bool Active

        {

            get { return this.ActiveValue; }

            set { SetProperty(ref ActiveValue, value); }

        }
    }
}
