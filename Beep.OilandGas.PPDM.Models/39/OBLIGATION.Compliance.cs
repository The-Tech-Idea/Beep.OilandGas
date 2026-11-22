using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.PPDM39.Models
{
    /// <summary>
    /// The compliance columns of an obligation (HSE-05): the field it belongs to and the jurisdiction it is filed in.
    /// </summary>
    /// <remarks>
    /// The PPDM obligation carried no field and no jurisdiction of its own - the owner's compliance service filters on a
    /// <c>FIELD_ID</c> it assumes exists and reads a <c>JURISDICTION_CODE</c> the table does not have, so those reads
    /// silently produce blanks. Both are additive columns here, so an obligation can be scoped and its jurisdiction stated
    /// explicitly rather than defaulted.
    /// </remarks>
    public partial class OBLIGATION
    {
        /// <summary>The field the obligation belongs to, when it is a field obligation.</summary>
        [MaxLength(128)]
        public string? FIELD_ID
        {
            get => FieldIdValue;
            set => SetProperty(ref FieldIdValue, value);
        }

        private string? FieldIdValue;

        /// <summary>The jurisdiction that imposes the obligation - always explicit, never defaulted to a country.</summary>
        [MaxLength(64)]
        public string? JURISDICTION
        {
            get => JurisdictionValue;
            set => SetProperty(ref JurisdictionValue, value);
        }

        private string? JurisdictionValue;
    }
}
