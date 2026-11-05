# Beep.OilandGas.Accounting — data ownership restoration plan

<!-- ownership-current:begin -->
## Current implementation

50 original files now belong to this project. All 502 original Data sources have been moved/consolidated. Actual paths/namespaces below supersede the initial proposal tables. Current verification: all 39 direct framework projects build with the canonical analyzer and default BeepDM source dependencies; 41 selected caller tests, 88 ownership tests, 6 native financial contract tests and 127 selected financial behavior cases pass (only the 201-write history case excluded). Compiled ownership/serialization/permit metadata checks pass; all 1,200 restored declarations are present with no cross-project duplicates. These are scoped checks using existing dependency outputs. Follow-up relationship tests pass 58/58 and the internal 73-project graph has no literal-reference cycles. Canonical AccountManagement and ThemeBranding outputs now build. The final native fixture boundary gate passes 71/71, including all ten prior expanded-run failures and the explicit missing-projection refusal; a full expanded rerun is not claimed. Default repository tests pass 69 with 3 skips, and 22/25 top-level test projects compile. The isolated 201-write history gate passes 1/1. Nodal and Pipeline callers now pass 13 and 9 focused cases; Diagnostics-dependent host verification remains open. The nested company-environment consumer passes 65/65 against actual owners after removing eight moved-definition source links. Expanded access tests passed 587/594 before final repairs; all seven failures pass in the final 27-case targeted recheck. A fresh full 594-case rerun is not claimed. See evidence/canonical-dependency-recovery/observed-evidence.json.

| Original source | Current definition | Namespace |
| --- | --- | --- |
| `TheTechIdea.Data/OilGas/AccountingPeriodCheckReview.cs` | `Beep.OilandGas.Accounting/Models/AccountingPeriodCheckReview.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/APReceiptAllocationCommand.cs` | `Beep.OilandGas.Accounting/Models/APReceiptAllocationCommand.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/CompanyAccountingCorrectionCycle.cs` | `Beep.OilandGas.Accounting/Models/CompanyAccountingCorrectionCycle.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/CompanyGLAccountConfiguration.cs` | `Beep.OilandGas.Accounting/Models/CompanyGLAccountConfiguration.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/ARAgingSummary.cs` | `Beep.OilandGas.Accounting/Data/Accounting/ARAgingSummary.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/ARApprovalResult.cs` | `Beep.OilandGas.Accounting/Data/Accounting/ARApprovalResult.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/ARInvoiceResponse.cs` | `Beep.OilandGas.Accounting/Data/Accounting/ARInvoiceResponse.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/CreateARCreditMemoRequest.cs` | `Beep.OilandGas.Accounting/Data/Accounting/CreateARCreditMemoRequest.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/CreateARInvoiceRequest.cs` | `Beep.OilandGas.Accounting/Data/Accounting/CreateARInvoiceRequest.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/CreateARPaymentRequest.cs` | `Beep.OilandGas.Accounting/Data/Accounting/CreateARPaymentRequest.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/CreateGLAccountRequest.cs` | `Beep.OilandGas.Accounting/Data/Accounting/CreateGLAccountRequest.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/CreateInvoicePaymentRequest.cs` | `Beep.OilandGas.Accounting/Data/Accounting/CreateInvoicePaymentRequest.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/CreateInvoiceRequest.cs` | `Beep.OilandGas.Accounting/Data/Accounting/CreateInvoiceRequest.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/CreateJournalEntryRequest.cs` | `Beep.OilandGas.Accounting/Data/Accounting/CreateJournalEntryRequest.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/CustomerSummary.cs` | `Beep.OilandGas.Accounting/Data/Accounting/CustomerSummary.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/GLAccountResponse.cs` | `Beep.OilandGas.Accounting/Data/Accounting/GLAccountResponse.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/InvoiceAgingSummary.cs` | `Beep.OilandGas.Accounting/Data/Accounting/InvoiceAgingSummary.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/InvoiceApprovalResult.cs` | `Beep.OilandGas.Accounting/Data/Accounting/InvoiceApprovalResult.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/InvoicePaymentStatus.cs` | `Beep.OilandGas.Accounting/Data/Accounting/InvoicePaymentStatus.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/JournalEntry.cs` | `Beep.OilandGas.Accounting/Data/Accounting/JournalEntry.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/JournalEntryApprovalResult.cs` | `Beep.OilandGas.Accounting/Data/Accounting/JournalEntryApprovalResult.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/PaymentApplicationRequest.cs` | `Beep.OilandGas.Accounting/Data/Accounting/PaymentApplicationRequest.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/PaymentApplicationResult.cs` | `Beep.OilandGas.Accounting/Data/Accounting/PaymentApplicationResult.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/UpdateARInvoiceRequest.cs` | `Beep.OilandGas.Accounting/Data/Accounting/UpdateARInvoiceRequest.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/UpdateGLAccountRequest.cs` | `Beep.OilandGas.Accounting/Data/Accounting/UpdateGLAccountRequest.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/UpdateInvoiceRequest.cs` | `Beep.OilandGas.Accounting/Data/Accounting/UpdateInvoiceRequest.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/ARInvoice.cs` | `Beep.OilandGas.Accounting/Data/Accounting/ARInvoice.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/CompanyAccountingProcessBinding.cs` | `Beep.OilandGas.Accounting/Models/CompanyAccountingProcessBinding.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/CompanyGLMappingTransition.cs` | `Beep.OilandGas.Accounting/Models/CompanyGLMappingTransition.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/CompanyGLMappingConfiguration.cs` | `Beep.OilandGas.Accounting/Models/CompanyGLMappingConfiguration.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/CompanySalesInvoiceProcessSourceReview.cs` | `Beep.OilandGas.Accounting/Models/CompanySalesInvoiceProcessSourceReview.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/CompanySalesJournalProcessSourceReview.cs` | `Beep.OilandGas.Accounting/Models/CompanySalesJournalProcessSourceReview.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/CompanySalesPaymentWorkflowSource.cs` | `Beep.OilandGas.Accounting/Models/CompanySalesPaymentWorkflowSource.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/CompanySalesReceivableProcessSourceReview.cs` | `Beep.OilandGas.Accounting/Models/CompanySalesReceivableProcessSourceReview.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/CompanySalesReceivableReviewRecovery.cs` | `Beep.OilandGas.Accounting/Models/CompanySalesReceivableReviewRecovery.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/GLAccount.cs` | `Beep.OilandGas.Accounting/Data/Accounting/GLAccount.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/ICompanyLedgerBookReader.cs` | `Beep.OilandGas.Accounting/Models/ICompanyLedgerBookReader.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/JournalEntryLine.cs` | `Beep.OilandGas.Accounting/Data/Accounting/JournalEntryLine.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/JournalEntryLineData.cs` | `Beep.OilandGas.Accounting/Data/Accounting/JournalEntryLineData.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/PaymentApplicationItem.cs` | `Beep.OilandGas.Accounting/Data/Accounting/PaymentApplicationItem.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/PurchaseOrderInventoryLinkCycle.cs` | `Beep.OilandGas.Accounting/Models/PurchaseOrderInventoryLinkCycle.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/PurchaseOrderInventoryLinkResolution.cs` | `Beep.OilandGas.Accounting/Models/PurchaseOrderInventoryLinkResolution.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/PurchaseReceiptDecisionReview.cs` | `Beep.OilandGas.Accounting/Models/PurchaseReceiptDecisionReview.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/PurchaseReceiptStockIntentPage.cs` | `Beep.OilandGas.Accounting/Models/PurchaseReceiptStockIntentPage.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/CompanySalesJournalWorkflowSource.cs` | `Beep.OilandGas.Accounting/Models/CompanySalesJournalWorkflowSource.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/CompanySalesPaymentReview.cs` | `Beep.OilandGas.Accounting/Models/CompanySalesPaymentReview.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/CompanySalesPayment.cs` | `Beep.OilandGas.Accounting/Models/CompanySalesPayment.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/PurchaseReceiptStockIntentIndex.cs` | `Beep.OilandGas.Accounting/Models/PurchaseReceiptStockIntentIndex.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/SALES_JOURNAL_POSTING_OUTCOME.cs` | `Beep.OilandGas.Accounting/Models/SALES_JOURNAL_POSTING_OUTCOME.cs` | `Beep.OilandGas.Accounting.Models` |
| `TheTechIdea.Data/OilGas/CompanySalesPostedLedgerReview.cs` | `Beep.OilandGas.Accounting/Models/CompanySalesPostedLedgerReview.cs` | `Beep.OilandGas.Accounting.Models` |

Shared contract/native partial groups use the existing Models project where lower-level consumers require them. Feature implementations remain in their existing projects. There is no replacement central data project.
<!-- ownership-current:end -->

Date: 2026-11-04. Status: Implementation applied; final owner/caller verification in progress.

Project: `Beep.OilandGas.Accounting/Beep.OilandGas.Accounting.csproj`. Accountable: repository owner. Responsible: implementing engineer; affected module maintainer reviews ownership and behavior.

## Scope and ownership

30 candidate incoming files; 249 caller files contain affected identifiers. Counts are lexical evidence; resolve same-name types semantically before edits.

Use this existing project for its own domain types. Domain rules/helpers travel with the owning feature. API transport types shared across layers need an existing dependency-safe contract owner; do not move a shared DTO into a concrete service assembly if that creates a cycle. Keep unresolved mappings open in inventory.json.

## Incoming files and proposed names

| Source | Current namespace / types | Historical same-name candidates | Naming and destination action |
| --- | --- | --- | --- |
| `TheTechIdea.Data/OilGas/AP_INVOICE_LINE.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting`: AP_INVOICE_LINE | Not established; proposed owner needs semantic/history review | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/AP_RECEIPT_ALLOCATION.cs` | `Beep.OilandGas.Models.Data.ProductionAccounting`: AP_RECEIPT_ALLOCATION | Not established; proposed owner needs semantic/history review | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/ARAgingSummary.cs` | `Beep.OilandGas.Models.Data.Accounting`: ARAgingSummary | Beep.OilandGas.Accounting/Data/Accounting/ARAgingSummary.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/ARApprovalResult.cs` | `Beep.OilandGas.Models.Data.Accounting`: ARApprovalResult | Beep.OilandGas.Accounting/Data/Accounting/ARApprovalResult.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/ARInvoice.cs` | `Beep.OilandGas.Models.Data.Accounting`: ARInvoice | Beep.OilandGas.Accounting/Data/Accounting/ARInvoice.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/ARInvoiceResponse.cs` | `Beep.OilandGas.Models.Data.Accounting`: ARInvoiceResponse | Beep.OilandGas.Accounting/Data/Accounting/ARInvoiceResponse.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/CreateARCreditMemoRequest.cs` | `Beep.OilandGas.Models.Data.Accounting`: CreateARCreditMemoRequest | Beep.OilandGas.Accounting/Data/Accounting/CreateARCreditMemoRequest.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/CreateARInvoiceRequest.cs` | `Beep.OilandGas.Models.Data.Accounting`: CreateARInvoiceRequest | Beep.OilandGas.Accounting/Data/Accounting/CreateARInvoiceRequest.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/CreateARPaymentRequest.cs` | `Beep.OilandGas.Models.Data.Accounting`: CreateARPaymentRequest | Beep.OilandGas.Accounting/Data/Accounting/CreateARPaymentRequest.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/CreateGLAccountRequest.cs` | `Beep.OilandGas.Models.Data.Accounting`: CreateGLAccountRequest | Beep.OilandGas.Accounting/Data/Accounting/CreateGLAccountRequest.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/CreateInvoicePaymentRequest.cs` | `Beep.OilandGas.Models.Data.Accounting`: CreateInvoicePaymentRequest | Beep.OilandGas.Accounting/Data/Accounting/CreateInvoicePaymentRequest.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/CreateInvoiceRequest.cs` | `Beep.OilandGas.Models.Data.Accounting`: CreateInvoiceRequest | Beep.OilandGas.Accounting/Data/Accounting/CreateInvoiceRequest.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/CreateJournalEntryRequest.cs` | `Beep.OilandGas.Models.Data.Accounting`: CreateJournalEntryRequest | Beep.OilandGas.Accounting/Data/Accounting/CreateJournalEntryRequest.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/CustomerSummary.cs` | `Beep.OilandGas.Models.Data.Accounting`: CustomerSummary | Beep.OilandGas.Accounting/Data/Accounting/CustomerSummary.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/GLAccount.cs` | `Beep.OilandGas.Models.Data.Accounting`: GLAccount | Beep.OilandGas.Accounting/Data/Accounting/GLAccount.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/GLAccountMapping.cs` | `Beep.OilandGas.Accounting.Models`: GLAccountMapping | Beep.OilandGas.Accounting/Models/GLAccountMapping.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/GLAccountResponse.cs` | `Beep.OilandGas.Models.Data.Accounting`: GLAccountResponse | Beep.OilandGas.Accounting/Data/Accounting/GLAccountResponse.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/InvoiceAgingSummary.cs` | `Beep.OilandGas.Models.Data.Accounting`: InvoiceAgingSummary | Beep.OilandGas.Accounting/Data/Accounting/InvoiceAgingSummary.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/InvoiceApprovalResult.cs` | `Beep.OilandGas.Models.Data.Accounting`: InvoiceApprovalResult | Beep.OilandGas.Accounting/Data/Accounting/InvoiceApprovalResult.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/InvoicePaymentStatus.cs` | `Beep.OilandGas.Models.Data.Accounting`: InvoicePaymentStatus | Beep.OilandGas.Accounting/Data/Accounting/InvoicePaymentStatus.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/JournalEntry.cs` | `Beep.OilandGas.Models.Data.Accounting`: JournalEntry | Beep.OilandGas.Accounting/Data/Accounting/JournalEntry.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/JournalEntryApprovalResult.cs` | `Beep.OilandGas.Models.Data.Accounting`: JournalEntryApprovalResult | Beep.OilandGas.Accounting/Data/Accounting/JournalEntryApprovalResult.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/JournalEntryLine.cs` | `Beep.OilandGas.Models.Data.Accounting`: JournalEntryLine | Beep.OilandGas.Accounting/Data/Accounting/JournalEntryLine.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/JournalEntryLineData.cs` | `Beep.OilandGas.Models.Data.Accounting`: JournalEntryLineData | Beep.OilandGas.Accounting/Data/Accounting/JournalEntryLineData.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/PaymentApplicationItem.cs` | `Beep.OilandGas.Models.Data.Accounting`: PaymentApplicationItem | Beep.OilandGas.Accounting/Data/Accounting/PaymentApplicationItem.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/PaymentApplicationRequest.cs` | `Beep.OilandGas.Models.Data.Accounting`: PaymentApplicationRequest | Beep.OilandGas.Accounting/Data/Accounting/PaymentApplicationRequest.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/PaymentApplicationResult.cs` | `Beep.OilandGas.Models.Data.Accounting`: PaymentApplicationResult | Beep.OilandGas.Accounting/Data/Accounting/PaymentApplicationResult.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/UpdateARInvoiceRequest.cs` | `Beep.OilandGas.Models.Data.Accounting`: UpdateARInvoiceRequest | Beep.OilandGas.Accounting/Data/Accounting/UpdateARInvoiceRequest.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/UpdateGLAccountRequest.cs` | `Beep.OilandGas.Models.Data.Accounting`: UpdateGLAccountRequest | Beep.OilandGas.Accounting/Data/Accounting/UpdateGLAccountRequest.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |
| `TheTechIdea.Data/OilGas/UpdateInvoiceRequest.cs` | `Beep.OilandGas.Models.Data.Accounting`: UpdateInvoiceRequest | Beep.OilandGas.Accounting/Data/Accounting/UpdateInvoiceRequest.cs | Retain native persisted entity/table identifiers; use PascalCase for CLR-only DTO/rule names. Choose namespace from target project conventions; remove TheTechIdea.Data namespace. Split unrelated declarations into existing feature folders. |

## Calling code to update

The following files reference affected identifiers. For each, resolve its exact symbol against inventory.json, then update imports, fully-qualified names, generic arguments, constructor/DI wiring and project references together. A lexical match is not proof of a dependency.

- `Beep.OilandGas.Accounting/Constants/DefaultGlAccounts.cs`
- `Beep.OilandGas.Accounting/Core/Interfaces/IARService.cs`
- `Beep.OilandGas.Accounting/Core/Interfaces/ICostAllocationService.cs`
- `Beep.OilandGas.Accounting/Core/Interfaces/ICostCenterService.cs`
- `Beep.OilandGas.Accounting/Core/Interfaces/IInventoryLcmService.cs`
- `Beep.OilandGas.Accounting/Core/Interfaces/IInvoiceService.cs`
- `Beep.OilandGas.Accounting/Core/Interfaces/IJournalEntryService.cs`
- `Beep.OilandGas.Accounting/Core/Interfaces/IPeriodClosingWorkflow.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/APAgingSummary.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/APApprovalResult.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/APInvoice.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/AccountBalanceSummary.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/AccountReconciliationResult.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/AgedItem.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/AgedItemsReport.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/AgingAnalysis.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/AgingBucket.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/AllocationBase.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/AllocationEntry.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/AmortizationRecord.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/AssetDepreciationSummary.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/AssetSummaryLine.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/BalanceSheet.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/BalanceSheetLine.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/BankReconciliation.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/Budget.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/BudgetForecast.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/BudgetLine.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/BudgetVarianceLine.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/BudgetVarianceReport.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/CashFlowLine.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/CashFlowStatement.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/CheckClearingAnalysis.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/ChecklistItem.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/ClosingChecklist.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/Cost/CostAllocationRequest.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/Cost/CreateCostTransactionRequest.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/CostAllocationResult.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/CostCenter.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/CostSummary.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/CostTransaction.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/CreateAPCreditMemoRequest.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/CreateAPInvoiceRequest.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/CreateAPPaymentRequest.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/CreateAmortizationRecordRequest.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/CreatePOReceiptRequest.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/CreatePurchaseOrderRequest.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/CreateReceivableRequest.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/DeferralOpportunity.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/DepartmentProfitability.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/DepartmentalProfitabilityReport.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/DepositInTransit.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/DepreciationEntry.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/ExecutiveSummary.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/FinancialDashboard.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/FixedAsset.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/ForecastItem.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/GLDetailReport.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/GLEntryLine.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/GLSummaryLine.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/GLSummaryReport.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/HighlightedMetric.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/IncomeStatement.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/IncomeStatementLine.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/IntercompanyReconciliation.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/InventoryItem.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/Invoice.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/KPI.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/OutstandingCheck.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/OutstandingItem.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/Ownership/OwnerRequest.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/Ownership/RegisterOwnershipInterestRequest.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/POApprovalResult.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/POStatusSummary.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/PaymentBatchRequest.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/PaymentBatchResult.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/PaymentTypeAnalysis.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/PeriodCloseReadiness.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/PeriodClosingEntry.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/PeriodClosingResult.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/PeriodClosingStatus.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/PeriodClosingValidation.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/PostingResult.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/PresentationPackage.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/Pricing/PriceIndexRequest.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/PricingDetail.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/PurchaseOrder.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/QuarterlyAdjustment.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/QuarterlyEstimatedTax.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/QuarterlyPayment.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/ReceivableResponse.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/ReconciliationHistory.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/ReconciliationSummary.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/ReconciliationVariance.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/Reporting/GenerateLeaseReportRequest.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/Revenue/CreateRevenueTransactionRequest.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/Revenue/RevenueAllocationRequest.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/Revenue/WorkingInterestRequest.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/RevenueTransaction.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/SalesApprovalResult.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/SalesReconciliationIssue.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/SalesReconciliationRequest.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/SalesReconciliationResult.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/SalesStatement.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/SalesSummary.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/SalesTransaction.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/SalesTransactionResponse.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/TaxCredit.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/TaxDeduction.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/TaxDeferralAnalysis.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/TaxProvision.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/TrendAnalysis.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/TrendLine.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/UnmatchedTransaction.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/UpdateAPInvoiceRequest.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/UpdatePurchaseOrderRequest.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/VendorSummary.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/VolumeDetail.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/YTDItem.cs`
- `Beep.OilandGas.Accounting/Data/Accounting/YTDPerformance.cs`
- `Beep.OilandGas.Accounting/Models/PeriodCloseChecklist.cs`
- `Beep.OilandGas.Accounting/Models/ReconciliationDashboardDTO.cs`
- `Beep.OilandGas.Accounting/Services/APInvoiceService.Correction.cs`
- `Beep.OilandGas.Accounting/Services/APInvoiceService.ReceiptAllocation.cs`
- `Beep.OilandGas.Accounting/Services/APInvoiceService.Variance.cs`
- `Beep.OilandGas.Accounting/Services/APInvoiceService.VarianceDecision.cs`
- `Beep.OilandGas.Accounting/Services/APInvoiceService.cs`
- `Beep.OilandGas.Accounting/Services/APPaymentService.cs`
- `Beep.OilandGas.Accounting/Services/ARService.CompanyDecision.cs`
- `Beep.OilandGas.Accounting/Services/ARService.CompanyJournalPosting.cs`
- `Beep.OilandGas.Accounting/Services/ARService.CompanyPaymentDraft.cs`
- `Beep.OilandGas.Accounting/Services/ARService.CompanyPaymentReview.cs`
- `Beep.OilandGas.Accounting/Services/ARService.CompanyPaymentWorkflow.cs`
- `Beep.OilandGas.Accounting/Services/ARService.CompanyPreparation.cs`
- `Beep.OilandGas.Accounting/Services/ARService.cs`
- `Beep.OilandGas.Accounting/Services/AccountMappingService.Company.cs`
- `Beep.OilandGas.Accounting/Services/AccountMappingService.CompanyConfiguration.cs`
- `Beep.OilandGas.Accounting/Services/AccountMappingService.CompanyTransition.cs`
- `Beep.OilandGas.Accounting/Services/AccountMappingService.cs`
- `Beep.OilandGas.Accounting/Services/AccountingBasisPostingService.cs`
- `Beep.OilandGas.Accounting/Services/AccountingPolicyService.CompanyBilling.cs`
- `Beep.OilandGas.Accounting/Services/AccountingPolicyService.CompanyMatching.cs`
- `Beep.OilandGas.Accounting/Services/AccountingPolicyService.CompanyMatchingCommands.cs`
- `Beep.OilandGas.Accounting/Services/AccountingPolicyService.cs`
- `Beep.OilandGas.Accounting/Services/AgricultureService.cs`
- `Beep.OilandGas.Accounting/Services/AnomalyDetectionService.cs`
- `Beep.OilandGas.Accounting/Services/AssetsHeldForSaleService.cs`
- `Beep.OilandGas.Accounting/Services/AuditService.cs`
- `Beep.OilandGas.Accounting/Services/BankReconciliationService.cs`
- `Beep.OilandGas.Accounting/Services/BorrowingCostService.cs`
- `Beep.OilandGas.Accounting/Services/BudgetService.cs`
- `Beep.OilandGas.Accounting/Services/BusinessCombinationService.cs`
- `Beep.OilandGas.Accounting/Services/CashFlowService.cs`
- `Beep.OilandGas.Accounting/Services/CeclService.cs`
- `Beep.OilandGas.Accounting/Services/ConsolidationService.cs`
- `Beep.OilandGas.Accounting/Services/CostAllocationService.cs`
- `Beep.OilandGas.Accounting/Services/CostCenterService.cs`
- `Beep.OilandGas.Accounting/Services/CurrencyTranslationService.cs`
- `Beep.OilandGas.Accounting/Services/DashboardService.cs`
- `Beep.OilandGas.Accounting/Services/DepreciationService.cs`
- `Beep.OilandGas.Accounting/Services/EarningsPerShareService.cs`
- `Beep.OilandGas.Accounting/Services/EmployeeBenefitsService.cs`
- `Beep.OilandGas.Accounting/Services/EquityMethodInvestmentService.cs`
- `Beep.OilandGas.Accounting/Services/ErrorCorrectionService.cs`
- `Beep.OilandGas.Accounting/Services/EventsAfterReportingService.cs`
- `Beep.OilandGas.Accounting/Services/ExpectedCreditLossService.cs`
- `Beep.OilandGas.Accounting/Services/ExplorationEvaluationService.cs`
- `Beep.OilandGas.Accounting/Services/FairValueMeasurementService.cs`
- `Beep.OilandGas.Accounting/Services/FinancialFactTableService.cs`
- `Beep.OilandGas.Accounting/Services/FinancialInstrumentDisclosureService.cs`
- `Beep.OilandGas.Accounting/Services/FinancialInstrumentService.cs`
- `Beep.OilandGas.Accounting/Services/FinancialStatementService.cs`
- `Beep.OilandGas.Accounting/Services/FirstTimeAdoptionService.cs`
- `Beep.OilandGas.Accounting/Services/FixedAssetLifecycleService.cs`
- `Beep.OilandGas.Accounting/Services/GLAccountService.Company.cs`
- `Beep.OilandGas.Accounting/Services/GLAccountService.CompanyBooks.cs`
- `Beep.OilandGas.Accounting/Services/GLAccountService.CompanyConfiguration.cs`
- `Beep.OilandGas.Accounting/Services/GLAccountService.CompanyFx.cs`
- `Beep.OilandGas.Accounting/Services/GLAccountService.cs`
- `Beep.OilandGas.Accounting/Services/GLIntegrationService.cs`
- `Beep.OilandGas.Accounting/Services/GaapAccountMappingService.cs`
- `Beep.OilandGas.Accounting/Services/GaapLeaseAccountingService.cs`
- `Beep.OilandGas.Accounting/Services/GaapRevenueRecognitionService.cs`
- `Beep.OilandGas.Accounting/Services/GeneralLedgerReportService.cs`
- `Beep.OilandGas.Accounting/Services/GovernmentGrantService.cs`
- `Beep.OilandGas.Accounting/Services/HyperinflationRestatementService.cs`
- `Beep.OilandGas.Accounting/Services/ImpairmentService.cs`
- `Beep.OilandGas.Accounting/Services/InsuranceContractsService.cs`
- `Beep.OilandGas.Accounting/Services/IntangibleAssetService.cs`
- `Beep.OilandGas.Accounting/Services/InterimReportingService.cs`
- `Beep.OilandGas.Accounting/Services/InventoryLcmService.cs`
- `Beep.OilandGas.Accounting/Services/InventoryService.CompanySources.cs`
- `Beep.OilandGas.Accounting/Services/InventoryService.NativeContext.cs`
- `Beep.OilandGas.Accounting/Services/InventoryService.PurchaseReceiptStock.cs`
- `Beep.OilandGas.Accounting/Services/InventoryService.ReceiptStockSources.cs`
- `Beep.OilandGas.Accounting/Services/InventoryService.StockRecovery.cs`
- `Beep.OilandGas.Accounting/Services/InventoryService.WorkOrderMaterialRelease.cs`
- `Beep.OilandGas.Accounting/Services/InventoryService.WorkOrderMaterialUsage.cs`
- `Beep.OilandGas.Accounting/Services/InventoryService.WorkOrderMaterials.cs`
- `Beep.OilandGas.Accounting/Services/InventoryService.cs`
- `Beep.OilandGas.Accounting/Services/InvestmentDisclosureService.cs`
- `Beep.OilandGas.Accounting/Services/InvestmentPropertyService.cs`
- `Beep.OilandGas.Accounting/Services/InvoiceService.CompanyDecision.cs`
- `Beep.OilandGas.Accounting/Services/InvoiceService.CompanyDraft.cs`
- `Beep.OilandGas.Accounting/Services/InvoiceService.CompanyDraftIntent.cs`
- `Beep.OilandGas.Accounting/Services/InvoiceService.CompanyQueue.cs`
- `Beep.OilandGas.Accounting/Services/InvoiceService.CompanyReviewSource.cs`
- `Beep.OilandGas.Accounting/Services/InvoiceService.cs`
- `Beep.OilandGas.Accounting/Services/JointArrangementsService.cs`
- `Beep.OilandGas.Accounting/Services/JournalEntryService.CompanyAdmission.cs`
- `Beep.OilandGas.Accounting/Services/JournalEntryService.CompanyPostedLedger.cs`
- `Beep.OilandGas.Accounting/Services/JournalEntryService.CompanyPosting.cs`
- `Beep.OilandGas.Accounting/Services/JournalEntryService.CompanyPostingConfirmation.cs`
- `Beep.OilandGas.Accounting/Services/JournalEntryService.CompanyPostingRecovery.cs`
- `Beep.OilandGas.Accounting/Services/JournalEntryService.CompanyPostingSchema.cs`
- `Beep.OilandGas.Accounting/Services/JournalEntryService.CompanyPostingSource.cs`
- `Beep.OilandGas.Accounting/Services/JournalEntryService.CompanyPreparation.cs`
- `Beep.OilandGas.Accounting/Services/JournalEntryService.CompanyPreparationRecovery.cs`
- `Beep.OilandGas.Accounting/Services/JournalEntryService.CompanyReview.cs`
- `Beep.OilandGas.Accounting/Services/JournalEntryService.CompanySource.cs`
- `Beep.OilandGas.Accounting/Services/JournalEntryService.CompanyWorkflowSource.cs`
- `Beep.OilandGas.Accounting/Services/JournalEntryService.cs`
- `Beep.OilandGas.Accounting/Services/LeaseAccountingService.cs`
- `Beep.OilandGas.Accounting/Services/OperatingSegmentService.cs`
- `Beep.OilandGas.Accounting/Services/PerformanceObligationService.cs`
- `Beep.OilandGas.Accounting/Services/PeriodClosingService.CompanyPeriodCreation.cs`
- `Beep.OilandGas.Accounting/Services/PeriodClosingService.CompanyPeriods.cs`
- `Beep.OilandGas.Accounting/Services/PeriodClosingService.CompanyTransitions.cs`
- `Beep.OilandGas.Accounting/Services/PeriodClosingService.cs`
- `Beep.OilandGas.Accounting/Services/PresentationEnhancementService.cs`
- `Beep.OilandGas.Accounting/Services/PresentationService.cs`
- `Beep.OilandGas.Accounting/Services/ProvisionService.cs`
- `Beep.OilandGas.Accounting/Services/PurchaseOrderService.InventoryLinkCycle.cs`
- `Beep.OilandGas.Accounting/Services/PurchaseOrderService.InventoryLinkDecision.cs`
- `Beep.OilandGas.Accounting/Services/PurchaseOrderService.InventoryLinkHistory.cs`
- `Beep.OilandGas.Accounting/Services/PurchaseOrderService.InventoryLinkProposal.cs`
- `Beep.OilandGas.Accounting/Services/PurchaseOrderService.ReceiptDecision.cs`
- `Beep.OilandGas.Accounting/Services/PurchaseOrderService.cs`
- `Beep.OilandGas.Accounting/Services/ReconciliationService.cs`
- `Beep.OilandGas.Accounting/Services/RegulatoryDeferralService.cs`
- `Beep.OilandGas.Accounting/Services/RelatedPartyDisclosureService.cs`
- `Beep.OilandGas.Accounting/Services/RetirementBenefitPlanService.cs`
- `Beep.OilandGas.Accounting/Services/SeparateFinancialStatementService.cs`
- `Beep.OilandGas.Accounting/Services/ShareBasedPaymentService.cs`
- `Beep.OilandGas.Accounting/Services/SubsidiaryDisclosureService.cs`
- `Beep.OilandGas.Accounting/Services/TaxCalculationService.cs`
- `Beep.OilandGas.Accounting/Services/TaxProvisionService.cs`
- `Beep.OilandGas.Accounting/Services/TrialBalanceService.cs`
- `Beep.OilandGas.Accounting/Services/XbrlTaggingService.cs`

## Migration tasks

- [x] Record the confirmed existing owner, destination, namespace and dependency decisions in inventory.json and restoration-progress.json; current destination hashes are in observed evidence.
- [x] Keep the internal project graph acyclic and record shared-contract placements without another global data project. The current static superset audit covers 69 existing projects and 284 literal project-reference edges, with zero cycles; external symbolic references are listed separately in evidence/canonical-dependency-recovery/project-reference-audit.json.
- [ ] Move definitions once, update all listed callers and references, then remove forwarding stubs/obsolete definitions. Preserve source changes from the soft-reset commit.
- [ ] Fix CLR namespaces and ambiguous/duplicate DTO names; retain native table/key names and public JSON member names unless all consumers migrate together. Update explicit JsonPropertyName, EF/Beep mappings, entity registration, migration discovery, serializers and stored assembly-qualified type names where applicable.
- [ ] Update typed HTTP client signatures, controller request/response types, service interfaces, DI registration, Razor bindings, setup/module manifests, tests, source-linked fixtures and build scripts that name the moved source/assembly.
- [ ] Reuse existing log, audit and notification paths; check that changed exception/type names do not hide errors, lose correlation or change UI failure feedback. IdentityServer stays authentication-only and app RBAC remains authoritative.
- [ ] Update relevant docs and project/solution/package references. Remove stale centralized-ownership instructions as part of the authorized implementation.

## Verification and dependencies

Run only affected checks selected against the active master tracker. Compile the target owner and immediate consumers, then verify wire serialization, native entity/key registration, migration discovery and retained app-role/company/field boundaries. Test projects must replace source links and assembly HintPaths to moved definitions, not create test-only substitute models. Record full-host/provider/build failures separately from focused harness results. Current executed checks and unavailable integration gates are recorded in the Current implementation section and evidence/canonical-dependency-recovery/observed-evidence.json.

Depends on the common ownership/naming decision and inventory review; caller migration follows each owner group. Finish with one-definition and zero-obsolete-reference checks. Keep a move manifest so an individual slice can be reversed without resetting unrelated staged work.

## Applied module restoration - 2026-11-04

Moved files now use Beep.OilandGas.Accounting.Models. All recorded caller imports and exact qualified names were updated. Focused test source links follow owner sources. Data copies removed. Shared validators reused across assembly boundaries. Actual owner-build outcomes are tracked in restoration-progress.json; no runtime/EF/provider acceptance claimed.

- `TheTechIdea.Data/OilGas/AccountingPeriodCheckReview.cs` -> `Beep.OilandGas.Accounting/Models/AccountingPeriodCheckReview.cs`
- `TheTechIdea.Data/OilGas/APReceiptAllocationCommand.cs` -> `Beep.OilandGas.Accounting/Models/APReceiptAllocationCommand.cs`
- `TheTechIdea.Data/OilGas/CompanyAccountingCorrectionCycle.cs` -> `Beep.OilandGas.Accounting/Models/CompanyAccountingCorrectionCycle.cs`
- `TheTechIdea.Data/OilGas/CompanyGLAccountConfiguration.cs` -> `Beep.OilandGas.Accounting/Models/CompanyGLAccountConfiguration.cs`

## Domain migration checkpoint - 2026-11-04

Applied owner namespace moves below; copies removed from Data, callers and focused source links updated. Owner builds are recorded in restoration-progress.json. Direct owner builds using existing dependency outputs do not prove clean integrated builds, API/Web or provider behavior.

- `Beep.OilandGas.Accounting/Models/AccountingPeriodCheckReview.cs`: `Beep.OilandGas.Accounting.Models`
- `Beep.OilandGas.Accounting/Models/APReceiptAllocationCommand.cs`: `Beep.OilandGas.Accounting.Models`
- `Beep.OilandGas.Accounting/Models/CompanyAccountingCorrectionCycle.cs`: `Beep.OilandGas.Accounting.Models`
- `Beep.OilandGas.Accounting/Models/CompanyGLAccountConfiguration.cs`: `Beep.OilandGas.Accounting.Models`

## Dependency-group implementation - 2026-11-04

Current-owner files below migrated with namespace/caller/source-link updates. Shared contract folders follow existing feature conventions. Duplicate import result/error definitions consolidated into existing plain transport models; no forwarded or parallel result types retained. Exact build/runtime scope and integration failures are recorded in restoration-progress.json.

- `Beep.OilandGas.Accounting/Data/Accounting/ARInvoice.cs` (`Beep.OilandGas.Accounting.Models`)
- `Beep.OilandGas.Accounting/Models/CompanyAccountingProcessBinding.cs` (`Beep.OilandGas.Accounting.Models`)
- `Beep.OilandGas.Accounting/Models/CompanyGLMappingTransition.cs` (`Beep.OilandGas.Accounting.Models`)
- `Beep.OilandGas.Accounting/Models/CompanyGLMappingConfiguration.cs` (`Beep.OilandGas.Accounting.Models`)
- `Beep.OilandGas.Accounting/Models/CompanySalesInvoiceProcessSourceReview.cs` (`Beep.OilandGas.Accounting.Models`)
- `Beep.OilandGas.Accounting/Models/CompanySalesJournalProcessSourceReview.cs` (`Beep.OilandGas.Accounting.Models`)
- `Beep.OilandGas.Accounting/Models/CompanySalesPaymentWorkflowSource.cs` (`Beep.OilandGas.Accounting.Models`)
- `Beep.OilandGas.Accounting/Models/CompanySalesReceivableProcessSourceReview.cs` (`Beep.OilandGas.Accounting.Models`)
- `Beep.OilandGas.Accounting/Models/CompanySalesReceivableReviewRecovery.cs` (`Beep.OilandGas.Accounting.Models`)
- `Beep.OilandGas.Accounting/Data/Accounting/GLAccount.cs` (`Beep.OilandGas.Accounting.Models`)
- `Beep.OilandGas.Accounting/Models/ICompanyLedgerBookReader.cs` (`Beep.OilandGas.Accounting.Models`)
- `Beep.OilandGas.Accounting/Data/Accounting/JournalEntryLine.cs` (`Beep.OilandGas.Accounting.Models`)
- `Beep.OilandGas.Accounting/Data/Accounting/JournalEntryLineData.cs` (`Beep.OilandGas.Accounting.Models`)
- `Beep.OilandGas.Accounting/Data/Accounting/PaymentApplicationItem.cs` (`Beep.OilandGas.Accounting.Models`)
- `Beep.OilandGas.Accounting/Models/PurchaseOrderInventoryLinkCycle.cs` (`Beep.OilandGas.Accounting.Models`)
- `Beep.OilandGas.Accounting/Models/PurchaseOrderInventoryLinkResolution.cs` (`Beep.OilandGas.Accounting.Models`)
- `Beep.OilandGas.Accounting/Models/PurchaseReceiptDecisionReview.cs` (`Beep.OilandGas.Accounting.Models`)
- `Beep.OilandGas.Accounting/Models/PurchaseReceiptStockIntentPage.cs` (`Beep.OilandGas.Accounting.Models`)

## Compiler-bound migration checkpoint - 2026-11-04

Files below returned with callers and source links. Incident command interfaces use the consuming data-management Core.Interfaces layer; shared work-order/field facts stay in the existing Models contract layer. Owner builds pass, using existing dependency outputs. Full consumer/provider gates remain open. The retired Data validation bridge is removed. Current internal evidence-validation access is restricted to Accounting, ProductionAccounting and the existing Repository test assembly; it does not cache authority or datasource reads.

- `Beep.OilandGas.Accounting/Models/CompanySalesJournalWorkflowSource.cs` (`Beep.OilandGas.Accounting.Models`)
- `Beep.OilandGas.Accounting/Models/CompanySalesPaymentReview.cs` (`Beep.OilandGas.Accounting.Models`)
- `Beep.OilandGas.Accounting/Models/CompanySalesPayment.cs` (`Beep.OilandGas.Accounting.Models`)
- `Beep.OilandGas.Accounting/Models/PurchaseReceiptStockIntentIndex.cs` (`Beep.OilandGas.Accounting.Models`)
- `Beep.OilandGas.Accounting/Models/SALES_JOURNAL_POSTING_OUTCOME.cs` (`Beep.OilandGas.Accounting.Models`)
