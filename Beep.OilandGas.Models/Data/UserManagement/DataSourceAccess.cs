using System.ComponentModel.DataAnnotations;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.UserManagement
{
    /// <summary>
    /// Request to check if a user has access to a data source
    /// </summary>
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

    /// <summary>
    /// Request to check if a user has access to a database
    /// </summary>
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






