using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionOperations
{
    /// <summary>
    /// The reference set of intervention methods (PRD-06). The application ships no taxonomy of its own: the methods are
    /// the operating company's (or the lift vendor's) data, and an option whose method is not in this table is refused
    /// rather than accepted as free text - the same rule PRD-03 applies to deferment reasons.
    /// </summary>
    public class R_INTERVENTION_METHOD : ModelEntityBase
    {
        [Key, MaxLength(32)]
        public string METHOD_CODE
        {
            get => MethodCodeValue;
            set => SetProperty(ref MethodCodeValue, value);
        }

        private string MethodCodeValue = string.Empty;

        [MaxLength(128)]
        public string? METHOD_NAME
        {
            get => MethodNameValue;
            set => SetProperty(ref MethodNameValue, value);
        }

        private string? MethodNameValue;

        /// <summary>For example LIFT, STIMULATION, WORKOVER, RATE or INTEGRITY.</summary>
        [MaxLength(64)]
        public string? CATEGORY
        {
            get => CategoryValue;
            set => SetProperty(ref CategoryValue, value);
        }

        private string? CategoryValue;

        /// <summary>
        /// 'Y' when the method changes the artificial-lift arrangement, which is what makes the lift recommendation apply.
        /// </summary>
        [MaxLength(1)]
        public string LIFT_RELATED_IND
        {
            get => LiftRelatedIndValue;
            set => SetProperty(ref LiftRelatedIndValue, value);
        }

        private string LiftRelatedIndValue = "N";

        [MaxLength(1)]
        public string ACTIVE_IND
        {
            get => ActiveIndValue;
            set => SetProperty(ref ActiveIndValue, value);
        }

        private string ActiveIndValue = "Y";
    }
}
