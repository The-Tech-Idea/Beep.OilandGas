using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.LifeCycle.Data.Tables
{
    /// One recorded integrity assessment of a well (ENG-05): the category, the reasons, the findings with their inputs, the
    /// state of each envelope and when and by whom it was made.
    /// </summary>
    /// <remarks>
    /// The feature asks for a "last assessed date" and distinguishes an unassessed well from a green one, which only a
    /// recorded assessment can answer. The category is stored with the reasons it was read from, so a later change to the
    /// barriers or the tests does not rewrite what an assessment concluded.
    /// </remarks>
    public class INTEGRITY_ASSESSMENT : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string ASSESSMENT_ID
        {
            get => AssessmentidValue;
            set => SetProperty(ref AssessmentidValue, value);
        }

        private string AssessmentidValue = string.Empty;

        [MaxLength(64)]
        public string UWI
        {
            get => UwiValue;
            set => SetProperty(ref UwiValue, value);
        }

        private string UwiValue = string.Empty;

        [MaxLength(64)]
        public string? FIELD_NAME
        {
            get => FieldnameValue;
            set => SetProperty(ref FieldnameValue, value);
        }

        private string? FieldnameValue = null;

        /// <summary>Green, Yellow, Orange or Red - the owner's guideline-117 category.</summary>
        [MaxLength(16)]
        public string CATEGORY
        {
            get => CategoryValue;
            set => SetProperty(ref CategoryValue, value);
        }

        private string CategoryValue = string.Empty;

        /// <summary>Y when the category complies with the two-barrier principle (Green or Yellow).</summary>
        [MaxLength(1)]
        public string? COMPLIANT
        {
            get => CompliantValue;
            set => SetProperty(ref CompliantValue, value);
        }

        private string? CompliantValue = null;

        /// <summary>The owner's reasons, one per line.</summary>
        [MaxLength(2000)]
        public string? REASONS_TEXT
        {
            get => ReasonstextValue;
            set => SetProperty(ref ReasonstextValue, value);
        }

        private string? ReasonstextValue = null;

        /// <summary>Every finding with the input that produced it.</summary>
        [MaxLength(4000)]
        public string? FINDINGS_JSON
        {
            get => FindingsjsonValue;
            set => SetProperty(ref FindingsjsonValue, value);
        }

        private string? FindingsjsonValue = null;

        /// <summary>The state of each barrier envelope the assessment saw.</summary>
        [MaxLength(1000)]
        public string? ENVELOPES_JSON
        {
            get => EnvelopesjsonValue;
            set => SetProperty(ref EnvelopesjsonValue, value);
        }

        private string? EnvelopesjsonValue = null;

        /// <summary>How many annuli had a classification at the assessment.</summary>
        public int ANNULI_COUNT
        {
            get => AnnulicountValue;
            set => SetProperty(ref AnnulicountValue, value);
        }

        private int AnnulicountValue = 0;

        /// <summary>How many corroded joints were supplied.</summary>
        public int JOINTS_COUNT
        {
            get => JointscountValue;
            set => SetProperty(ref JointscountValue, value);
        }

        private int JointscountValue = 0;

        [MaxLength(1)]
        public string? LEAK_TO_SURFACE
        {
            get => LeaktosurfaceValue;
            set => SetProperty(ref LeaktosurfaceValue, value);
        }

        private string? LeaktosurfaceValue = null;

        [MaxLength(128)]
        public string? ASSESSED_BY
        {
            get => AssessedbyValue;
            set => SetProperty(ref AssessedbyValue, value);
        }

        private string? AssessedbyValue = null;

        public DateTime ASSESSED_AT_UTC
        {
            get => AssessedatutcValue;
            set => SetProperty(ref AssessedatutcValue, value);
        }

        private DateTime AssessedatutcValue = DateTime.UtcNow;

    }
}
