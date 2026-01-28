using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class CategoryInfo : ModelEntityBase
    {
        private string CategoryValue = string.Empty;

        public string Category

        {

            get { return this.CategoryValue; }

            set { SetProperty(ref CategoryValue, value); }

        }
        private int CountValue;

        public int Count

        {

            get { return this.CountValue; }

            set { SetProperty(ref CountValue, value); }

        }
        private List<string> TablesValue = new List<string>();

        public List<string> Tables

        {

            get { return this.TablesValue; }

            set { SetProperty(ref TablesValue, value); }

        }
    }
}
