using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class FX_RATE : ModelEntityBase
    {
        private string FX_RATE_IDValue;
        [Key,Required,MaxLength(128)] public string FX_RATE_ID
        {
            get { return this.FX_RATE_IDValue; }
            set { SetProperty(ref FX_RATE_IDValue, value); }
        }

        private string FROM_CURRENCYValue;
        [MaxLength(16)] public string FROM_CURRENCY
        {
            get { return this.FROM_CURRENCYValue; }
            set { SetProperty(ref FROM_CURRENCYValue, value); }
        }

        private string TO_CURRENCYValue;
        [MaxLength(16)] public string TO_CURRENCY
        {
            get { return this.TO_CURRENCYValue; }
            set { SetProperty(ref TO_CURRENCYValue, value); }
        }

        private DateTime? RATE_DATEValue;
        public DateTime? RATE_DATE
        {
            get { return this.RATE_DATEValue; }
            set { SetProperty(ref RATE_DATEValue, value); }
        }

        private decimal? RATEValue;
        public decimal? RATE
        {
            get { return this.RATEValue; }
            set { SetProperty(ref RATEValue, value); }
        }

        [MaxLength(128),JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string? OWNER_TENANT_ID{get;set;}
        [MaxLength(128),JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string? OWNER_COMPANY_BA_ID{get;set;}
        [MaxLength(128),JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string? BOOK_ID{get;set;}
        [MaxLength(128),JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string? RATE_VERSION{get;set;}
        [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public DateTime? EFFECTIVE_FROM{get;set;}
        [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public DateTime? EFFECTIVE_THROUGH{get;set;}
        [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public DateTime? OBSERVED_AT_UTC{get;set;}
        [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public int? RATE_DECIMALS{get;set;}
        [MaxLength(32),JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string? RATE_DIRECTION{get;set;}
        [MaxLength(256),JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string? PROVIDER_NAME{get;set;}
        [MaxLength(2000),JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string? PROVENANCE_REFERENCE{get;set;}
        [MaxLength(2000),JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string? AUTHORITY_REFERENCE{get;set;}
        [MaxLength(32),JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string? ALLOCATION_POLICY{get;set;}
        [MaxLength(64),JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string? BOOK_NATIVE_REVISION{get;set;}
        [MaxLength(262144),JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string? CONFIGURATION_JSON{get;set;}

    }
}
