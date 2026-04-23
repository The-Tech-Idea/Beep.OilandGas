using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class CreateSchemaResult : ModelEntityBase
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
        private string? SchemaNameValue;

        public string? SchemaName

        {

            get { return this.SchemaNameValue; }

            set { SetProperty(ref SchemaNameValue, value); }

        }
        private int TablesCreatedValue;

        public int TablesCreated

        {

            get { return this.TablesCreatedValue; }

            set { SetProperty(ref TablesCreatedValue, value); }

        }
        private int TotalEntitiesValue;

        public int TotalEntities

        {

            get { return this.TotalEntitiesValue; }

            set { SetProperty(ref TotalEntitiesValue, value); }

        }
    }
}
