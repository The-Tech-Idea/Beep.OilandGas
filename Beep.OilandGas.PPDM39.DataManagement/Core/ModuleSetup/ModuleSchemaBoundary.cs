using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;

public static class ModuleSchemaBoundary
{
    // Policy names avoid a dependency from setup infrastructure onto feature assemblies.
    private static readonly HashSet<string> FinancialReadContractNames = new(StringComparer.Ordinal)
    {
        "CreateInvoiceRequest",
        "UpdateInvoiceRequest",
        "CreateInvoicePaymentRequest",
        "InvoiceApprovalResult",
        "InvoiceAgingSummary",
        "InvoicePaymentStatus",
        "CreateARInvoiceRequest",
        "UpdateARInvoiceRequest",
        "CreateARPaymentRequest",
        "CreateARCreditMemoRequest",
        "ARApprovalResult",
        "ARAgingSummary",
        "CustomerSummary",
        "PaymentApplicationItem",
        "PaymentApplicationRequest",
        "PaymentApplicationResult",
        "ARInvoice",
        "ARInvoiceResponse",
        "CreateGLAccountRequest",
        "UpdateGLAccountRequest",
        "GLAccount",
        "GLAccountResponse",
        "CreateJournalEntryRequest",
        "JournalEntryLineData",
        "JournalEntry",
        "JournalEntryLine",
        "JournalEntryApprovalResult",
        "JournalPostingEvidence",
    };
    private static readonly string[] RepositoryNamespaces =
    {
        "Beep.OilandGas.Models.Data.Security",
        "Beep.OilandGas.Repository.Models",
        "Beep.OilandGas.UserManagement.Models.Identity",
        "Microsoft.AspNetCore.Identity"
    };

    public static void Validate(IEnumerable<Type> entityTypes)
    {
        if (entityTypes.Any(type =>
            (FinancialReadContractNames.Contains(type.Name) &&
             (type.Namespace == "Beep.OilandGas.Models.Data.Accounting" ||
              type.Namespace == "Beep.OilandGas.Accounting.Models" ||
              type.FullName == "Beep.OilandGas.Models.Data.ProductionAccounting.JournalPostingEvidence")) ||
            ApplicationContractSchemaPolicy.AppliesTo(type) &&
            (!typeof(Beep.OilandGas.Models.Data.ModelEntityBase).IsAssignableFrom(type) || !type.GetProperties().Any(property=>property.IsDefined(typeof(System.ComponentModel.DataAnnotations.KeyAttribute),true))) ||
            RepositoryNamespaces.Any(ns =>
            string.Equals(type.Namespace, ns, StringComparison.Ordinal) ||
            (type.Namespace?.StartsWith(ns + ".", StringComparison.Ordinal) ?? false))))
            throw new ArgumentException("Security and Identity extensions belong to the default EF repository and cannot be migrated into a module database.");
    }
}
