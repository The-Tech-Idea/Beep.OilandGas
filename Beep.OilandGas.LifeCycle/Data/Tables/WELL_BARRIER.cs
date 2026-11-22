using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.LifeCycle.Data.Tables
{
    /// One well-barrier element of a well's barrier set (ENG-05): its envelope, what it refers to and its state.
    /// </summary>
    /// <remarks>
    /// The integrity category is the guideline-117 reading of the barrier set and the annulus and wall-loss evidence, so
    /// without the set every well would be judged "no barrier set with two envelopes" and the board would be a single colour.
    /// The tools model the set; nothing in either repository records it.
    /// </remarks>
    public class WELL_BARRIER : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string BARRIER_ID
        {
            get => BarrieridValue;
            set => SetProperty(ref BarrieridValue, value);
        }

        private string BarrieridValue = string.Empty;

        [MaxLength(64)]
        public string UWI
        {
            get => UwiValue;
            set => SetProperty(ref UwiValue, value);
        }

        private string UwiValue = string.Empty;

        /// <summary>The field the well belongs to, so the board can filter by it.</summary>
        [MaxLength(64)]
        public string? FIELD_NAME
        {
            get => FieldnameValue;
            set => SetProperty(ref FieldnameValue, value);
        }

        private string? FieldnameValue = null;

        /// <summary>The phase the set was drawn for: Production, Drilling, P&A...</summary>
        [MaxLength(32)]
        public string? PHASE
        {
            get => PhaseValue;
            set => SetProperty(ref PhaseValue, value);
        }

        private string? PhaseValue = null;

        /// <summary>Planned or AsBuilt - the owner's barrier document status.</summary>
        [MaxLength(16)]
        public string? SET_STATUS
        {
            get => SetstatusValue;
            set => SetProperty(ref SetstatusValue, value);
        }

        private string? SetstatusValue = null;

        [MaxLength(32)]
        public string? REVISION
        {
            get => RevisionValue;
            set => SetProperty(ref RevisionValue, value);
        }

        private string? RevisionValue = null;

        /// <summary>The date of the barrier set the element belongs to.</summary>
        public DateTime? SET_DATE
        {
            get => SetdateValue;
            set => SetProperty(ref SetdateValue, value);
        }

        private DateTime? SetdateValue = null;

        [MaxLength(128)]
        public string? PREPARED_BY
        {
            get => PreparedbyValue;
            set => SetProperty(ref PreparedbyValue, value);
        }

        private string? PreparedbyValue = null;

        [MaxLength(128)]
        public string? VERIFIED_BY
        {
            get => VerifiedbyValue;
            set => SetProperty(ref VerifiedbyValue, value);
        }

        private string? VerifiedbyValue = null;

        /// <summary>Primary, Secondary or Environmental.</summary>
        [MaxLength(16)]
        public string ENVELOPE_ROLE
        {
            get => EnveloperoleValue;
            set => SetProperty(ref EnveloperoleValue, value);
        }

        private string EnveloperoleValue = string.Empty;

        /// <summary>The well-barrier-element type code, e.g. CASING_CEMENT or DHSV.</summary>
        [MaxLength(64)]
        public string ELEMENT_TYPE
        {
            get => ElementtypeValue;
            set => SetProperty(ref ElementtypeValue, value);
        }

        private string ElementtypeValue = string.Empty;

        [MaxLength(128)]
        public string? ELEMENT_NAME
        {
            get => ElementnameValue;
            set => SetProperty(ref ElementnameValue, value);
        }

        private string? ElementnameValue = null;

        /// <summary>Component, String, Cement, Formation or FluidColumn.</summary>
        [MaxLength(16)]
        public string? TARGET_KIND
        {
            get => TargetkindValue;
            set => SetProperty(ref TargetkindValue, value);
        }

        private string? TargetkindValue = null;

        /// <summary>The id of the component or string the element refers to - the same id the model gives it.</summary>
        [MaxLength(128)]
        public string? TARGET_ID
        {
            get => TargetidValue;
            set => SetProperty(ref TargetidValue, value);
        }

        private string? TargetidValue = null;

        public decimal? TOP_DEPTH
        {
            get => TopdepthValue;
            set => SetProperty(ref TopdepthValue, value);
        }

        private decimal? TopdepthValue = null;

        [MaxLength(16)]
        public string? TOP_DEPTH_OUOM
        {
            get => TopdepthouomValue;
            set => SetProperty(ref TopdepthouomValue, value);
        }

        private string? TopdepthouomValue = null;

        public decimal? BOTTOM_DEPTH
        {
            get => BottomdepthValue;
            set => SetProperty(ref BottomdepthValue, value);
        }

        private decimal? BottomdepthValue = null;

        [MaxLength(16)]
        public string? BOTTOM_DEPTH_OUOM
        {
            get => BottomdepthouomValue;
            set => SetProperty(ref BottomdepthouomValue, value);
        }

        private string? BottomdepthouomValue = null;

        /// <summary>Intact, Unverified, Impaired or Failed.</summary>
        [MaxLength(16)]
        public string ELEMENT_STATUS
        {
            get => ElementstatusValue;
            set => SetProperty(ref ElementstatusValue, value);
        }

        private string ElementstatusValue = string.Empty;

        public DateTime? VERIFIED_DATE
        {
            get => VerifieddateValue;
            set => SetProperty(ref VerifieddateValue, value);
        }

        private DateTime? VerifieddateValue = null;

        [MaxLength(128)]
        public string? VERIFICATION_METHOD
        {
            get => VerificationmethodValue;
            set => SetProperty(ref VerificationmethodValue, value);
        }

        private string? VerificationmethodValue = null;

        [MaxLength(128)]
        public string? ACCEPTANCE_REF
        {
            get => AcceptancerefValue;
            set => SetProperty(ref AcceptancerefValue, value);
        }

        private string? AcceptancerefValue = null;

        [MaxLength(1000)]
        public string? NOTES
        {
            get => NotesValue;
            set => SetProperty(ref NotesValue, value);
        }

        private string? NotesValue = null;

    }
}
