using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// The warehouse columns of a stock item (ACC-06): what the item is linked to, so its cost lands on the equipment or
    /// asset it belongs to.
    /// </summary>
    /// <remarks>
    /// <c>INVENTORY_ITEM</c> already held the code, the name, the unit, the quantity on hand, the unit cost, the total value,
    /// the valuation method, the warehouse and the location. It had no link to an asset or equipment record, which is the
    /// feature's "link panel", so those two columns are added rather than a link table for one pair of values.
    /// </remarks>
    public partial class INVENTORY_ITEM
    {
        /// <summary>The kind of record this item is linked to - ASSET, EQUIPMENT or WELL, as the operator states it.</summary>
        [MaxLength(32)]
        public string? LINKED_ASSET_TYPE
        {
            get => LinkTypeValue;
            set => SetProperty(ref LinkTypeValue, value);
        }

        private string? LinkTypeValue;

        /// <summary>The asset or equipment id this item is linked to.</summary>
        [MaxLength(128)]
        public string? LINKED_ASSET_ID
        {
            get => LinkIdValue;
            set => SetProperty(ref LinkIdValue, value);
        }

        private string? LinkIdValue;
    }
}
