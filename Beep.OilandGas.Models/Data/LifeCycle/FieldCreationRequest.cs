using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.LifeCycle
{
    public class FieldCreationRequest : ModelEntityBase
    {
        private string FieldNameValue = string.Empty;

        public string FieldName

        {

            get { return this.FieldNameValue; }

            set { SetProperty(ref FieldNameValue, value); }

        }
        private string? FieldTypeValue;

        public string? FieldType

        {

            get { return this.FieldTypeValue; }

            set { SetProperty(ref FieldTypeValue, value); }

        }
        private string? AreaIdValue;

        public string? AreaId

        {

            get { return this.AreaIdValue; }

            set { SetProperty(ref AreaIdValue, value); }

        }
        private string? BasinIdValue;

        public string? BasinId

        {

            get { return this.BasinIdValue; }

            set { SetProperty(ref BasinIdValue, value); }

        }
        private string? CountryValue;

        public string? Country

        {

            get { return this.CountryValue; }

            set { SetProperty(ref CountryValue, value); }

        }
        private string? StateProvinceValue;

        public string? StateProvince

        {

            get { return this.StateProvinceValue; }

            set { SetProperty(ref StateProvinceValue, value); }

        }
        private string? CountyValue;

        public string? County

        {

            get { return this.CountyValue; }

            set { SetProperty(ref CountyValue, value); }

        }
        public Dictionary<string, object>? AdditionalProperties { get; set; }
    }
}
