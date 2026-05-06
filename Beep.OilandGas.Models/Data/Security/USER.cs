using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Security
{
    public partial class USER : ModelEntityBase
    {
        private string USER_IDValue = string.Empty;
        public string USER_ID
        {
            get => USER_IDValue;
            set => SetProperty(ref USER_IDValue, value);
        }

        private string USERNAMEValue = string.Empty;
        public string USERNAME
        {
            get => USERNAMEValue;
            set => SetProperty(ref USERNAMEValue, value);
        }

        private string USER_NAMEValue = string.Empty;
        public string USER_NAME
        {
            get => USER_NAMEValue;
            set => SetProperty(ref USER_NAMEValue, value);
        }

        private string FULL_NAMEValue = string.Empty;
        public string FULL_NAME
        {
            get => FULL_NAMEValue;
            set => SetProperty(ref FULL_NAMEValue, value);
        }

        private string EMAILValue = string.Empty;
        public string EMAIL
        {
            get => EMAILValue;
            set => SetProperty(ref EMAILValue, value);
        }

        private string PASSWORD_HASHValue = string.Empty;
        public string PASSWORD_HASH
        {
            get => PASSWORD_HASHValue;
            set => SetProperty(ref PASSWORD_HASHValue, value);
        }

        private bool IS_ACTIVEValue;
        public bool IS_ACTIVE
        {
            get => IS_ACTIVEValue;
            set => SetProperty(ref IS_ACTIVEValue, value);
        }

        private string TENANT_IDValue = string.Empty;
        public string TENANT_ID
        {
            get => TENANT_IDValue;
            set => SetProperty(ref TENANT_IDValue, value);
        }

        private string? BUSINESS_ASSOCIATE_IDValue;
        public string? BUSINESS_ASSOCIATE_ID
        {
            get => BUSINESS_ASSOCIATE_IDValue;
            set => SetProperty(ref BUSINESS_ASSOCIATE_IDValue, value);
        }

        // ── Authentication & Lockout Fields ─────────────────────────────────

        private int? FAILED_LOGIN_COUNTValue;
        public int? FAILED_LOGIN_COUNT
        {
            get => FAILED_LOGIN_COUNTValue;
            set => SetProperty(ref FAILED_LOGIN_COUNTValue, value);
        }

        private string? LOCKED_INDValue;
        public string? LOCKED_IND
        {
            get => LOCKED_INDValue;
            set => SetProperty(ref LOCKED_INDValue, value);
        }

        private DateTime? LOCKOUT_UNTIL_UTCValue;
        public DateTime? LOCKOUT_UNTIL_UTC
        {
            get => LOCKOUT_UNTIL_UTCValue;
            set => SetProperty(ref LOCKOUT_UNTIL_UTCValue, value);
        }

        private DateTime? LAST_LOGIN_UTCValue;
        public DateTime? LAST_LOGIN_UTC
        {
            get => LAST_LOGIN_UTCValue;
            set => SetProperty(ref LAST_LOGIN_UTCValue, value);
        }

        private DateTime? LAST_PASSWORD_CHANGE_UTCValue;
        public DateTime? LAST_PASSWORD_CHANGE_UTC
        {
            get => LAST_PASSWORD_CHANGE_UTCValue;
            set => SetProperty(ref LAST_PASSWORD_CHANGE_UTCValue, value);
        }

        private int? MAX_PASSWORD_AGE_DAYSValue;
        public int? MAX_PASSWORD_AGE_DAYS
        {
            get => MAX_PASSWORD_AGE_DAYSValue;
            set => SetProperty(ref MAX_PASSWORD_AGE_DAYSValue, value);
        }

        private string? PASSWORD_RESET_TOKENValue;
        public string? PASSWORD_RESET_TOKEN
        {
            get => PASSWORD_RESET_TOKENValue;
            set => SetProperty(ref PASSWORD_RESET_TOKENValue, value);
        }

        private DateTime? PASSWORD_RESET_TOKEN_EXPIRY_UTCValue;
        public DateTime? PASSWORD_RESET_TOKEN_EXPIRY_UTC
        {
            get => PASSWORD_RESET_TOKEN_EXPIRY_UTCValue;
            set => SetProperty(ref PASSWORD_RESET_TOKEN_EXPIRY_UTCValue, value);
        }

        private string? MFA_ENABLED_INDValue;
        public string? MFA_ENABLED_IND
        {
            get => MFA_ENABLED_INDValue;
            set => SetProperty(ref MFA_ENABLED_INDValue, value);
        }

        // Standard PPDM columns
    }
}
