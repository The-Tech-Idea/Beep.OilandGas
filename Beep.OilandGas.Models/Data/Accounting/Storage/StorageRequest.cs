namespace Beep.OilandGas.Models.Data.Accounting.Storage
{
    /// <summary>
    /// Request DTO for creating a storage facility
    /// </summary>
    public class CreateStorageFacilityRequest : ModelEntityBase
    {
        private string? FacilityIdValue;

        public string? FacilityId

        {

            get { return this.FacilityIdValue; }

            set { SetProperty(ref FacilityIdValue, value); }

        }
        private string FacilityNameValue = string.Empty;

        public string FacilityName

        {

            get { return this.FacilityNameValue; }

            set { SetProperty(ref FacilityNameValue, value); }

        }
        private string? LocationValue;

        public string? Location

        {

            get { return this.LocationValue; }

            set { SetProperty(ref LocationValue, value); }

        }
    }
}






