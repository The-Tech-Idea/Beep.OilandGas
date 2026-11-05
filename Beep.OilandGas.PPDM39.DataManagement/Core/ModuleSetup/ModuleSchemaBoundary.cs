using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;

public static class ModuleSchemaBoundary
{
    private static readonly Type[] FinancialReadContracts =
    [
        typeof(Beep.OilandGas.Models.Data.Accounting.CreateInvoiceRequest),
        typeof(Beep.OilandGas.Models.Data.Accounting.UpdateInvoiceRequest),
        typeof(Beep.OilandGas.Models.Data.Accounting.CreateInvoicePaymentRequest),
        typeof(Beep.OilandGas.Models.Data.Accounting.InvoiceApprovalResult),
        typeof(Beep.OilandGas.Models.Data.Accounting.InvoiceAgingSummary),
        typeof(Beep.OilandGas.Models.Data.Accounting.InvoicePaymentStatus),
        typeof(Beep.OilandGas.Models.Data.Accounting.CreateARInvoiceRequest),
        typeof(Beep.OilandGas.Models.Data.Accounting.UpdateARInvoiceRequest),
        typeof(Beep.OilandGas.Models.Data.Accounting.CreateARPaymentRequest),
        typeof(Beep.OilandGas.Models.Data.Accounting.CreateARCreditMemoRequest),
        typeof(Beep.OilandGas.Models.Data.Accounting.ARApprovalResult),
        typeof(Beep.OilandGas.Models.Data.Accounting.ARAgingSummary),
        typeof(Beep.OilandGas.Models.Data.Accounting.CustomerSummary),
        typeof(Beep.OilandGas.Models.Data.Accounting.PaymentApplicationItem),
        typeof(Beep.OilandGas.Models.Data.Accounting.PaymentApplicationRequest),
        typeof(Beep.OilandGas.Models.Data.Accounting.PaymentApplicationResult),
        typeof(Beep.OilandGas.Models.Data.Accounting.ARInvoice),
        typeof(Beep.OilandGas.Models.Data.Accounting.ARInvoiceResponse),
        typeof(Beep.OilandGas.Models.Data.Accounting.CreateGLAccountRequest),
        typeof(Beep.OilandGas.Models.Data.Accounting.UpdateGLAccountRequest),
        typeof(Beep.OilandGas.Models.Data.Accounting.GLAccount),
        typeof(Beep.OilandGas.Models.Data.Accounting.GLAccountResponse),
        typeof(Beep.OilandGas.Models.Data.Accounting.CreateJournalEntryRequest),
        typeof(Beep.OilandGas.Models.Data.Accounting.JournalEntryLineData),
        typeof(Beep.OilandGas.Models.Data.Accounting.JournalEntry),
        typeof(Beep.OilandGas.Models.Data.Accounting.JournalEntryLine),
        typeof(Beep.OilandGas.Models.Data.Accounting.JournalEntryApprovalResult),
        typeof(JournalPostingEvidence)
    ];
    private static readonly string[] RepositoryNamespaces =
    {
        "Beep.OilandGas.Models.Data.Security",
        "Beep.OilandGas.UserManagement.Models.Identity",
        "Microsoft.AspNetCore.Identity"
    };

    public static void Validate(IEnumerable<Type> entityTypes)
    {
        if (entityTypes.Any(type =>
            FinancialReadContracts.Contains(type) ||
            (type.Namespace=="TheTechIdea.Data.OilGas" || (type.Namespace?.StartsWith("TheTechIdea.Data.OilGas.",StringComparison.Ordinal) ?? false)) &&
            (!typeof(Beep.OilandGas.Models.Data.ModelEntityBase).IsAssignableFrom(type) || !type.GetProperties().Any(property=>property.IsDefined(typeof(System.ComponentModel.DataAnnotations.KeyAttribute),true))) ||
            RepositoryNamespaces.Any(ns =>
            string.Equals(type.Namespace, ns, StringComparison.Ordinal) ||
            (type.Namespace?.StartsWith(ns + ".", StringComparison.Ordinal) ?? false))))
            throw new ArgumentException("Security and Identity extensions belong to the default EF repository and cannot be migrated into a module database.");
    }
}
