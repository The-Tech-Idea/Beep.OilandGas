using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Setup
{
    public class AssetAccessSetupCollection : ModelEntityBase
    {
        private string UserIdValue = string.Empty;

        [Required]
        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }

        private string? OrganizationIdValue;


        public string? OrganizationId


        {


            get { return this.OrganizationIdValue; }


            set { SetProperty(ref OrganizationIdValue, value); }


        }

        private List<AssetAccessSetupData> AssetAccessesValue = new List<AssetAccessSetupData>();


        public List<AssetAccessSetupData> AssetAccesses


        {


            get { return this.AssetAccessesValue; }


            set { SetProperty(ref AssetAccessesValue, value); }


        }
    }
}
