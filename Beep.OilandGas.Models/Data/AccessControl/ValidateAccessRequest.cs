using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.AccessControl
{
    public class ValidateAccessRequest : ModelEntityBase
    {
        private string UserIdValue = string.Empty;

        [Required(ErrorMessage = "UserId is required")]
        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }

        private List<AssetHierarchyNode> AssetPathValue = new List<AssetHierarchyNode>();


        [Required(ErrorMessage = "AssetPath is required")]
        public List<AssetHierarchyNode> AssetPath


        {


            get { return this.AssetPathValue; }


            set { SetProperty(ref AssetPathValue, value); }


        }
    }
}
