using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Setup
{
    public class AssetAccessSetupData : ModelEntityBase
    {
        private string UserIdValue = string.Empty;

        [Required]
        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }

        private string AssetTypeValue = string.Empty;


        [Required]
        public string AssetType


        {


            get { return this.AssetTypeValue; }


            set { SetProperty(ref AssetTypeValue, value); }


        } // FIELD, POOL, FACILITY, WELL, etc.

        private string AssetIdValue = string.Empty;


        [Required]
        public string AssetId


        {


            get { return this.AssetIdValue; }


            set { SetProperty(ref AssetIdValue, value); }


        }

        private string AccessLevelValue = "READ";


        [Required]
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

        private DateTime? EffectiveDateValue;


        public DateTime? EffectiveDate


        {


            get { return this.EffectiveDateValue; }


            set { SetProperty(ref EffectiveDateValue, value); }


        }

        private DateTime? ExpiryDateValue;


        public DateTime? ExpiryDate


        {


            get { return this.ExpiryDateValue; }


            set { SetProperty(ref ExpiryDateValue, value); }


        }

        private bool ActiveValue = true;


        public bool Active


        {


            get { return this.ActiveValue; }


            set { SetProperty(ref ActiveValue, value); }


        }
    }
}
