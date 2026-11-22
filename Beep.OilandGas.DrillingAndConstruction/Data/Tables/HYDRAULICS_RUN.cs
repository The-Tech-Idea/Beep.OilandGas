using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DrillingAndConstruction
{
    /// One recorded hydraulics run (ENG-08): the mud rheology, the circulating rate and the flow sections it was run with,
    /// and the standpipe pressure, annular loss and equivalent circulating density it produced.
    /// </summary>
    /// <remarks>
    /// The tool computes the circulating system in memory and keeps nothing, so a predicted ECD could not be compared with the
    /// next run or with a measured one. The run keeps the two Fann readings as well as their derived PV, YP, n and K, so a change
    /// to the model or to the settings cannot rewrite what a past run concluded. The sections and the nozzles are stored as the
    /// records they are, and the profile is recomputed from them rather than stored.
    /// </remarks>
    public class HYDRAULICS_RUN : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string RUN_ID
        {
            get => RunidValue;
            set => SetProperty(ref RunidValue, value);
        }

        private string RunidValue = string.Empty;

        [MaxLength(64)]
        public string UWI
        {
            get => UwiValue;
            set => SetProperty(ref UwiValue, value);
        }

        private string UwiValue = string.Empty;

        /// <summary>The saved well plan whose trajectory gave the bit TVD (ENG-06).</summary>
        [MaxLength(64)]
        public string? PLAN_ID
        {
            get => PlanidValue;
            set => SetProperty(ref PlanidValue, value);
        }

        private string? PlanidValue = null;

        /// <summary>binghamPlastic or powerLaw - the model the run used (AC-6).</summary>
        [MaxLength(32)]
        public string RHEOLOGY_MODEL
        {
            get => RheologymodelValue;
            set => SetProperty(ref RheologymodelValue, value);
        }

        private string RheologymodelValue = string.Empty;

        /// <summary>Mud density the run used (AC-6).</summary>
        public decimal MUD_DENSITY_PPG
        {
            get => MuddensityppgValue;
            set => SetProperty(ref MuddensityppgValue, value);
        }

        private decimal MuddensityppgValue = 0m;

        /// <summary>The 600 rpm Fann reading, kept so PV, YP, n and K can be reproduced.</summary>
        public decimal THETA600
        {
            get => Theta600Value;
            set => SetProperty(ref Theta600Value, value);
        }

        private decimal Theta600Value = 0m;

        /// <summary>The 300 rpm Fann reading.</summary>
        public decimal THETA300
        {
            get => Theta300Value;
            set => SetProperty(ref Theta300Value, value);
        }

        private decimal Theta300Value = 0m;

        /// <summary>PV in cP as the model derived it.</summary>
        public decimal PLASTIC_VISCOSITY
        {
            get => PlasticviscosityValue;
            set => SetProperty(ref PlasticviscosityValue, value);
        }

        private decimal PlasticviscosityValue = 0m;

        /// <summary>YP in lbf/100 ft2 as the model derived it.</summary>
        public decimal YIELD_POINT
        {
            get => YieldpointValue;
            set => SetProperty(ref YieldpointValue, value);
        }

        private decimal YieldpointValue = 0m;

        /// <summary>Power-law n as the model derived it.</summary>
        public decimal FLOW_INDEX
        {
            get => FlowindexValue;
            set => SetProperty(ref FlowindexValue, value);
        }

        private decimal FlowindexValue = 0m;

        /// <summary>Power-law K as the model derived it.</summary>
        public decimal CONSISTENCY_INDEX
        {
            get => ConsistencyindexValue;
            set => SetProperty(ref ConsistencyindexValue, value);
        }

        private decimal ConsistencyindexValue = 0m;

        /// <summary>Circulating rate the run used (AC-6).</summary>
        public decimal FLOW_RATE
        {
            get => FlowrateValue;
            set => SetProperty(ref FlowrateValue, value);
        }

        private decimal FlowrateValue = 0m;

        [MaxLength(16)]
        public string? FLOW_RATE_OUOM
        {
            get => FlowrateouomValue;
            set => SetProperty(ref FlowrateouomValue, value);
        }

        private string? FlowrateouomValue = null;

        /// <summary>Surface equipment loss given to the model.</summary>
        public decimal SURFACE_LOSS_PSI
        {
            get => SurfacelosspsiValue;
            set => SetProperty(ref SurfacelosspsiValue, value);
        }

        private decimal SurfacelosspsiValue = 0m;

        /// <summary>The bit's true vertical depth, which the ECD is quoted at.</summary>
        public decimal? BIT_TVD
        {
            get => BittvdValue;
            set => SetProperty(ref BittvdValue, value);
        }

        private decimal? BittvdValue = null;

        [MaxLength(16)]
        public string? BIT_TVD_OUOM
        {
            get => BittvdouomValue;
            set => SetProperty(ref BittvdouomValue, value);
        }

        private string? BittvdouomValue = null;

        /// <summary>The nozzle sizes in 32nds of an inch, as recorded (AC-4).</summary>
        [MaxLength(512)]
        public string? NOZZLES_TEXT
        {
            get => NozzlestextValue;
            set => SetProperty(ref NozzlestextValue, value);
        }

        private string? NozzlestextValue = null;

        public int NOZZLE_COUNT
        {
            get => NozzlecountValue;
            set => SetProperty(ref NozzlecountValue, value);
        }

        private int NozzlecountValue = 0;

        /// <summary>The flow sections as recorded: name, top MD, bottom MD, hole or pipe ID, pipe OD or none.</summary>
        [MaxLength(2000)]
        public string? SECTIONS_TEXT
        {
            get => SectionstextValue;
            set => SetProperty(ref SectionstextValue, value);
        }

        private string? SectionstextValue = null;

        /// <summary>How many sections the run produced.</summary>
        public int SECTION_COUNT
        {
            get => SectioncountValue;
            set => SetProperty(ref SectioncountValue, value);
        }

        private int SectioncountValue = 0;

        /// <summary>Surface + string + bit + annulus, psi.</summary>
        public decimal? STANDPIPE_PRESSURE
        {
            get => StandpipepressureValue;
            set => SetProperty(ref StandpipepressureValue, value);
        }

        private decimal? StandpipepressureValue = null;

        /// <summary>Annular pressure loss, psi.</summary>
        public decimal? ANNULAR_LOSS
        {
            get => AnnularlossValue;
            set => SetProperty(ref AnnularlossValue, value);
        }

        private decimal? AnnularlossValue = null;

        /// <summary>Equivalent circulating density at the bit TVD, ppg.</summary>
        public decimal? ECD
        {
            get => EcdValue;
            set => SetProperty(ref EcdValue, value);
        }

        private decimal? EcdValue = null;

        /// <summary>Bit pressure drop, psi.</summary>
        public decimal? BIT_LOSS
        {
            get => BitlossValue;
            set => SetProperty(ref BitlossValue, value);
        }

        private decimal? BitlossValue = null;

        /// <summary>Bit hydraulic horsepower.</summary>
        public decimal? BIT_HHP
        {
            get => BithhpValue;
            set => SetProperty(ref BithhpValue, value);
        }

        private decimal? BithhpValue = null;

        /// <summary>Jet impact force, lbf.</summary>
        public decimal? IMPACT_FORCE
        {
            get => ImpactforceValue;
            set => SetProperty(ref ImpactforceValue, value);
        }

        private decimal? ImpactforceValue = null;

        /// <summary>The highest ECD anywhere on the profile, ppg.</summary>
        public decimal? MAX_ECD
        {
            get => MaxecdValue;
            set => SetProperty(ref MaxecdValue, value);
        }

        private decimal? MaxecdValue = null;

        /// <summary>Where that ECD occurs.</summary>
        public decimal? MAX_ECD_DEPTH
        {
            get => MaxecddepthValue;
            set => SetProperty(ref MaxecddepthValue, value);
        }

        private decimal? MaxecddepthValue = null;

        /// <summary>The recorded fracture gradient the profile was compared with, in ppg equivalent, when one is recorded.</summary>
        public decimal? FRACTURE_GRADIENT
        {
            get => FracturegradientValue;
            set => SetProperty(ref FracturegradientValue, value);
        }

        private decimal? FracturegradientValue = null;

        /// <summary>The shoe depth the gradient belongs to.</summary>
        public decimal? FRACTURE_DEPTH
        {
            get => FracturedepthValue;
            set => SetProperty(ref FracturedepthValue, value);
        }

        private decimal? FracturedepthValue = null;

        /// <summary>Y when the profile is above the recorded fracture gradient (AC-3).</summary>
        [MaxLength(1)]
        public string? FRACTURE_EXCEEDED
        {
            get => FractureexceededValue;
            set => SetProperty(ref FractureexceededValue, value);
        }

        private string? FractureexceededValue = null;

        /// <summary>The critical sentence, with the depth, when the fracture gradient is exceeded.</summary>
        [MaxLength(500)]
        public string? CRITICAL_TEXT
        {
            get => CriticaltextValue;
            set => SetProperty(ref CriticaltextValue, value);
        }

        private string? CriticaltextValue = null;

        [MaxLength(16)]
        public string? DEPTH_OUOM
        {
            get => DepthouomValue;
            set => SetProperty(ref DepthouomValue, value);
        }

        private string? DepthouomValue = null;

        [MaxLength(128)]
        public string? RAN_BY
        {
            get => RanbyValue;
            set => SetProperty(ref RanbyValue, value);
        }

        private string? RanbyValue = null;

        public DateTime RAN_AT_UTC
        {
            get => RanatutcValue;
            set => SetProperty(ref RanatutcValue, value);
        }

        private DateTime RanatutcValue = DateTime.UtcNow;

    }
}
