using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DrillingAndConstruction
{
    /// <summary>
    /// One review decision recorded against a drilling program version (DEV-05). Append-only: a later approval never
    /// erases the comment that returned the program, and the author/reviewer separation is visible in the history.
    /// </summary>
    public class DRILLING_PROGRAM_REVIEW : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string REVIEW_ID
        {
            get => ReviewIdValue;
            set => SetProperty(ref ReviewIdValue, value);
        }

        private string ReviewIdValue = string.Empty;

        [MaxLength(64)]
        public string PROGRAM_ID
        {
            get => ProgramIdValue;
            set => SetProperty(ref ProgramIdValue, value);
        }

        private string ProgramIdValue = string.Empty;

        /// <summary>The program version the decision was made on.</summary>
        public int PROGRAM_VERSION
        {
            get => ProgramVersionValue;
            set => SetProperty(ref ProgramVersionValue, value);
        }

        private int ProgramVersionValue;

        /// <summary>APPROVE, APPROVE_WITH_COMMENTS, RETURN or REJECT.</summary>
        [MaxLength(32)]
        public string? DECISION
        {
            get => DecisionValue;
            set => SetProperty(ref DecisionValue, value);
        }

        private string? DecisionValue;

        public string? COMMENTS
        {
            get => CommentsValue;
            set => SetProperty(ref CommentsValue, value);
        }

        private string? CommentsValue;

        [MaxLength(128)]
        public string? REVIEWER
        {
            get => ReviewerValue;
            set => SetProperty(ref ReviewerValue, value);
        }

        private string? ReviewerValue;

        public DateTime? REVIEWED_AT
        {
            get => ReviewedAtValue;
            set => SetProperty(ref ReviewedAtValue, value);
        }

        private DateTime? ReviewedAtValue;
    }
}
