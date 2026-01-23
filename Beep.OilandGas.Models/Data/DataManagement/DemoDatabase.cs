using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.DataManagement
{
    /// <summary>
    /// Create demo database request
    /// </summary>
    public class CreateDemoDatabaseRequest : ModelEntityBase
    {
        private string UserIdValue = string.Empty;

        [Required(ErrorMessage = "UserId is required")]
        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }

        private string SeedDataOptionValue = "reference-sample";


        public string SeedDataOption


        {


            get { return this.SeedDataOptionValue; }


            set { SetProperty(ref SeedDataOptionValue, value); }


        } // none, reference-only, reference-sample, full-demo
        private string? ConnectionNameValue;

        public string? ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        } // Optional, will be auto-generated if not provided
    }

    /// <summary>
    /// Create demo database response
    /// </summary>
    public class CreateDemoDatabaseResponse : ModelEntityBase
    {
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private string ConnectionNameValue = string.Empty;

        public string ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
        private string DatabasePathValue = string.Empty;

        public string DatabasePath

        {

            get { return this.DatabasePathValue; }

            set { SetProperty(ref DatabasePathValue, value); }

        }
        private string MessageValue = string.Empty;

        public string Message

        {

            get { return this.MessageValue; }

            set { SetProperty(ref MessageValue, value); }

        }
        private string? ErrorDetailsValue;

        public string? ErrorDetails

        {

            get { return this.ErrorDetailsValue; }

            set { SetProperty(ref ErrorDetailsValue, value); }

        }
        private DateTime CreatedDateValue;

        public DateTime CreatedDate

        {

            get { return this.CreatedDateValue; }

            set { SetProperty(ref CreatedDateValue, value); }

        }
        private DateTime ExpiryDateValue;

        public DateTime ExpiryDate

        {

            get { return this.ExpiryDateValue; }

            set { SetProperty(ref ExpiryDateValue, value); }

        }
    }

    /// <summary>
    /// Demo database metadata
    /// </summary>
    public class DemoDatabaseMetadata : ModelEntityBase
    {
        private string ConnectionNameValue = string.Empty;

        public string ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
        private string UserIdValue = string.Empty;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
        private string DatabasePathValue = string.Empty;

        public string DatabasePath

        {

            get { return this.DatabasePathValue; }

            set { SetProperty(ref DatabasePathValue, value); }

        }
        private string SeedDataOptionValue = string.Empty;

        public string SeedDataOption

        {

            get { return this.SeedDataOptionValue; }

            set { SetProperty(ref SeedDataOptionValue, value); }

        }
        private DateTime CreatedDateValue;

        public DateTime CreatedDate

        {

            get { return this.CreatedDateValue; }

            set { SetProperty(ref CreatedDateValue, value); }

        }
        private DateTime ExpiryDateValue;

        public DateTime ExpiryDate

        {

            get { return this.ExpiryDateValue; }

            set { SetProperty(ref ExpiryDateValue, value); }

        }
        public bool IsExpired => DateTime.UtcNow > ExpiryDate;
    }

    /// <summary>
    /// List demo databases response
    /// </summary>
    public class ListDemoDatabasesResponse : ModelEntityBase
    {
        private List<DemoDatabaseMetadata> DatabasesValue = new();

        public List<DemoDatabaseMetadata> Databases

        {

            get { return this.DatabasesValue; }

            set { SetProperty(ref DatabasesValue, value); }

        }
        private int TotalCountValue;

        public int TotalCount

        {

            get { return this.TotalCountValue; }

            set { SetProperty(ref TotalCountValue, value); }

        }
        private int ExpiredCountValue;

        public int ExpiredCount

        {

            get { return this.ExpiredCountValue; }

            set { SetProperty(ref ExpiredCountValue, value); }

        }
    }

    /// <summary>
    /// Delete demo database response
    /// </summary>
    public class DeleteDemoDatabaseResponse : ModelEntityBase
    {
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private string MessageValue = string.Empty;

        public string Message

        {

            get { return this.MessageValue; }

            set { SetProperty(ref MessageValue, value); }

        }
        private string? ErrorDetailsValue;

        public string? ErrorDetails

        {

            get { return this.ErrorDetailsValue; }

            set { SetProperty(ref ErrorDetailsValue, value); }

        }
    }

    /// <summary>
    /// Cleanup demo databases response
    /// </summary>
    public class CleanupDemoDatabasesResponse : ModelEntityBase
    {
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private int DeletedCountValue;

        public int DeletedCount

        {

            get { return this.DeletedCountValue; }

            set { SetProperty(ref DeletedCountValue, value); }

        }
        private List<string> DeletedDatabasesValue = new();

        public List<string> DeletedDatabases

        {

            get { return this.DeletedDatabasesValue; }

            set { SetProperty(ref DeletedDatabasesValue, value); }

        }
        private string MessageValue = string.Empty;

        public string Message

        {

            get { return this.MessageValue; }

            set { SetProperty(ref MessageValue, value); }

        }
        private string? ErrorDetailsValue;

        public string? ErrorDetails

        {

            get { return this.ErrorDetailsValue; }

            set { SetProperty(ref ErrorDetailsValue, value); }

        }
    }
}







