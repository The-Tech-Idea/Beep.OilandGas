using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class LOVResponse : ModelEntityBase
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
        private List<ListOfValue> LOVsValue = new List<ListOfValue>();

        public List<ListOfValue> LOVs

        {

            get { return this.LOVsValue; }

            set { SetProperty(ref LOVsValue, value); }

        }
        private int CountValue;

        public int Count

        {

            get { return this.CountValue; }

            set { SetProperty(ref CountValue, value); }

        }
    }
}
