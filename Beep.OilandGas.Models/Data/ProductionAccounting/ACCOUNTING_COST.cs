using System.Runtime.CompilerServices;
using Beep.OilandGas.Models.Data.ProductionAccounting;

// Preserve existing consumers while the shared domain contract is owned by TheTechIdea.Data.
[assembly: TypeForwardedTo(typeof(ACCOUNTING_COST))]
