using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// A reservation of stock for a document (ACC-06): the maintenance work order or cost centre that is holding a
    /// quantity, so availability is a fact rather than a number nobody can explain.
    /// </summary>
    /// <remarks>
    /// A reservation is not an <c>INVENTORY_TRANSACTION</c>: it moves no quantity, so writing it as a movement would either
    /// fake a zero-quantity row or, worse, a real deduction. It is its own record, and the quantity held for an item is the
    /// sum of its open reservations - never a scalar on the item that could drift from them. AC-3 requires the holder to be
    /// named, so <see cref="HOLDING_DOCUMENT"/> is required and is what the item grid's "held by" line prints.
    /// </remarks>
    public class INVENTORY_RESERVATION : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string INVENTORY_RESERVATION_ID
        {
            get => ReservationIdValue;
            set => SetProperty(ref ReservationIdValue, value);
        }

        private string ReservationIdValue = string.Empty;

        [MaxLength(64)]
        public string INVENTORY_ITEM_ID
        {
            get => ItemValue;
            set => SetProperty(ref ItemValue, value);
        }

        private string ItemValue = string.Empty;

        public decimal QUANTITY
        {
            get => QuantityValue;
            set => SetProperty(ref QuantityValue, value);
        }

        private decimal QuantityValue;

        /// <summary>The work order, cost centre or request holding the stock (AC-3).</summary>
        [MaxLength(128)]
        public string HOLDING_DOCUMENT
        {
            get => HolderValue;
            set => SetProperty(ref HolderValue, value);
        }

        private string HolderValue = string.Empty;

        /// <summary>The document's own type, so a holder is never a bare string.</summary>
        [MaxLength(32)]
        public string? HOLDING_KIND
        {
            get => HoldingKindValue;
            set => SetProperty(ref HoldingKindValue, value);
        }

        private string? HoldingKindValue;

        public DateTime? RESERVED_AT
        {
            get => ReservedAtValue;
            set => SetProperty(ref ReservedAtValue, value);
        }

        private DateTime? ReservedAtValue;

        [MaxLength(128)]
        public string? ACTOR
        {
            get => ActorValue;
            set => SetProperty(ref ActorValue, value);
        }

        private string? ActorValue;

        /// <summary>OPEN, ISSUED or RELEASED - from WarehouseCodes. Only OPEN holds stock.</summary>
        [MaxLength(16)]
        public string STATUS
        {
            get => StatusValue;
            set => SetProperty(ref StatusValue, value);
        }

        private string StatusValue = "OPEN";

        /// <summary>The issue that spent this reservation, once it was issued.</summary>
        [MaxLength(64)]
        public string? ISSUED_TRANSACTION_ID
        {
            get => IssuedValue;
            set => SetProperty(ref IssuedValue, value);
        }

        private string? IssuedValue;

        [MaxLength(128)]
        public string? RELEASED_BY
        {
            get => ReleasedByValue;
            set => SetProperty(ref ReleasedByValue, value);
        }

        private string? ReleasedByValue;

        public DateTime? RELEASED_AT
        {
            get => ReleasedAtValue;
            set => SetProperty(ref ReleasedAtValue, value);
        }

        private DateTime? ReleasedAtValue;

        [MaxLength(500)]
        public string? RELEASE_REASON
        {
            get => ReleaseReasonValue;
            set => SetProperty(ref ReleaseReasonValue, value);
        }

        private string? ReleaseReasonValue;
    }
}
