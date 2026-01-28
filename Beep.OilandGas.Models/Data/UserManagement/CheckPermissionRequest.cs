using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.UserManagement
{
    public class CheckPermissionRequest : ModelEntityBase
    {
        private string UserIdValue = string.Empty;

        [Required(ErrorMessage = "UserId is required")]
        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }

        private string PermissionValue = string.Empty;


        [Required(ErrorMessage = "Permission is required")]
        public string Permission


        {


            get { return this.PermissionValue; }


            set { SetProperty(ref PermissionValue, value); }


        }
    }
}
