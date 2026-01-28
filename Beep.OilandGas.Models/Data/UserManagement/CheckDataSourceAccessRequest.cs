using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.UserManagement
{
    public class CheckDataSourceAccessRequest : ModelEntityBase
    {
        private string UserIdValue = string.Empty;

        [Required(ErrorMessage = "UserId is required")]
        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }

        private string DataSourceNameValue = string.Empty;


        [Required(ErrorMessage = "DataSourceName is required")]
        public string DataSourceName


        {


            get { return this.DataSourceNameValue; }


            set { SetProperty(ref DataSourceNameValue, value); }


        }
    }
}
