using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.UserManagement
{
    public class CheckRoleRequest : ModelEntityBase
    {
        private string UserIdValue = string.Empty;

        [Required(ErrorMessage = "UserId is required")]
        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }

        private string RoleValue = string.Empty;


        [Required(ErrorMessage = "Role is required")]
        public string Role


        {


            get { return this.RoleValue; }


            set { SetProperty(ref RoleValue, value); }


        }
    }
}
