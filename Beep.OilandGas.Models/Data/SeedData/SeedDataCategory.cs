using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class SeedDataCategory : ModelEntityBase
    {
        private string CategoryNameValue = string.Empty;

        public string CategoryName

        {

            get { return this.CategoryNameValue; }

            set { SetProperty(ref CategoryNameValue, value); }

        }
        private string DescriptionValue = string.Empty;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private List<string> TableNamesValue = new List<string>();

        public List<string> TableNames

        {

            get { return this.TableNamesValue; }

            set { SetProperty(ref TableNamesValue, value); }

        }
        private int EstimatedRecordsValue;

        public int EstimatedRecords

        {

            get { return this.EstimatedRecordsValue; }

            set { SetProperty(ref EstimatedRecordsValue, value); }

        }
    }
}
