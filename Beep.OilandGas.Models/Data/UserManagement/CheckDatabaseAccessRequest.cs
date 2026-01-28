using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.UserManagement
{
    public class CheckDatabaseAccessRequest : ModelEntityBase
    {
        private string UserIdValue = string.Empty;

        [Required(ErrorMessage = "UserId is required")]
        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }

        private string DatabaseNameValue = string.Empty;


        [Required(ErrorMessage = "DatabaseName is required")]
        public string DatabaseName


        {


            get { return this.DatabaseNameValue; }


            set { SetProperty(ref DatabaseNameValue, value); }


        }
    }
}
