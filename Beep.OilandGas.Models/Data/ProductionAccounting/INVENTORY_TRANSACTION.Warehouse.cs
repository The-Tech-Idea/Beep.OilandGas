using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// The warehouse columns of a stock movement (ACC-06): the condition it arrived in, the restock decision taken on it,
    /// the reservation it spent, the ledger entry its value change produced, and who did it.
    /// </summary>
    /// <remarks>
    /// <c>INVENTORY_TRANSACTION</c> already held the type, the date, the quantity (negative for a deduction, which is the
    /// owner's own convention in <c>InventoryService.UseStockAsync</c>), the unit and total cost and a reference. It had no
    /// condition and no restock decision, so AC-4's "a return requires an explicit restock decision" had nowhere to be
    /// recorded, and no journal link, so AC-5's "stock value changes produce a linked journal" could not be traced. These
    /// columns are additive.
    /// </remarks>
    public partial class INVENTORY_TRANSACTION
    {
        /// <summary>NEW, SERVICEABLE, DAMAGED or SCRAP - how the goods actually arrived or came back.</summary>
        [MaxLength(16)]
        public string? CONDITION_CODE
        {
            get => ConditionValue;
            set => SetProperty(ref ConditionValue, value);
        }

        private string? ConditionValue;

        /// <summary>RESTOCK, SCRAP or REPAIR - the decision a return or a damaged receipt had to state (AC-4).</summary>
        [MaxLength(16)]
        public string? RESTOCK_DECISION
        {
            get => DecisionValue;
            set => SetProperty(ref DecisionValue, value);
        }

        private string? DecisionValue;

        /// <summary>The reservation this issue spent, when it issued against one.</summary>
        [MaxLength(64)]
        public string? RESERVATION_ID
        {
            get => ReservationValue;
            set => SetProperty(ref ReservationValue, value);
        }

        private string? ReservationValue;

        /// <summary>
        /// The ledger entry this movement's value change produced (AC-5). Null when the movement changed no value, which is
        /// stated on the row's own text rather than left to be guessed from a zero.
        /// </summary>
        [MaxLength(64)]
        public string? JOURNAL_ENTRY_ID
        {
            get => JournalValue;
            set => SetProperty(ref JournalValue, value);
        }

        private string? JournalValue;

        /// <summary>Who moved the stock.</summary>
        [MaxLength(128)]
        public string? ACTOR
        {
            get => ActorValue;
            set => SetProperty(ref ActorValue, value);
        }

        private string? ActorValue;
    }
}
