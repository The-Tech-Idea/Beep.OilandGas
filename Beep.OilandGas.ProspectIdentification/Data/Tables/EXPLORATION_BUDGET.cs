using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
    public partial class EXPLORATION_BUDGET : ModelEntityBase
    {
        private System.String BUDGET_IDValue;
        /// <summary>PK for this budget line (PROGRAM_ID + BUDGET_YEAR is not unique across versions).</summary>
        public System.String BUDGET_ID
        {
            get { return this.BUDGET_IDValue; }
            set { SetProperty(ref BUDGET_IDValue, value); }
        }

        private System.String PROGRAM_IDValue;
        public System.String PROGRAM_ID
        {
            get
            {
                return this.PROGRAM_IDValue;
            }
            set { SetProperty(ref PROGRAM_IDValue, value); }
        }
        private System.Int32 BUDGET_YEARValue;
        /// <summary>Fiscal year of the budget (integer, e.g. 2024).</summary>
        public System.Int32 BUDGET_YEAR
        {
            get
            {
                return this.BUDGET_YEARValue;
            }
            set { SetProperty(ref BUDGET_YEARValue, value); }
        }
        private System.Decimal  BUDGET_AMOUNTValue;
        public System.Decimal  BUDGET_AMOUNT
        {
            get
            {
                return this.BUDGET_AMOUNTValue;
            }
            set { SetProperty(ref BUDGET_AMOUNTValue, value); }
        }
        private System.String BUDGET_CURRENCYValue;
        public System.String BUDGET_CURRENCY
        {
            get
            {
                return this.BUDGET_CURRENCYValue;
            }
            set { SetProperty(ref BUDGET_CURRENCYValue, value); }
        }
        private System.String BUDGET_CATEGORYValue;
        public System.String BUDGET_CATEGORY
        {
            get
            {
                return this.BUDGET_CATEGORYValue;
            }
            set { SetProperty(ref BUDGET_CATEGORYValue, value); }
        }
        private System.Decimal  ACTUAL_AMOUNTValue;
        public System.Decimal  ACTUAL_AMOUNT
        {
            get
            {
                return this.ACTUAL_AMOUNTValue;
            }
            set { SetProperty(ref ACTUAL_AMOUNTValue, value); }
        }
        private System.Decimal  VARIANCEValue;
        public System.Decimal  VARIANCE
        {
            get
            {
                return this.VARIANCEValue;
            }
            set { SetProperty(ref VARIANCEValue, value); }
        }
        private System.String DESCRIPTIONValue;
        public System.String DESCRIPTION
        {
            get
            {
                return this.DESCRIPTIONValue;
            }
            set { SetProperty(ref DESCRIPTIONValue, value); }
        }
        private System.DateTime? EFFECTIVE_DATEValue;

        private System.DateTime? EXPIRY_DATEValue;

        private System.String REMARKValue;

        private System.String SOURCEValue;

        // --- O&G best-practice additions (AFE / budget approval workflow) ---

        private System.String AFE_NUMBERValue;
        /// <summary>Authorization for Expenditure reference number (industry-standard cost control ID).</summary>
        public System.String AFE_NUMBER
        {
            get { return this.AFE_NUMBERValue; }
            set { SetProperty(ref AFE_NUMBERValue, value); }
        }

        private System.Int32 BUDGET_VERSIONValue;
        /// <summary>AFE revision version (1 = original, 2+ = supplemental AFE).</summary>
        public System.Int32 BUDGET_VERSION
        {
            get { return this.BUDGET_VERSIONValue; }
            set { SetProperty(ref BUDGET_VERSIONValue, value); }
        }

        private System.String APPROVAL_STATUSValue;
        /// <summary>Budget approval lifecycle: DRAFT / PENDING / APPROVED / REJECTED.</summary>
        public System.String APPROVAL_STATUS
        {
            get { return this.APPROVAL_STATUSValue; }
            set { SetProperty(ref APPROVAL_STATUSValue, value); }
        }

        private System.String APPROVED_BYValue;
        /// <summary>User ID of the approver.</summary>
        public System.String APPROVED_BY
        {
            get { return this.APPROVED_BYValue; }
            set { SetProperty(ref APPROVED_BYValue, value); }
        }

        private System.DateTime? APPROVAL_DATEValue;
        public System.DateTime? APPROVAL_DATE
        {
            get { return this.APPROVAL_DATEValue; }
            set { SetProperty(ref APPROVAL_DATEValue, value); }
        }

        public EXPLORATION_BUDGET() { }
    }
}
