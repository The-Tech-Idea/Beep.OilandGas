using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Inventory;
namespace Beep.OilandGas.ApiService.Controllers.Accounting.Traditional;
public partial class InventoryController
{
        /// <summary>
        /// Create an inventory transaction.
        /// </summary>
        [Microsoft.AspNetCore.Authorization.Authorize]
        [HttpPost("transactions")]
        public async Task<ActionResult<object>> CreateTransaction(
            [FromBody] CreateInventoryTransactionRequest request,
            [FromQuery] string connectionName = "PPDM39")
        {
            await Task.CompletedTask;
            return StatusCode(410,new{error="Companyless inventory creation with default ledger accounts is retired. Use the reviewed company receipt/stock workflow; financial posting requires its own authorized journal."});
        }

}
