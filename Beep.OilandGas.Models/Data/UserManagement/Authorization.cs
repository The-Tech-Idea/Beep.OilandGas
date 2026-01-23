using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.UserManagement
{
    /// <summary>
    /// Request to check if a user has a specific permission
    /// </summary>
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

    /// <summary>
    /// Request to check if a user has any of the specified permissions
    /// </summary>
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

    /// <summary>
    /// Request to check if a user has all of the specified permissions
    /// </summary>
    public class CheckPermissionAllRequest : ModelEntityBase
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

    /// <summary>
    /// Request to check if a user has a specific role
    /// </summary>
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






