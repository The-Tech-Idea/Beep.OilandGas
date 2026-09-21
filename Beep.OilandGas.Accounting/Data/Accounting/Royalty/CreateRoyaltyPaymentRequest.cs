using System;
namespace Beep.OilandGas.Models.Data.Accounting.Royalty;

public sealed record CreateRoyaltyPaymentRequest(Guid RequestId, decimal Amount);
