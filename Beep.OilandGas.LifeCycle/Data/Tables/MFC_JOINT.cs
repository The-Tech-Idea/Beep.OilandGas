using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.LifeCycle.Data.Tables
{
    /// One joint's wall-loss statistics and its re-rating (ENG-03), written per joint so the grid on screen and the
    /// record behind it are the same rows.
    /// </summary>
    /// <remarks>
    /// The measured figures and the re-rated resistances are stored rather than recomputed on read: a later change to
    /// <c>integrity.wallBasis</c> or to a grade's minimum yield must not silently rewrite what a past inspection
    /// concluded.
    /// </remarks>
    public class MFC_JOINT : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string MFC_JOINT_ID
        {
            get => MfcjointidValue;
            set => SetProperty(ref MfcjointidValue, value);
        }

        private string MfcjointidValue = string.Empty;

        [MaxLength(64)]
        public string MFC_RESULT_ID
        {
            get => MfcresultidValue;
            set => SetProperty(ref MfcresultidValue, value);
        }

        private string MfcresultidValue = string.Empty;

        [MaxLength(64)]
        public string UWI
        {
            get => UwiValue;
            set => SetProperty(ref UwiValue, value);
        }

        private string UwiValue = string.Empty;

        public int JOINT_NUMBER
        {
            get => JointnumberValue;
            set => SetProperty(ref JointnumberValue, value);
        }

        private int JointnumberValue = 0;

        public decimal TOP_DEPTH
        {
            get => TopdepthValue;
            set => SetProperty(ref TopdepthValue, value);
        }

        private decimal TopdepthValue = 0m;

        public decimal BOTTOM_DEPTH
        {
            get => BottomdepthValue;
            set => SetProperty(ref BottomdepthValue, value);
        }

        private decimal BottomdepthValue = 0m;

        [MaxLength(16)]
        public string? DEPTH_OUOM
        {
            get => DepthouomValue;
            set => SetProperty(ref DepthouomValue, value);
        }

        private string? DepthouomValue = null;

        /// <summary>Maximum penetration as a fraction of the nominal wall.</summary>
        public decimal? MAX_PENETRATION
        {
            get => MaxpenetrationValue;
            set => SetProperty(ref MaxpenetrationValue, value);
        }

        private decimal? MaxpenetrationValue = null;

        /// <summary>The depth the maximum penetration was measured at.</summary>
        public decimal? PENETRATION_DEPTH
        {
            get => PenetrationdepthValue;
            set => SetProperty(ref PenetrationdepthValue, value);
        }

        private decimal? PenetrationdepthValue = null;

        /// <summary>Maximum areal metal loss as a fraction of the nominal wall area.</summary>
        public decimal? MAX_METAL_LOSS
        {
            get => MaxmetallossValue;
            set => SetProperty(ref MaxmetallossValue, value);
        }

        private decimal? MaxmetallossValue = null;

        /// <summary>The minimum remaining wall measured, in inches.</summary>
        public decimal? MIN_REMAINING_WALL
        {
            get => MinremainingwallValue;
            set => SetProperty(ref MinremainingwallValue, value);
        }

        private decimal? MinremainingwallValue = null;

        public decimal? NOMINAL_WALL
        {
            get => NominalwallValue;
            set => SetProperty(ref NominalwallValue, value);
        }

        private decimal? NominalwallValue = null;

        public decimal? MAX_OVALITY
        {
            get => MaxovalityValue;
            set => SetProperty(ref MaxovalityValue, value);
        }

        private decimal? MaxovalityValue = null;

        public decimal? MIN_ID
        {
            get => MinidValue;
            set => SetProperty(ref MinidValue, value);
        }

        private decimal? MinidValue = null;

        /// <summary>The damage class the settings assigned, or null when no class covers this penetration.</summary>
        [MaxLength(64)]
        public string? DAMAGE_CLASS
        {
            get => DamageclassValue;
            set => SetProperty(ref DamageclassValue, value);
        }

        private string? DamageclassValue = null;

        [MaxLength(128)]
        public string? STRING_NAME
        {
            get => StringnameValue;
            set => SetProperty(ref StringnameValue, value);
        }

        private string? StringnameValue = null;

        [MaxLength(32)]
        public string? GRADE
        {
            get => GradeValue;
            set => SetProperty(ref GradeValue, value);
        }

        private string? GradeValue = null;

        /// <summary>The wall the re-rating used, in inches - minimum or average per the recorded basis.</summary>
        public decimal? WALL_USED
        {
            get => WallusedValue;
            set => SetProperty(ref WallusedValue, value);
        }

        private decimal? WallusedValue = null;

        public decimal? NOMINAL_BURST
        {
            get => NominalburstValue;
            set => SetProperty(ref NominalburstValue, value);
        }

        private decimal? NominalburstValue = null;

        public decimal? NOMINAL_COLLAPSE
        {
            get => NominalcollapseValue;
            set => SetProperty(ref NominalcollapseValue, value);
        }

        private decimal? NominalcollapseValue = null;

        public decimal? NOMINAL_TENSION
        {
            get => NominaltensionValue;
            set => SetProperty(ref NominaltensionValue, value);
        }

        private decimal? NominaltensionValue = null;

        /// <summary>Burst at the wall used, psi.</summary>
        public decimal? BURST
        {
            get => BurstValue;
            set => SetProperty(ref BurstValue, value);
        }

        private decimal? BurstValue = null;

        /// <summary>Collapse at the wall used, psi.</summary>
        public decimal? COLLAPSE
        {
            get => CollapseValue;
            set => SetProperty(ref CollapseValue, value);
        }

        private decimal? CollapseValue = null;

        [MaxLength(32)]
        public string? COLLAPSE_REGIME
        {
            get => CollapseregimeValue;
            set => SetProperty(ref CollapseregimeValue, value);
        }

        private string? CollapseregimeValue = null;

        /// <summary>Tension at the wall used, lbf.</summary>
        public decimal? TENSION
        {
            get => TensionValue;
            set => SetProperty(ref TensionValue, value);
        }

        private decimal? TensionValue = null;

        public decimal? BURST_LOAD
        {
            get => BurstloadValue;
            set => SetProperty(ref BurstloadValue, value);
        }

        private decimal? BurstloadValue = null;

        public decimal? COLLAPSE_LOAD
        {
            get => CollapseloadValue;
            set => SetProperty(ref CollapseloadValue, value);
        }

        private decimal? CollapseloadValue = null;

        public decimal? TENSION_LOAD
        {
            get => TensionloadValue;
            set => SetProperty(ref TensionloadValue, value);
        }

        private decimal? TensionloadValue = null;

        /// <summary>Y when the joint's re-rating meets every design factor, N when it does not, null when it could not be rated.</summary>
        [MaxLength(1)]
        public string? PASSES
        {
            get => PassesValue;
            set => SetProperty(ref PassesValue, value);
        }

        private string? PassesValue = null;

        /// <summary>What could not be computed for this joint, one note per line.</summary>
        [MaxLength(1000)]
        public string? NOTES
        {
            get => NotesValue;
            set => SetProperty(ref NotesValue, value);
        }

        private string? NotesValue = null;

    }
}
