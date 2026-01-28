using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class TestSeparator : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the separator identifier.
        /// </summary>
        private string SeparatorIdValue = string.Empty;

        public string SeparatorId

        {

            get { return this.SeparatorIdValue; }

            set { SetProperty(ref SeparatorIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the separator name.
        /// </summary>
        private string SeparatorNameValue = string.Empty;

        public string SeparatorName

        {

            get { return this.SeparatorNameValue; }

            set { SetProperty(ref SeparatorNameValue, value); }

        }

        /// <summary>
        /// Gets or sets the test results.
        /// </summary>
        private List<TestResult> TestResultsValue = new();

        public List<TestResult> TestResults

        {

            get { return this.TestResultsValue; }

            set { SetProperty(ref TestResultsValue, value); }

        }
    }
}
