using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.UserManagement
{
    public class CheckPermissionAnyRequest : ModelEntityBase
    {
        private string UserIdValue = string.Empty;

        [Required(ErrorMessage = "UserId is required")]
        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }

        private IEnumerable<string> PermissionsValue = Enumerable.Empty<string>();


        [Required(ErrorMessage = "At least one permission is required")]
        public IEnumerable<string> Permissions


        {


            get { return this.PermissionsValue; }


            set { SetProperty(ref PermissionsValue, value); }


        }
    }
}
