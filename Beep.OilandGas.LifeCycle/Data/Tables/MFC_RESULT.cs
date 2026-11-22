using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.LifeCycle.Data.Tables
{
    /// One recorded multi-finger caliper interpretation (ENG-03): the log it came from, the rules it was run
    /// under and what it found.
    /// </summary>
    /// <remarks>
    /// The owner has no caliper result table at all - the analysis is computed in memory and lost - so the log
    /// reference, the damage classes and the wall basis that produced the joints had nowhere to live. Storing them
    /// with the run is what makes a wall-loss figure auditable later, when the settings have moved on.
    /// </remarks>
    public class MFC_RESULT : ModelEntityBase
    {
        [Key, MaxLength(64)]
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

        /// <summary>The caliper log set's name as the file records it (AC-6).</summary>
        [MaxLength(256)]
        public string? LOG_NAME
        {
            get => LognameValue;
            set => SetProperty(ref LognameValue, value);
        }

        private string? LognameValue = null;

        /// <summary>The file the log was read from, so the same data can be re-read (AC-6).</summary>
        [MaxLength(512)]
        public string? LOG_REFERENCE
        {
            get => LogreferenceValue;
            set => SetProperty(ref LogreferenceValue, value);
        }

        private string? LogreferenceValue = null;

        /// <summary>How the joints were found: given, ccl or intervals.</summary>
        [MaxLength(32)]
        public string? JOINT_SOURCE
        {
            get => JointsourceValue;
            set => SetProperty(ref JointsourceValue, value);
        }

        private string? JointsourceValue = null;

        /// <summary>Finger curves the analysis used.</summary>
        [MaxLength(512)]
        public string? FINGERS
        {
            get => FingersValue;
            set => SetProperty(ref FingersValue, value);
        }

        private string? FingersValue = null;

        public int JOINT_COUNT
        {
            get => JointcountValue;
            set => SetProperty(ref JointcountValue, value);
        }

        private int JointcountValue = 0;

        /// <summary>minimumWall or averageWall - the owner's integrity.wallBasis at the time.</summary>
        [MaxLength(32)]
        public string? WALL_BASIS
        {
            get => WallbasisValue;
            set => SetProperty(ref WallbasisValue, value);
        }

        private string? WallbasisValue = null;

        /// <summary>The damage classes applied, as JSON, because they are company practice and can change.</summary>
        [MaxLength(2000)]
        public string? DAMAGE_CLASSES_JSON
        {
            get => DamageclassesjsonValue;
            set => SetProperty(ref DamageclassesjsonValue, value);
        }

        private string? DamageclassesjsonValue = null;

        /// <summary>The mud density the re-rating used, in ppg.</summary>
        public decimal FLUID_GRADIENT_PPG
        {
            get => FluidgradientppgValue;
            set => SetProperty(ref FluidgradientppgValue, value);
        }

        private decimal FluidgradientppgValue = 0m;

        /// <summary>The surface test pressure the re-rating used, in psi.</summary>
        public decimal TEST_PRESSURE_PSI
        {
            get => TestpressurepsiValue;
            set => SetProperty(ref TestpressurepsiValue, value);
        }

        private decimal TestpressurepsiValue = 0m;

        /// <summary>The joint with the deepest penetration.</summary>
        [MaxLength(64)]
        public string? WORST_JOINT
        {
            get => WorstjointValue;
            set => SetProperty(ref WorstjointValue, value);
        }

        private string? WorstjointValue = null;

        /// <summary>The depth of the worst penetration, in the recorded depth unit.</summary>
        public decimal? WORST_DEPTH
        {
            get => WorstdepthValue;
            set => SetProperty(ref WorstdepthValue, value);
        }

        private decimal? WorstdepthValue = null;

        /// <summary>The worst penetration as a fraction of the nominal wall.</summary>
        public decimal WORST_PENETRATION
        {
            get => WorstpenetrationValue;
            set => SetProperty(ref WorstpenetrationValue, value);
        }

        private decimal WorstpenetrationValue = 0m;

        [MaxLength(64)]
        public string? WORST_DAMAGE_CLASS
        {
            get => WorstdamageclassValue;
            set => SetProperty(ref WorstdamageclassValue, value);
        }

        private string? WorstdamageclassValue = null;

        /// <summary>Joints whose re-rating failed a design factor.</summary>
        public int FAILED_JOINTS
        {
            get => FailedjointsValue;
            set => SetProperty(ref FailedjointsValue, value);
        }

        private int FailedjointsValue = 0;

        [MaxLength(16)]
        public string? DEPTH_OUOM
        {
            get => DepthouomValue;
            set => SetProperty(ref DepthouomValue, value);
        }

        private string? DepthouomValue = null;

        [MaxLength(128)]
        public string? CHECKED_BY
        {
            get => CheckedbyValue;
            set => SetProperty(ref CheckedbyValue, value);
        }

        private string? CheckedbyValue = null;

        public DateTime CHECKED_AT_UTC
        {
            get => CheckedatutcValue;
            set => SetProperty(ref CheckedatutcValue, value);
        }

        private DateTime CheckedatutcValue = DateTime.UtcNow;

    }
}
