using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PermitsAndApplications
{
    public class PermitValidationResult : ModelEntityBase
    {
        /// <summary>
        /// Indicates if the application is valid.
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
        private List<string> ErrorsValue = new();

        public List<string> Errors

        {

            get { return this.ErrorsValue; }

            set { SetProperty(ref ErrorsValue, value); }

        }

        /// <summary>
        /// List of validation warnings.
        /// </summary>
        private List<string> WarningsValue = new();

        public List<string> Warnings

        {

            get { return this.WarningsValue; }

            set { SetProperty(ref WarningsValue, value); }

        }

        /// <summary>
        /// List of required forms that are missing.
        /// </summary>
        private List<string> MissingFormsValue = new();

        public List<string> MissingForms

        {

            get { return this.MissingFormsValue; }

            set { SetProperty(ref MissingFormsValue, value); }

        }

        /// <summary>
        /// Completion percentage of the application.
        /// </summary>
        private decimal CompletionPercentageValue;

        public decimal CompletionPercentage

        {

            get { return this.CompletionPercentageValue; }

            set { SetProperty(ref CompletionPercentageValue, value); }

        }
    }
}
