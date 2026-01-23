namespace Beep.OilandGas.Models.Data
{
    /// <summary>
    /// Maps a well identifier to its data source
    /// </summary>
    public class WellSourceMapping : ModelEntityBase
    {
        /// <summary>
        /// Well identifier (UWI, Well ID, etc.)
        /// </summary>
        private string WellIdentifierValue;

        public string WellIdentifier

        {

            get { return this.WellIdentifierValue; }

            set { SetProperty(ref WellIdentifierValue, value); }

        }

        /// <summary>
        /// Data source name/connection name
        /// </summary>
        private string DataSourceValue;

        public string DataSource

        {

            get { return this.DataSourceValue; }

            set { SetProperty(ref DataSourceValue, value); }

        }

        /// <summary>
        /// Optional well name for display
        /// </summary>
        private string WellNameValue;

        public string WellName

        {

            get { return this.WellNameValue; }

            set { SetProperty(ref WellNameValue, value); }

        }
    }
}






