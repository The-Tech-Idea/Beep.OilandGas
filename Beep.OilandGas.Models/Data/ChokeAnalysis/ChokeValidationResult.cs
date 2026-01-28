using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ChokeAnalysis
{
    public class ChokeValidationResult : ModelEntityBase
    {
        /// <summary>
        /// Indicates if configuration is valid.
        /// </summary>
        private bool IsValidValue;

        public bool IsValid

        {

            get { return this.IsValidValue; }

            set { SetProperty(ref IsValidValue, value); }

        }

        /// <summary>
        /// List of validation errors.
        /// </summary>
        private string[] ErrorsValue = Array.Empty<string>();

        public string[] Errors

        {

            get { return this.ErrorsValue; }

            set { SetProperty(ref ErrorsValue, value); }

        }

        /// <summary>
        /// List of warnings.
        /// </summary>
        private string[] WarningsValue = Array.Empty<string>();

        public string[] Warnings

        {

            get { return this.WarningsValue; }

            set { SetProperty(ref WarningsValue, value); }

        }
    }
}
