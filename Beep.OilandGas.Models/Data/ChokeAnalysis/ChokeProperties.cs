using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Beep.OilandGas.Models.Data.ChokeAnalysis;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ChokeAnalysis
{
    /// <summary>
    /// Represents choke properties
    /// DTO for calculations - Entity class: CHOKE_PROPERTIES
    /// </summary>
    public partial class ChokeProperties : ModelEntityBase {
        /// <summary>
        /// Choke diameter in inches
        /// </summary>
        private decimal _chokeDiameterValue;
        public decimal ChokeDiameter
        {
            get { return _chokeDiameterValue; }
            set { SetProperty(ref _chokeDiameterValue, value); }
        }

        /// <summary>
        /// Choke type (bean, adjustable, etc.)
        /// </summary>
        private ChokeType _chokeTypeValue;
        public ChokeType ChokeType
        {
            get { return _chokeTypeValue; }
            set { SetProperty(ref _chokeTypeValue, value); }
        }

        /// <summary>
        /// Discharge coefficient
        /// </summary>
        private decimal _dischargeCoefficientValue = 0.85m;
        public decimal DischargeCoefficient
        {
            get { return _dischargeCoefficientValue; }
            set { SetProperty(ref _dischargeCoefficientValue, value); }
        }

        /// <summary>
        /// Choke area in square inches
        /// </summary>
        public decimal ChokeArea => (decimal)Math.PI * ChokeDiameter * ChokeDiameter / 4m;

     
    }
}


