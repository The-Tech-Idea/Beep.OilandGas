using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class RATablesExtractResponse : ModelEntityBase
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
        private int TotalTablesValue;

        public int TotalTables

        {

            get { return this.TotalTablesValue; }

            set { SetProperty(ref TotalTablesValue, value); }

        }
        private DateTime ExtractionDateValue;

        public DateTime ExtractionDate

        {

            get { return this.ExtractionDateValue; }

            set { SetProperty(ref ExtractionDateValue, value); }

        }
        private List<string> TableNamesValue = new List<string>();

        public List<string> TableNames

        {

            get { return this.TableNamesValue; }

            set { SetProperty(ref TableNamesValue, value); }

        }
        private List<CategoryInfo> CategoriesValue = new List<CategoryInfo>();

        public List<CategoryInfo> Categories

        {

            get { return this.CategoriesValue; }

            set { SetProperty(ref CategoriesValue, value); }

        }
    }
}
